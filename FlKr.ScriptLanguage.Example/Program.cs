using System;
using System.Collections.Generic;
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
// wenn A + B gleich 7 und nicht wahrheit dann
// C ist A + 5 sowie
// ergebnis C
// sonst
// ergebnis A + B.");
//             List<IToken> tokens = Lexer.Tokenize(@"
// A ist (3 + (1 - 2)).");
            List<IToken> tokens = Lexer.Tokenize(@"
A ist -(3 + -(1 - --2)).");
            Parser parser = new Parser();
            Action func = parser.Parse(tokens);
            func();
        }
    }
}