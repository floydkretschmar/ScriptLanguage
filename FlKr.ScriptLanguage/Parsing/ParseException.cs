using System;
using System.Collections.Generic;
using System.Linq;
using FlKr.ScriptLanguage.Base;

namespace FlKr.ScriptLanguage.Parsing
{
    public class ParseException : Exception
    {
        public ParseException(List<Token> tokens) : base(string.Join(" ", tokens.Select(x => x.Value)))
        {
        }
        
        public ParseException(List<Token> tokens, string message) : base($"{message}: {string.Join(" ", tokens.Select(x => x.Value))}")
        {
        }

        public ParseException(string? message) : base(message)
        {
        }
    }
}