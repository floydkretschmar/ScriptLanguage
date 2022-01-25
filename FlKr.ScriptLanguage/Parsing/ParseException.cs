using System;
using System.Collections.Generic;
using System.Linq;
using FlKr.ScriptLanguage.Lexing.Tokens;

namespace FlKr.ScriptLanguage.Parsing
{
    public class ParseException : Exception
    {
        public ParseException(List<IToken> tokens, string message) : base(
            $"{message}: {string.Join(" ", tokens.Select(x => x.ToString()))}")
        {
        }

        public ParseException(string message) : base(message)
        {
        }
    }
}