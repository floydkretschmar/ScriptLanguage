using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FlKr.ScriptLanguage.Lexing.Tokens;

namespace FlKr.ScriptLanguage.Lexing
{
    public class Lexer
    {
        private static List<TokenDefinition> _tokenDefinitions = new List<TokenDefinition>()
        {
            new TokenDefinition(@"^\s", TokenDetailTypes.Whitespace, TokenTypes.Syntax),
            new TokenDefinition(@"^\.", TokenDetailTypes.EndOfLine, TokenTypes.Syntax),
            new TokenDefinition(@"^,", TokenDetailTypes.EndOfLineBlock, TokenTypes.Syntax),
            new TokenDefinition(@"^\(", TokenDetailTypes.LeftBracket, TokenTypes.Syntax),
            new TokenDefinition(@"^\)", TokenDetailTypes.RightBracket, TokenTypes.Syntax),

            new TokenDefinition(@"^'\b(\w+?)\b'", TokenDetailTypes.Text, TokenTypes.Value),
            new TokenDefinition(@"^\b(wahr)\b", TokenDetailTypes.True, TokenTypes.Value),
            new TokenDefinition(@"^\b(falsch)\b", TokenDetailTypes.False, TokenTypes.Value),
            new TokenDefinition(@"^[0-9]+(,[0-9]+)?", TokenDetailTypes.Number, TokenTypes.Value),

            new TokenDefinition(@"^\+", TokenDetailTypes.Addition, TokenTypes.MathOperation),
            new TokenDefinition(@"^-", TokenDetailTypes.Subtraction, TokenTypes.MathOperation),
            new TokenDefinition(@"^/", TokenDetailTypes.Division, TokenTypes.MathOperation),
            new TokenDefinition(@"^\*", TokenDetailTypes.Multiplication, TokenTypes.MathOperation),
            new TokenDefinition(@"^%", TokenDetailTypes.Modulo, TokenTypes.MathOperation),
            new TokenDefinition(@"^\^", TokenDetailTypes.Exponentiation, TokenTypes.MathOperation),

            new TokenDefinition(@"^=", TokenDetailTypes.Equals, TokenTypes.LogicOperation),
            new TokenDefinition(@"^>=", TokenDetailTypes.GreaterEqualThan, TokenTypes.LogicOperation),
            new TokenDefinition(@"^>", TokenDetailTypes.GreaterThan, TokenTypes.LogicOperation),
            new TokenDefinition(@"^<=", TokenDetailTypes.LessEqualThan, TokenTypes.LogicOperation),
            new TokenDefinition(@"^<", TokenDetailTypes.LessThan, TokenTypes.LogicOperation),
            new TokenDefinition(@"^\b(nicht)\b", TokenDetailTypes.Not, TokenTypes.LogicOperation),
            new TokenDefinition(@"^\b(und)\b", TokenDetailTypes.And, TokenTypes.LogicOperation),
            new TokenDefinition(@"^\b(oder)\b", TokenDetailTypes.Or, TokenTypes.LogicOperation),

            new TokenDefinition(@"^\b(wenn)\b", TokenDetailTypes.If, TokenTypes.ControlFlow),
            new TokenDefinition(@"^\b(dann)\b", TokenDetailTypes.Then, TokenTypes.ControlFlow),
            new TokenDefinition(@"^\b(sonst)\b", TokenDetailTypes.Else, TokenTypes.ControlFlow),
            new TokenDefinition(@"^ergebnis", TokenDetailTypes.Return, TokenTypes.ControlFlow),

            new TokenDefinition(@"^ist", TokenDetailTypes.Assignment, TokenTypes.Variable),
            new TokenDefinition(@"^\b(\w+?)\b", TokenDetailTypes.VariableName, TokenTypes.Variable)
        };

        public static List<IToken> Tokenize(string code)
        {
            List<IToken> tokens = new List<IToken>();
            var remainingCode = code;

            while (remainingCode != string.Empty)
            {
                Token token = null;
                foreach (var tokenDefinition in _tokenDefinitions)
                {
                    var match = Regex.Match(remainingCode, tokenDefinition.Pattern);
                    if (match.Success)
                    {
                        var value = match.Captures.First().Value;
                        remainingCode = remainingCode.Remove(0, value.Length);
                        TokenDetailTypes detailTypes = tokenDefinition.DetailType;

                        // if is a subtraction operator but:
                        // 1. its the first token
                        // 2. the previous token was neither a variable nor a value
                        // => it is a unary negative instead
                        if (tokenDefinition.DetailType == TokenDetailTypes.Subtraction &&
                            (tokens.Count == 0 || (tokens.Last().DetailType != TokenDetailTypes.VariableName &&
                                                   tokens.Last().Type != TokenTypes.Value)))
                            detailTypes = TokenDetailTypes.Negative;

                        token = new Token()
                        {
                            Value = value.Trim(),
                            DetailType = detailTypes,
                            Type = tokenDefinition.Type
                        };

                        break;
                    }
                }

                if (token.DetailType != TokenDetailTypes.Whitespace)
                    tokens.Add(token);
            }

            return tokens;
        }
    }
}