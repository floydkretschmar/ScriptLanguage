using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FlKr.ScriptLanguage.Lexing.Tokens;

namespace FlKr.ScriptLanguage.Lexing;

public class Lexer
{
    private static readonly List<TokenDefinition> _tokenDefinitions = new()
    {
        new(@"^\s", TokenDetailTypes.Whitespace, TokenTypes.Syntax),
        new(@"^\.", TokenDetailTypes.EndOfLine, TokenTypes.Syntax),
        new(@"^\(", TokenDetailTypes.LeftBracket, TokenTypes.Syntax),
        new(@"^\)", TokenDetailTypes.RightBracket, TokenTypes.Syntax),

        new(@"^'\b(\w+?)\b'", TokenDetailTypes.Text, TokenTypes.Value),
        new(@"^\b(wahr)\b", TokenDetailTypes.True, TokenTypes.Value),
        new(@"^\b(falsch)\b", TokenDetailTypes.False, TokenTypes.Value),
        new(@"^[0-9]+(,[0-9]+)?", TokenDetailTypes.Number, TokenTypes.Value),

        new(@"^\+", TokenDetailTypes.Addition, TokenTypes.MathOperation),
        new(@"^-", TokenDetailTypes.Subtraction, TokenTypes.MathOperation),
        new(@"^/", TokenDetailTypes.Division, TokenTypes.MathOperation),
        new(@"^\*", TokenDetailTypes.Multiplication, TokenTypes.MathOperation),
        new(@"^%", TokenDetailTypes.Modulo, TokenTypes.MathOperation),
        new(@"^\^", TokenDetailTypes.Exponentiation, TokenTypes.MathOperation),

        new(@"^=", TokenDetailTypes.Equals, TokenTypes.LogicOperation),
        new(@"^>=", TokenDetailTypes.GreaterEqualThan, TokenTypes.LogicOperation),
        new(@"^>", TokenDetailTypes.GreaterThan, TokenTypes.LogicOperation),
        new(@"^<=", TokenDetailTypes.LessEqualThan, TokenTypes.LogicOperation),
        new(@"^<", TokenDetailTypes.LessThan, TokenTypes.LogicOperation),
        new(@"^\b(nicht)\b", TokenDetailTypes.Not, TokenTypes.LogicOperation),
        new(@"^\b(und)\b", TokenDetailTypes.And, TokenTypes.LogicOperation),
        new(@"^\b(oder)\b", TokenDetailTypes.Or, TokenTypes.LogicOperation),

        new(@"^\b(sonst)\b \b(wenn)\b", TokenDetailTypes.ElseIf, TokenTypes.ControlFlow),
        new(@"^\b(wenn)\b", TokenDetailTypes.If, TokenTypes.ControlFlow),
        new(@"^\b(sonst)\b", TokenDetailTypes.Else, TokenTypes.ControlFlow),
        new(@"^\b(mache)\b", TokenDetailTypes.Do, TokenTypes.ControlFlow),
        new(@"^{", TokenDetailTypes.BeginBlock, TokenTypes.ControlFlow),
        new(@"^}", TokenDetailTypes.EndBlock, TokenTypes.ControlFlow),
        new(@"^ergebnis", TokenDetailTypes.Return, TokenTypes.ControlFlow),

        new(@"^ist", TokenDetailTypes.Assignment, TokenTypes.Variable),
        new(@"^\b(\w+?)\b", TokenDetailTypes.VariableName, TokenTypes.Variable)
    };

    public static List<IToken> Tokenize(string code)
    {
        var tokens = new List<IToken>();
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
                    var detailTypes = tokenDefinition.DetailType;

                    // if is a subtraction operator but:
                    // 1. its the first token
                    // 2. the previous token was neither a variable nor a value
                    // => it is a unary negative instead
                    if (tokenDefinition.DetailType == TokenDetailTypes.Subtraction &&
                        (tokens.Count == 0 || tokens.Last().DetailType != TokenDetailTypes.VariableName &&
                            tokens.Last().Type != TokenTypes.Value))
                        detailTypes = TokenDetailTypes.Negative;

                    token = new Token
                    {
                        Value = value.Trim(),
                        DetailType = detailTypes,
                        Type = tokenDefinition.Type
                    };

                    break;
                }
            }

            if (token == null)
                remainingCode = remainingCode.Remove(0, 1);
            else if (token.DetailType != TokenDetailTypes.Whitespace)
                tokens.Add(token);
        }

        return tokens;
    }
}