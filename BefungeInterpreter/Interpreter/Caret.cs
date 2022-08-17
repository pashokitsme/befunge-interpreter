using System;
using System.Collections;
using System.Collections.Generic;

namespace Interpreter;

public struct Vector2
{
	public int X;
	public int Y;

	public Vector2(int x, int y)
	{
		X = x;
		Y = y;
	}

	public static Vector2 operator +(Vector2 a, Vector2 b) => new(a.X + b.X, a.Y + b.Y);
	public static Vector2 Up => new(-1, 0);
	public static Vector2 Down => new(1, 0);
	public static Vector2 Right => new(0, 1);
	public static Vector2 Left => new(0, -1);
	public static Vector2 Zero => new(0, 0);
}

public class Caret : IEnumerable<Instruction>, IEnumerator<Instruction>
{
	public Vector2 Direction { get; set; } = Vector2.Right;
	public Vector2 Position => _position;
	
	public Instruction Current => _current ??= GetInstruction();
	object? IEnumerator.Current => _current;
	
	private Instruction? _current;
	private readonly BefungeRuntime _runtime;
	private Vector2 _position = new(0, -1);
	private Vector2 _size;

	public Caret(BefungeRuntime runtime)
	{
		_runtime = runtime;
		_size = new(runtime.Code.Length, runtime.Code[0].Length);
	}

	private Instruction GetInstruction() => Instruction.GetByName(_runtime.Code[_position.X][_position.Y]);

	public bool MoveNext()
	{
		_current = null;
		var t = _position + Direction;
		
		if (t.X >= _size.X) t.X -= _size.X;
		if (t.X < 0) t.X = _size.X - 1;
		if (t.Y >= _size.Y) t.Y -= _size.Y;
		if (t.Y < 0) t.Y = _size.Y - 1;
		if (t.X != _position.X) _size.Y = _runtime.Code[t.X].Length;
		
		_position = t;
		return Current.Name != '@';
	}

	public void Reset()
	{
		Direction = Vector2.Right;
		_position = Vector2.Zero;
	}

	public void Dispose() => GC.SuppressFinalize(this);

	public IEnumerator<Instruction> GetEnumerator() => this;

	IEnumerator IEnumerable.GetEnumerator() => this;

}