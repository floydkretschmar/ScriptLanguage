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
        [TestCase("(2,5 - (1 - 2)) * 2", 7)]
        [TestCase("10 % 3", 1)]
        [TestCase("2^2", 4)]
        [TestCase("-2^2", -4)]
        [TestCase("-(2+3)^2", -25)]
        public void Parse_ReturnMathOperation_ReturnsOperationResult(string expression, double expectedResult)
        {
            var script = $"ergebnis {expression}.";

            var func = _parser.Parse<double>(Lexer.Tokenize(script));
            var result = func();
            
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        [Test]
        [TestCase("1 + 1 = 2", true)]
        [TestCase("1 < 2", true)]
        [TestCase("1 <= 2 oder 3 >= 4", true)]
        [TestCase("1 <= 2 und 3 >= 4", false)]
        [TestCase("1 + 1 = 3", false)]
        [TestCase("1 > 2", false)]
        [TestCase("wahr und falsch", false)]
        [TestCase("nicht wahr", false)]
        public void Parse_ReturnLogicOperation_ReturnsOperationResult(string expression, bool expectedResult)
        {
            var script = $"ergebnis {expression}.";

            var func = _parser.Parse<bool>(Lexer.Tokenize(script));
            var result = func();
            
            Assert.That(result, Is.EqualTo(expectedResult));
        }
        
        [Test]
        public void Parse_ReturnText_ReturnsText()
        {
            var script = $"ergebnis 'Test'.";

            var func = _parser.Parse<string>(Lexer.Tokenize(script));
            var result = func();
            
            Assert.That(result, Is.EqualTo("'Test'"));
        }
        
        [Test]
        public void Parse_IfThen_ReturnsConditionalResult()
        {
            var script = @"
wenn 1 + 1 = 2 und wahr mache {
C ist 3.
ergebnis C.
} sonst ergebnis 4.";

            var func = _parser.Parse<double>(Lexer.Tokenize(script));
            var result = func();
            
            Assert.That(result, Is.EqualTo(3));
        }
        
        [Test]
        public void Parse_IfThenElse_ReturnsConditionalResult()
        {
            var script = @"
wenn falsch mache {
C ist 3.
ergebnis C.
} sonst ergebnis 4.";

            var func = _parser.Parse<double>(Lexer.Tokenize(script));
            var result = func();
            
            Assert.That(result, Is.EqualTo(4));
        }
        
        [Test]
        public void Parse_NestedIfThen_ReturnsConditionalResult()
        {
            var script = @"
wenn 1 + 1 = 2 und wahr mache {
    wenn 2 + 2 = 4 mache {
        ergebnis 3.
    }.
} sonst ergebnis 4.";

            var func = _parser.Parse<double>(Lexer.Tokenize(script));
            var result = func();
            
            Assert.That(result, Is.EqualTo(3));
        }
        
        [Test]
        public void Parse_IfThenElseNoBlocks_ReturnsConditionalResult()
        {
            var script = @"
A ist 3.
B ist wahr.
wenn B mache ergebnis A.
sonst ergebnis 4.";

            var func = _parser.Parse<double>(Lexer.Tokenize(script));
            var result = func();
            
            Assert.That(result, Is.EqualTo(3));
        }
        
        [Test]
        public void Parse_IfThenElseBlock_ReturnsConditionalResult()
        {
            var script = @"
A ist 3.
B ist falsch.
wenn B mache ergebnis A.
sonst { 
C ist A + 5.
ergebnis C.
}.";

            var func = _parser.Parse<double>(Lexer.Tokenize(script));
            var result = func();
            
            Assert.That(result, Is.EqualTo(8));
        }
        
        [Test]
        public void Parse_IfThenElseIfElseBlock_ReturnsConditionalResult()
        {
            var script = @"
A ist 3.
B ist falsch.
wenn B mache ergebnis A.
sonst wenn A = 4 mache ergebnis 5.
sonst wenn A = 3 mache {
D ist 4.
ergebnis D + 3.
}
sonst { 
C ist A + 5.
ergebnis C.
}.";

            var func = _parser.Parse<double>(Lexer.Tokenize(script));
            var result = func();
            
            Assert.That(result, Is.EqualTo(7));
        }

        [Test]
        public void Parse_AssignmentOperation_AssignsValue()
        {
            var script = @$"
A ist 1. 
B ist 1.
ergebnis A + B.";

            var func = _parser.Parse<double>(Lexer.Tokenize(script));
            var result = func();
            
            Assert.That(result, Is.EqualTo(2));
        }

        [Test]
        [TestCase("A ist .", "at least 3 tokens")]
        [TestCase("ist 1 2.", "First token of a variable")]
        [TestCase("A 3 ist.", "Second token of a variable")]
        [TestCase("A ist ist 3.", "one assignment per expression")]
        [TestCase("A.", "Only variable expression from type Assignment")]
        public void Parse_InvalidVariableAssignmentSyntaxProvided_ThrowParseException(string script, string messageSnippet)
        {
            Assert.That(() => _parser.Parse<double>(Lexer.Tokenize(script)), Throws.TypeOf<ParseException>().With.Message.Contains(messageSnippet));
        }

        [Test]
        [TestCase("ergebnis A.", "not been declared")]
        [TestCase("ergebnis 1 2.", "Invalid value expression")]
        [TestCase("ergebnis sonst.", "valid value expression")]
        [TestCase("ergebnis.", "Return operation is invalid.")]
        [TestCase("mache ergebnis 4.", "Do can never be the leading token in a control flow expression")]
        [TestCase("sonst ergebnis 4.", "Else detected before If")]
        [TestCase("sonst wenn wahr mache ergebnis 4.", "ElseIf detected before If")]
        [TestCase("wenn a ist 3 mache ergebnis 4.", "Conditions cannot contain assignment tokens")]
        [TestCase("wenn 3 + 3 mache ergebnis 4.", "Conditions is not a valid boolean expression")]
        public void Parse_InvalidControlFlowSyntaxProvided_ThrowParseException(string script, string messageSnippet)
        {
            Assert.That(() => _parser.Parse<double>(Lexer.Tokenize(script)), Throws.TypeOf<ParseException>().With.Message.Contains(messageSnippet));
        }

        [Test]
        [TestCase("ergebnis -.", "Invalid negative numeric")]
        [TestCase("ergebnis -wahr.", "not a valid numerical")]
        [TestCase("ergebnis 1-2+wahr.", "not a valid numerical")]
        [TestCase("ergebnis wahr-wahr+wahr.", "not a valid numerical")]
        public void Parse_InvalidMathSyntaxProvided_ThrowParseException(string script, string messageSnippet)
        {
            Assert.That(() => _parser.Parse<double>(Lexer.Tokenize(script)), Throws.TypeOf<ParseException>().With.Message.Contains(messageSnippet));
        }
        
        [Test]
        [TestCase("A ist ((1 + 2).", "Unequal amount of brackets")]
        [TestCase(".", "Invalid")]
        [TestCase("A ist 3", "Invalid")]
        [TestCase("3 + 3.", "can be used as statements")]
        [TestCase("(3 + 3).", "can be used as statements")]
        [TestCase("-3.", "can be used as statements")]
        [TestCase("und 3.", "can be used as statements")]
        [TestCase("ergebnis a", "Invalid end of statement.")]
        [TestCase("ergebnis", "Invalid statement.")]
        [TestCase("{{ a ist 3 }", "Unequal amount of block start and end tokens detected in expression.")]
        public void Parse_InvalidGeneralSyntaxProvided_ThrowParseException(string script, string messageSnippet)
        {
            Assert.That(() => _parser.Parse<double>(Lexer.Tokenize(script)), Throws.TypeOf<ParseException>().With.Message.Contains(messageSnippet));
        }
        
        [Test]
        [TestCase("ergebnis 1 + 2 und wahr.", "not a valid boolean expression")]
        [TestCase("ergebnis wahr und 1+2.", "not a valid boolean expression")]
        [TestCase("ergebnis 1 + 2 oder wahr.", "not a valid boolean expression")]
        [TestCase("ergebnis wahr oder 1+2.", "not a valid boolean expression")]
        [TestCase("ergebnis nicht 1+2.", "not a valid boolean expression")]
        [TestCase("ergebnis nicht.", "Invalid negation")]
        public void Parse_InvalidLogicOperationSyntaxProvided_ThrowParseException(string script, string messageSnippet)
        {
            Assert.That(() => _parser.Parse<bool>(Lexer.Tokenize(script)), Throws.TypeOf<ParseException>().With.Message.Contains(messageSnippet));
        }
    }
}