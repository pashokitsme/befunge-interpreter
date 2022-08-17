using System;
using Interpreter;

var befunge = new BefungeInterpreter();
// var result = befunge.Interpret(">987v>.v\nv456<  :\n>321 ^ _@");
var result = befunge.Interpret("01->1# +# :# 0# g# ,# :# 5# 8# *# 4# +# -# _@)");
Console.WriteLine("\n___");
Console.WriteLine(result);