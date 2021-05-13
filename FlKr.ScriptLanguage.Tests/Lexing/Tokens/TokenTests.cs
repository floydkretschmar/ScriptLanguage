using System.Collections.Generic;
using FlKr.ScriptLanguage.Lexing.Tokens;
using NUnit.Framework;

namespace FlKr.ScriptLanguage.Tests.Lexing.Tokens
{
    [TestFixture]
    public class TokenTests
    {
        [Test]
        public void ToString_WhenCalled_ReturnStringRepresentation()
        {
            Token token = new Token() {Value = "1"};

            var result = token.ToString();
            
            Assert.That(result, Is.EqualTo("1"));
        }
    }   
}