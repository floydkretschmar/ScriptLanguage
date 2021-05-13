using FlKr.ScriptLanguage.Lexing;
using FlKr.ScriptLanguage.Parsing;
using NUnit.Framework;

namespace FlKr.ScriptLanguage.Tests.Parsing
{
    [TestFixture]
    public class ParserTests
    {
        private Parser _parser;
        
        [SetUp]
        public void SetUp()
        {
            _parser = new Parser();
        }

        [Test]
        [TestCase("1 + 1", 2)]
        [TestCase("2 * 2", 4)]
        [TestCase("1 + 2 / 4", 1.5)]
        [TestCase("(2,5 - 1) * 2", 3)]
        [TestCase("10 % 3", 1)]
        [TestCase("2^2", 4)]
        [TestCase("-2^2", -4)]
        public void Parse_MathOperations_ReturnsSpecifiedExpression(string expression, double expectedResult)
        {
            var script = $"ergebnis {expression}.";

            var func = _parser.Parse<double>(Lexer.Tokenize(script));
            var result = func();
            
            Assert.That(result, Is.EqualTo(expectedResult));
        }
    }
}