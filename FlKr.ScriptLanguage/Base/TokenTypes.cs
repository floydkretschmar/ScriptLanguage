namespace FlKr.ScriptLanguage.Base
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
        
        // Variable
        VariableName,
        Assignment,
        
        // Value
        Text,
        Number,
        Boolean,
        
        // Math Operation
        Addition,
        Subtraction,
        Division,
        Multiplication,
        Modulo,
        
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