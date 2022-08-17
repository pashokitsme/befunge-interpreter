using Interpreter;
using NUnit.Framework;

namespace InterpreterTests;

public class Tests
{
	[Test]
	public void Basic() => Assert.AreEqual("123456789", new BefungeInterpreter().Interpret(">987v>.v\nv456<  :\n>321 ^ _@"));
}