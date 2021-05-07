using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using FlKr.ScriptLanguage.Base;
using FlKr.ScriptLanguage.Lexing;
using FlKr.ScriptLanguage.Parsing;

namespace FlKr.ScriptLanguage.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            List<Token> tokens = Lexer.Tokenize(@"
A ist 3. 
B ist 4. 
wahrheit ist falsch.

wenn A + B gleich 7 und nicht wahrheit dann
C ist A + 5 sowie
ergebnis C
sonst
ergebnis A + B.");
            Parser parser = new Parser();
            LambdaExpression func = parser.Parse(tokens);
        }
    }
}