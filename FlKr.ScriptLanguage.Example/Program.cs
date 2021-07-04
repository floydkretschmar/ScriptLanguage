using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using FlKr.ScriptLanguage.Lexing;
using FlKr.ScriptLanguage.Lexing.Tokens;
using FlKr.ScriptLanguage.Parsing;

namespace FlKr.ScriptLanguage.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            var code = Example1();
            Console.WriteLine("----------- Code -----------");
            Console.WriteLine(code);
            Console.WriteLine("----------- Start tokenizing -----------");
            List<IToken> tokens = Lexer.Tokenize(code);
            Console.WriteLine($"List of Tokens: {string.Join(',', tokens.Select(x => $"[{x.ToString()}|{x.Type}|{x.DetailType}]"))}");
            Console.WriteLine("----------- Finished tokenizing -----------");
            Parser parser = new Parser();
            Console.WriteLine("----------- Start parsing -----------");
            Func<double> func = parser.Parse<double>(tokens);
            Console.WriteLine("----------- Finished parsing -----------");
            var value = func();
            Console.WriteLine($"Result: {value}");
        }

        static string Example1()
        {
            return @"
A ist wahr.

wenn A und 1 + 1 = 2 mache {
    C ist 3.
    ergebnis C.
} sonst ergebnis 4.";
        }
    }
}