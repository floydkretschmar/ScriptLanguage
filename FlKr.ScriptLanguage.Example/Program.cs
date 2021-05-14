using System;
using System.Collections.Generic;
using System.Diagnostics;
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
//             List<IToken> tokens = Lexer.Tokenize(@"
// A ist 3. 
// B ist 4. 
// wahrheit ist falsch.
//
// wenn A + B = 7 und nicht wahrheit dann
// C ist A + 5,
// ergebnis C
// sonst
// ergebnis A + B.");
//             List<IToken> tokens = Lexer.Tokenize(@"
// A ist (3 + (1 - 2)).");
//             List<IToken> tokens = Lexer.Tokenize(@"
// A ist -(3,0 + -(1 - --2) * 2).
// B ist 10 / 3.
//
// ergebnis A + B.");
            List<IToken> tokens = Lexer.Tokenize(@"
A ist falsch.

wenn A und 1 + 1 = 2 dann
C ist 3.
ergebnis C.
sonst
ergebnis 4.
machen.");
            Parser parser = new Parser();
            Func<double> func = parser.Parse<double>(tokens);
            var value = func();
            Debug.Write(value);
        }
    }
}