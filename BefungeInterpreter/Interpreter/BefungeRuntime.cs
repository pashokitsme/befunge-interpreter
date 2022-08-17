using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Interpreter;

public class BefungeRuntime
{
	public char[][] Code => _code;
	public Stack<int> Stack => _stack;
	public Caret Caret => _caret;

	private readonly char[][] _code;
	private bool _stringMode;
	private readonly Stack<int> _stack;
	private readonly StringBuilder _output = new();

	private readonly Caret _caret;

	private int _steps;
	private Stopwatch _stopwatch = new();

	public BefungeRuntime(char[][] code)
	{
		_code = code;
		_stack = new();
		_caret = new(this);
	}

	public string Run()
	{
		_stopwatch.Start();
		foreach (var instruction in _caret)
		{
			_steps++;
			Console.WriteLine($"Executing '{instruction.Name}' at [{_caret.Position.X}, {_caret.Position.Y}]");
			if (_stringMode && instruction.Name != '"')
				Push(instruction.Name);
			else
				instruction.Execute(this);
		}
		
		_stopwatch.Stop();

		Console.WriteLine($"\n\n\nSteps: {_steps}. Elapsed: {_stopwatch.ElapsedMilliseconds}ms ({_stopwatch.ElapsedTicks}ticks)");
		return _output.ToString();
	}

	public void ToggleStringMode() => _stringMode = !_stringMode;

	public void Write<T>(T value) => _output.Append(value);

	public (int A, int B) Pop2() => (_stack.Pop(), _stack.Pop());

	public int Pop() => _stack.Pop();

	public void Push(int value) => _stack.Push(value);

	public int Peek() => _stack.Peek();
}