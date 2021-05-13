using System.Collections.Generic;
using FlKr.ScriptLanguage.Lexing.Tokens;
using NUnit.Framework;

namespace FlKr.ScriptLanguage.Tests.Lexing.Tokens
{
    [TestFixture]
    public class ExpressionTokenTests
    {
        [Test]
        public void ToString_WhenCalled_ReturnStringRepresentation()
        {
            List<IToken> expression = new List<IToken>()
                {new Token() {Value = "1"}, new Token() {Value = "2"}, new Token() {Value = "3"}};
            ExpressionToken token = new ExpressionToken() {Expression = expression};

            var result = token.ToString();
            
            Assert.That(result, Is.EqualTo("1 2 3"));
        }
    }   
}