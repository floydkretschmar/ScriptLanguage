using System;
using System.Linq;
using FlKr.ScriptLanguage.Lexing;
using FlKr.ScriptLanguage.Parsing;

namespace FlKr.ScriptLanguage.Example;

internal class Program
{
    private static void Main(string[] args)
    {
        var code = Example1();
        Console.WriteLine("----------- Code -----------");
        Console.WriteLine(code);
        Console.WriteLine("----------- Start tokenizing -----------");
        var tokens = Lexer.Tokenize(code);
        Console.WriteLine(
            $"List of Tokens: {string.Join(',', tokens.Select(x => $"[{x.ToString()}|{x.Type}|{x.DetailType}]"))}");
        Console.WriteLine("----------- Finished tokenizing -----------");
        var parser = new Parser();
        Console.WriteLine("----------- Start parsing -----------");
        var func = parser.Parse<double>(tokens);
        Console.WriteLine("----------- Finished parsing -----------");
        var value = func();
        Console.WriteLine($"Result: {value}");
    }

    private static string Example1()
    {
        return @"
A ist falsch.

wenn A und 1 + 1 = 2 mache {
    C ist 3.
    ergebnis C.
} sonst wenn 1 + 2 = 3 mache {
    ergebnis 17.
} sonst ergebnis 4.";
    }
}