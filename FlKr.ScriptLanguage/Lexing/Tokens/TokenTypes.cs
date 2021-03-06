namespace FlKr.ScriptLanguage.Lexing.Tokens;

public enum TokenTypes
{
    MathOperation,
    LogicOperation,
    ControlFlow,
    Value,
    Syntax,
    Variable,
    Expression
}

public enum TokenDetailTypes
{
    // Syntax
    Whitespace,
    EndOfLine,
    BracketedExpression,
    BlockExpression,
    LeftBracket,
    RightBracket,

    // Variable
    VariableName,
    Assignment,

    // Value
    Text,
    Number,
    True,
    False,

    // Math Operation
    Addition,
    Subtraction,
    Division,
    Multiplication,
    Modulo,
    Exponentiation,
    Negative,

    // Logic Operation
    Equals,
    GreaterThan,
    GreaterEqualThan,
    LessThan,
    LessEqualThan,
    Not,
    And,
    Or,

    // Control flow
    If,
    Else,
    ElseIf,
    Do,
    BeginBlock,
    EndBlock,
    Return
}