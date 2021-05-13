﻿using System;
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
// wenn A + B gleich 7 und nicht wahrheit dann
// C ist A + 5 sowie
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
A ist 1 + 2 <= 5 und falsch.
B ist nicht 1 + 1 = 2.

ergebnis A oder B.");
            Parser parser = new Parser();
            Func<bool> func = parser.Parse<bool>(tokens);
            var value = func();
            Debug.Write(value);
        }
    }
}