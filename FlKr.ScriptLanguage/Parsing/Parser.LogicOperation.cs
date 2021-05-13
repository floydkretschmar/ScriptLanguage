using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FlKr.ScriptLanguage.Lexing.Tokens;

namespace FlKr.ScriptLanguage.Parsing
{
    public partial class Parser
    {
        private Expression ParseOrOperationExpression(List<IToken> expression, out Type dataType)
        {
            List<List<IToken>> orConditions = SplitIntoExpressions(
                expression,
                true,
                TokenDetailTypes.Or);

            if (orConditions.Count > 1)
            {
                Expression orExpression = ParseAndOperationExpression(orConditions.First());
                for (int i = 1; i < orConditions.Count; i++)
                {
                    orExpression = Expression.OrElse(orExpression, ParseAndOperationExpression(orConditions[i]));
                }

                dataType = typeof(bool);
                return orExpression;
            }
            else
            {
                return ParseAndOperationExpression(expression, out dataType);
            }
        }

        private Expression ParseAndOperationExpression(List<IToken> expression)
        {
            return ParseAndOperationExpression(expression, out var dataType);
        }

        private Expression ParseAndOperationExpression(List<IToken> expression, out Type dataType)
        {
            List<List<IToken>> andConditions = SplitIntoExpressions(
                expression,
                true,
                TokenDetailTypes.And);

            if (andConditions.Count > 1)
            {
                Expression andExpression = ParseEqualsOperationExpression(andConditions.First());
                for (int i = 1; i < andConditions.Count; i++)
                {
                    andExpression = Expression.AndAlso(andExpression, ParseEqualsOperationExpression(andConditions[i]));
                }

                dataType = typeof(bool);
                return andExpression;
            }
            else
            {
                return ParseEqualsOperationExpression(expression, out dataType);
            }
        }

        private Expression ParseEqualsOperationExpression(List<IToken> expression)
        {
            return ParseEqualsOperationExpression(expression, out var dataType);
        }

        private Expression ParseEqualsOperationExpression(List<IToken> expression, out Type dataType)
        {
            List<List<IToken>> equalsConditions = SplitIntoExpressions(
                expression,
                true,
                TokenDetailTypes.And);
            if (equalsConditions.Count > 2)
            {
                throw new ParseException(expression, "Invalid equality expression");
            }
            else if (equalsConditions.Count == 2)
            {
                var equalityParts = new List<Expression>();
                foreach (var equalsCondition in equalsConditions)
                {
                    equalityParts.Add(ParseNotOperationExpression(equalsCondition, expression));
                }

                dataType = typeof(bool);
                return Expression.Equal(equalityParts[0], equalityParts[1]);
            }
            else
            {
                return ParseNotOperationExpression(expression, expression, out dataType);
            }
        }

        private Expression ParseNotOperationExpression(List<IToken> expression, List<IToken> parentExpression)
        {
            return ParseNotOperationExpression(expression, parentExpression, out var dataType);
        }

        private Expression ParseNotOperationExpression(List<IToken> expression, List<IToken> parentExpression,
            out Type dataType)
        {
            if (expression.First().DetailType == TokenDetailTypes.Not)
            {
                if (expression.Count < 2)
                    throw new ParseException(parentExpression, "Invalid negation in equality expression");

                dataType = typeof(bool);
                return Expression.Not(
                    ParseMathOperationExpression(
                        expression.GetRange(1, expression.Count - 1)));
            }

            return ParseMathOperationExpression(expression, out dataType);
        }
    }
}