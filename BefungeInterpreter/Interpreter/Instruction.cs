using System;
using System.Collections.Generic;

namespace Interpreter;

public class Instruction
{
	private static readonly Dictionary<char, Instruction> _instructions = new();

	static Instruction()
	{
		_instructions
			.New(' ', _ => { })
			.New('@', _ => { })
			.New('>', runtime => runtime.Caret.Direction = Vector2.Right)
			.New('<', runtime => runtime.Caret.Direction = Vector2.Left)
			.New('^', runtime => runtime.Caret.Direction = Vector2.Up)
			.New('v', runtime => runtime.Caret.Direction = Vector2.Down)
			.New('_', runtime => runtime.Caret.Direction = runtime.Pop() == 0 ? Vector2.Right : Vector2.Left)
			.New('|', runtime => runtime.Caret.Direction = runtime.Pop() == 0 ? Vector2.Down : Vector2.Up)
			.New('?', runtime => runtime.Caret.Direction = Random.Shared.Next(4) switch
			{
				0 => Vector2.Down,
				1 => Vector2.Up,
				2 => Vector2.Left,
				3 => Vector2.Right,
				var _ => Vector2.Right
			})
			.New('#', runtime => runtime.Caret.MoveNext())
			.New('+', runtime =>
			{
				var t = runtime.Pop2();
				runtime.Push(t.A + t.B);
			})
			.New('-', runtime =>
			{
				var t = runtime.Pop2();
				runtime.Push(t.B - t.A);
			})
			.New('*', runtime =>
			{
				var t = runtime.Pop2();
				runtime.Push(t.A * t.B);
			})
			.New('/', runtime =>
			{
				var t = runtime.Pop2();
				runtime.Push(t.A == 0 ? 0 : t.B / t.A);
			})
			.New('%', runtime =>
			{
				var t = runtime.Pop2();
				runtime.Push(t.A == 0 ? 0 : t.B % t.A);
			})
			.New(':', runtime => runtime.Push(runtime.Stack.TryPeek(out var value) ? value : 0))
			.New('\\', runtime =>
			{
				var stack = runtime.Stack;
				var a = stack.Pop();
				if (stack.TryPop(out var b))
				{
					runtime.Push(a);
					runtime.Push(b);
					return;
				}
				
				runtime.Push(0);
				runtime.Push(a);
			})
			.New('!', runtime => runtime.Push(runtime.Pop() == 0 ? 1 : 0))
			.New('$', runtime => runtime.Pop())
			.New('`', runtime =>
			{
				var t = runtime.Pop2();
				runtime.Push(t.B > t.A ? 1 : 0);
			})
			.New('.', runtime => runtime.Write(runtime.Pop()))
			.New('"', runtime => runtime.ToggleStringMode())
			.New(',', runtime => runtime.Write((char)runtime.Pop()))
			.New('p', runtime =>
			{
				var y = runtime.Pop();
				var x = runtime.Pop();
				var value = runtime.Pop();
				runtime.Code[y][x] = (char)value;
			})
			.New('g', runtime =>
			{
				var y = runtime.Pop();
				var x = runtime.Pop();
				runtime.Push(runtime.Code[y][x]);
			});

		for (var i = 0; i < 10; i++)
		{
			var j = i;
			_instructions.New((char) (i + '0'), runtime => runtime.Push(j));
		}
	}
	
	public static Instruction GetByName(char name) => _instructions.ContainsKey(name) ? _instructions[name] : new(name, _ => { });

	public char Name => _name;
	
	private char _name;
	private Action<BefungeRuntime> _action;

	public Instruction(char name, Action<BefungeRuntime> action)
	{
		_name = name;
		_action = action;
	}
	
	public void Execute(BefungeRuntime runtime) => _action(runtime);
}

public static class DictionaryExtensions
{
	public static Dictionary<char, Instruction> New(this Dictionary<char, Instruction> dictionary, char name, Action<BefungeRuntime> action)
	{
		dictionary.Add(name, new(name, action));
		return dictionary;
	}
}
