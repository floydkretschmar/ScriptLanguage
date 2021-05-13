﻿namespace FlKr.ScriptLanguage.Lexing.Tokens
{
    public enum TokenTypes
    {
        MathOperation,
        LogicOperation,
        ControlFlow,
        Value,
        Syntax,
        Variable
    }
    
    public enum TokenDetailTypes
    {
        // Syntax
        Whitespace,
        EndOfLine,
        EndOfLineBlock,
        Expression,
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
        Not,
        And,
        Or,
        
        // Control flow
        If,
        Then,
        Else,
        Return
    }
}