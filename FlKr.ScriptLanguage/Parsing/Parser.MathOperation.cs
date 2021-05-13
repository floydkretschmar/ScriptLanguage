using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FlKr.ScriptLanguage.Lexing.Tokens;

namespace FlKr.ScriptLanguage.Parsing
{
    delegate Expression ParseOperation(List<IToken> expression, out Type dataType);

    public partial class Parser
    {
        private Expression ParseAdditionOperationExpression(List<IToken> expression)
        {
            return ParseAdditionOperationExpression(expression, out var dataType);
        }

        private Expression ParseAdditionOperationExpression(List<IToken> expression, out Type dataType)
        {
            List<List<IToken>> parts = SplitIntoExpressions(
                expression,
                true,
                TokenDetailTypes.Addition);

            if (parts.Count > 1)
            {
                return ParseMathOperationExpression(
                    parts,
                    Expression.Add,
                    ParseSubtractionOperationExpression,
                    out dataType);
            }

            return ParseSubtractionOperationExpression(expression, out dataType);
        }

        private Expression ParseSubtractionOperationExpression(List<IToken> expression, out Type dataType)
        {
            List<List<IToken>> parts = SplitIntoExpressions(
                expression,
                true,
                TokenDetailTypes.Subtraction);

            if (parts.Count > 1)
                return ParseMathOperationExpression(
                    parts,
                    Expression.Subtract,
                    ParseMultiplicationOperationExpression,
                    out dataType);

            return ParseMultiplicationOperationExpression(expression, out dataType);
        }

        private Expression ParseMultiplicationOperationExpression(List<IToken> expression, out Type dataType)
        {
            List<List<IToken>> parts = SplitIntoExpressions(
                expression,
                true,
                TokenDetailTypes.Multiplication);

            if (parts.Count > 1)
                return ParseMathOperationExpression(
                    parts,
                    Expression.Multiply,
                    ParseDivisionOperationExpression,
                    out dataType);

            return ParseDivisionOperationExpression(expression, out dataType);
        }

        private Expression ParseDivisionOperationExpression(List<IToken> expression, out Type dataType)
        {
            List<List<IToken>> parts = SplitIntoExpressions(
                expression,
                true,
                TokenDetailTypes.Division);

            if (parts.Count > 1)
                return ParseMathOperationExpression(
                    parts,
                    Expression.Divide,
                    ParseNegativeExpression,
                    out dataType);

            return ParseNegativeExpression(expression, out dataType);
        }

        private Expression ParseNegativeExpression(List<IToken> expression, out Type dataType)
        {
            if (expression.First().DetailType == TokenDetailTypes.Negative)
            {
                if (expression.Count < 2)
                    throw new ParseException(expression, "Invalid unary negative expression");

                return Expression.Negate(
                    ParseNegativeExpression(
                        expression.GetRange(1, expression.Count - 1), out dataType));
            }
            
            if (expression.Count > 1)
                throw new ParseException(expression, "Invalid negative value expression.");

            var negatedExpression = ParseValueExpression(expression[0], out dataType);
            if (dataType != typeof(double) && dataType != typeof(int))
                throw new ParseException(expression,
                    "Invalid negation: Negated expressions have to return numeric values.");
            return negatedExpression;
        }

        private Expression ParseValueExpression(IToken valueToken, out Type dataType)
        {
            switch (valueToken.DetailType)
            {
                case TokenDetailTypes.True:
                    dataType = typeof(bool);
                    return Expression.Constant(true, dataType);
                case TokenDetailTypes.False:
                    dataType = typeof(bool);
                    return Expression.Constant(false, dataType);
                case TokenDetailTypes.FloatingPoint:
                    dataType = typeof(double);
                    return Expression.Constant(double.Parse(((Token) valueToken).Value), dataType);
                case TokenDetailTypes.Integer:
                    dataType = typeof(int);
                    return Expression.Constant(int.Parse(((Token) valueToken).Value), dataType);
                case TokenDetailTypes.Text:
                    dataType = typeof(string);
                    return Expression.Constant(((Token) valueToken).Value, dataType);
                case TokenDetailTypes.Expression:
                    var expressionToken = (ExpressionToken) valueToken;
                    dataType = expressionToken.DataType;
                    return expressionToken.Value;
                case TokenDetailTypes.VariableName:
                    if (!_variables.TryGetValue(((Token) valueToken).Value, out var variable))
                        throw new ParseException(new List<IToken>() {valueToken}, "Variable has not been declared.");
                    dataType = variable.Type;
                    return variable;
                default:
                    throw new ParseException(new List<IToken>() {valueToken},
                        $"The token type {valueToken.DetailType} is not a valid value expression.");
            }
        }


        private Expression ParseMathOperationExpression(
            List<List<IToken>> expressions,
            Func<Expression, Expression, BinaryExpression> expressionDefinition,
            ParseOperation parseOperation,
            out Type dataType)
        {
            var expression = parseOperation(expressions.First(), out dataType);
            if (dataType != typeof(int) && dataType != typeof(double))
                throw new ParseException(expressions.First(), "Expression is not a valid numerical expression.");

            for (int i = 1; i < expressions.Count; i++)
            {
                var otherExpression = parseOperation(expressions[i], out var subDataType);
                if (subDataType != typeof(int) && subDataType != typeof(double))
                    throw new ParseException(expressions.First(),
                        "Expression is not a valid numerical expression.");

                // only use decimal point math when necessary
                if (dataType == typeof(int) && subDataType == typeof(double))
                    dataType = typeof(double);

                expression = expressionDefinition(expression, otherExpression);
            }

            return expression;
        }
    }
}