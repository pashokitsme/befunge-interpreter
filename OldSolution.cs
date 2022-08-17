using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

public static class CursorExtensions
{
    public static void Right(this ref Cursor cursor) => cursor.Right(ref cursor);

    public static void Left(this ref Cursor cursor) => cursor.Left(ref cursor);

    public static void Up(this ref Cursor cursor) => cursor.Up(ref cursor);

    public static void Down(this ref Cursor cursor) => cursor.Down(ref cursor);

    public static void Last(this ref Cursor cursor)
    {
        cursor.LastAction(ref cursor);
        Console.WriteLine($"Moving {cursor.LastAction.Method.Name}");
    }
}

public delegate void LastAction(ref Cursor cursor);

public struct Cursor
{

    public int X;
    public int Y;

    public LastAction LastAction;

    public Cursor(int x, int y)
    {
        X = x;
        Y = y;
        LastAction = null;
    }

    public void Right(ref Cursor cursor)
    {
        cursor.X++;
        LastAction = Right;
    }

    public void Left(ref Cursor cursor)
    {
        cursor.X--;
        LastAction = Left;
    }

    public void Up(ref Cursor cursor)
    {
        cursor.Y--;
        LastAction = Up;
    }

    public void Down(ref Cursor cursor)
    {
        cursor.Y++;
        LastAction = Down;
    }
}

public class BefungeInterpreter
{
    private List<List<char>> _program;
    private Stack<int> _stack;
    private StringBuilder _output;

    private Cursor _cursor;
    private Random _random = new Random();

    private bool _stringMode;

    public string Interpret(string code)
    {
        var table = code.Split("\n");

        _cursor.LastAction = _cursor.Right;

        _program = new List<List<char>>(table.Length);
        _stack = new Stack<int>();
        _output = new StringBuilder();
        _stringMode = false;

        _program.AddRange(from line in table
                          select line.ToCharArray().ToList());

        Print();

        var watch = new Stopwatch();
        watch.Start();

        var instruction = GetInstruction();

        while (instruction != '@')
        {
            ExecuteNext(instruction);
            instruction = GetInstruction();
        }

        watch.Stop();

        Console.WriteLine($"OUTPUT: {_output}");
        Console.WriteLine($"Elapsed {watch.Elapsed} ({watch.ElapsedMilliseconds}ms)");
        return _output.ToString();
    }

    private void ExecuteNext(char instruction)
    {
        Console.WriteLine($"{_cursor.Y} {_cursor.X} {instruction}");

        if (char.IsNumber(instruction))
        {
            _stack.Push(instruction - '0');
            _cursor.Last();
            return;
        }

        if (_stringMode && instruction != '"')
        {
            _stack.Push(instruction);
            _cursor.Last();
            return;
        }

        if (char.IsWhiteSpace(instruction))
        {
            _cursor.Last();
            return;
        }

        switch (instruction)
        {
            case '>':
                _cursor.Right();
                break;

            case '<':
                _cursor.Left();
                break;

            case 'v':
                _cursor.Down();
                break;

            case '^':
                _cursor.Up();
                break;

            case '.':
                {
                    if (_stack.TryPop(out var value))
                        Out(value);
                    _cursor.Last();
                    break;
                }
            case ':':
                {
                    if (_stack.TryPeek(out var value) == false)
                        value = 0;

                    _stack.Push(value);
                    _cursor.Last();
                    break;
                }

            case '_':
                {
                    _ = _stack.TryPop(out var value);
                    if (value == 0)
                        _cursor.Right();
                    else
                        _cursor.Left();
                    break;
                }

            case '+':
                {
                    var (a, b) = GetLast2();
                    _stack.Push(a + b);
                    _cursor.Last();
                    break;
                }

            case '-':
                {
                    var (a, b) = GetLast2();
                    _stack.Push(b - a);
                    _cursor.Last();
                    break;
                }

            case '*':
                {
                    var (a, b) = GetLast2();
                    _stack.Push(a * b);
                    _cursor.Last();
                    break;
                }

            case '/':
                {
                    var (a, b) = GetLast2();
                    if (a == 0)
                        _stack.Push(0);
                    else
                        _stack.Push(b / a);
                    _cursor.Last();
                    break;
                }

            case '%':
                {
                    var (a, b) = GetLast2();
                    _stack.Push(b % a);
                    _cursor.Last();
                    break;
                }
            case '!':
                {
                    var value = _stack.Pop();
                    PushCond(value == 0);
                    _cursor.Last();
                    break;
                }

            case '`':
                {
                    var (a, b) = GetLast2();
                    PushCond(a < b);
                    _cursor.Last();
                    break;
                }

            case '|':
                {
                    _ = _stack.TryPop(out var v);
                    if (v == 0)
                        _cursor.Down();
                    else
                        _cursor.Up();
                    _cursor.Last();
                    break;
                }
            case '\\':
                {
                    var a = _stack.Pop();
                    var b = _stack.Pop();
                    _stack.Push(a);
                    _stack.Push(b);
                    _cursor.Last();
                    break;
                }

            case '$':
                {
                    _stack.TryPop(out _);
                    _cursor.Last();
                    break;
                }
            case '#':
                {
                    _cursor.Last();
                    _cursor.Last();
                    break;
                }
            case '?':
                {
                    MoveRandom(_random.Next(0, 4));
                    break;
                }
            case 'g':
                {
                    var (a, b) = GetLast2();
                    var inst = GetInstruction(a, b);
                    _stack.Push(inst);
                    _cursor.Last();
                    break;
                }
            case 'p':
                {
                    var y = _stack.Pop();
                    var x = _stack.Pop();
                    var value = _stack.Pop();
                    _program[y][x] = (char)value;
                    _cursor.Last();
                    break;
                }
            case ',':
                {
                    Out((char)_stack.Pop());
                    _cursor.Last();
                    break;
                }
            case '"':
                {
                    _stringMode = !_stringMode;
                    _cursor.Last();
                    break;
                }
            default:
                throw new ArgumentException(instruction.ToString());
        }
    }

    private void MoveRandom(int a)
    {
        if (a == 0)
            _cursor.Up();
        if (a == 1)
            _cursor.Left();
        if (a == 2)
            _cursor.Right();
        if (a == 3)
            _cursor.Down();
    }

    private void PushCond(bool condition)
    {
        if (condition)
            _stack.Push(1);
        else
            _stack.Push(0);
    }

    private (int a, int b) GetLast2()
    {
        var a = _stack.Pop();
        var b = _stack.Pop();

        return (a, b);
    }

    private void Out(int value) => _output.Append(value);
    private void Out(char value) => _output.Append(value);

    private char GetInstruction()
    {
        while (_cursor.Y >= _program.Count)
            _cursor.Y -= _program.Count;

        if (_cursor.Y < 0)
            _cursor.Y = _program.Count - (_cursor.Y * -1);

        while (_cursor.X >= _program[_cursor.Y].Count)
            _cursor.X -= _program[_cursor.Y].Count;

        if (_cursor.X < 0)
            _cursor.X = _program[_cursor.Y].Count - (_cursor.X * -1);

        return _program[_cursor.Y][_cursor.X];
    }

    private char GetInstruction(int y, int x)
    {
        while (y >= _program.Count)
            y -= _program.Count;

        if (y < 0)
            y = _program.Count - (y * -1);

        while (x >= _program[y].Count)
            x -= _program[y].Count;

        if (x < 0)
            x = _program[y].Count - (x * -1);

        return _program[y][x];
    }

    private void Print()
    {
        foreach (var line in _program)
        {
            foreach (var instruction in line)
                Console.Write(instruction);

            Console.Write("\n");
        }
        Console.Write("\n");
    }
}
