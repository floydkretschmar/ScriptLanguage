namespace FlKr.ScriptLanguage.Base
{
    internal class TokenDefinition
    {
        public string Pattern { get; }
        
        public TokenDetailTypes DetailType { get; }

        public TokenTypes Type { get; }
        
        public TokenDefinition(string pattern, TokenDetailTypes detailType, TokenTypes type)
        {
            Pattern = pattern;
            DetailType = detailType;
            Type = type;
        }
    }
}