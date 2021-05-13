using System;

namespace FlKr.ScriptLanguage.Lexing.Tokens
{
    public interface IToken : IFormattable
    {
        TokenTypes Type { get; }
        
        TokenDetailTypes DetailType { get; }
    }
}