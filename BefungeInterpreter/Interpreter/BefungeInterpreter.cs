using System.Linq;

namespace Interpreter;

public class BefungeInterpreter
{
	public string Interpret(string rawCode)
	{
		var runtime = new BefungeRuntime((from line in rawCode.Split('\n') select line.ToCharArray()).ToArray());
		return runtime.Run();
	}
}