using FlKr.ScriptLanguage.Lexing;
using FlKr.ScriptLanguage.Lexing.Tokens;
using NUnit.Framework;

namespace FlKr.ScriptLanguage.Tests.Lexing
{
    [TestFixture]
    public class LexerTests
    {
        [Test]
        [TestCase(".", TokenTypes.Syntax, TokenDetailTypes.EndOfLine)]
        [TestCase("(", TokenTypes.Syntax, TokenDetailTypes.LeftBracket)]
        [TestCase(")", TokenTypes.Syntax, TokenDetailTypes.RightBracket)]
        [TestCase("'abc'", TokenTypes.Value, TokenDetailTypes.Text)]
        [TestCase("1", TokenTypes.Value, TokenDetailTypes.Number)]
        [TestCase("wahr", TokenTypes.Value, TokenDetailTypes.True)]
        [TestCase("falsch", TokenTypes.Value, TokenDetailTypes.False)]
        [TestCase("+", TokenTypes.MathOperation, TokenDetailTypes.Addition)]
        [TestCase("-", TokenTypes.MathOperation, TokenDetailTypes.Negative)]
        [TestCase("/", TokenTypes.MathOperation, TokenDetailTypes.Division)]
        [TestCase("*", TokenTypes.MathOperation, TokenDetailTypes.Multiplication)]
        [TestCase("%", TokenTypes.MathOperation, TokenDetailTypes.Modulo)]
        [TestCase("^", TokenTypes.MathOperation, TokenDetailTypes.Exponentiation)]
        [TestCase("=", TokenTypes.LogicOperation, TokenDetailTypes.Equals)]
        [TestCase(">=", TokenTypes.LogicOperation, TokenDetailTypes.GreaterEqualThan)]
        [TestCase(">", TokenTypes.LogicOperation, TokenDetailTypes.GreaterThan)]
        [TestCase("<=", TokenTypes.LogicOperation, TokenDetailTypes.LessEqualThan)]
        [TestCase("<", TokenTypes.LogicOperation, TokenDetailTypes.LessThan)]
        [TestCase("nicht", TokenTypes.LogicOperation, TokenDetailTypes.Not)]
        [TestCase("und", TokenTypes.LogicOperation, TokenDetailTypes.And)]
        [TestCase("oder", TokenTypes.LogicOperation, TokenDetailTypes.Or)]
        [TestCase("wenn", TokenTypes.ControlFlow, TokenDetailTypes.If)]
        [TestCase("mache", TokenTypes.ControlFlow, TokenDetailTypes.Do)]
        [TestCase("sonst", TokenTypes.ControlFlow, TokenDetailTypes.Else)]
        [TestCase("sonst wenn", TokenTypes.ControlFlow, TokenDetailTypes.ElseIf)]
        [TestCase("ergebnis", TokenTypes.ControlFlow, TokenDetailTypes.Return)]
        [TestCase("ist", TokenTypes.Variable, TokenDetailTypes.Assignment)]
        [TestCase("abc", TokenTypes.Variable, TokenDetailTypes.VariableName)]
        public void Tokenize_SingleTokenParsing_ReturnToken(string value, TokenTypes type, TokenDetailTypes detailType)
        {
            var expectedToken = new Token()
            {
                Type = type,
                Value = value,
                DetailType = detailType
            };
            
            var result = Lexer.Tokenize(value);
            
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0], Is.TypeOf<Token>());

            var token = (Token) result[0];
            Assert.That(token.Type, Is.EqualTo(expectedToken.Type));
            Assert.That(token.DetailType, Is.EqualTo(expectedToken.DetailType));
            Assert.That(token.Value, Is.EqualTo(expectedToken.Value));
        }
        
        [Test]
        public void Tokenize_SingleWhitespaceParsing_ReturnEmptyList()
        {
            var result = Lexer.Tokenize(" ");
            
            Assert.That(result.Count, Is.Zero);
        }
    }
}