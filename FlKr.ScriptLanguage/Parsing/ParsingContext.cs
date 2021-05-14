using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FlKr.ScriptLanguage.Lexing.Tokens;

namespace FlKr.ScriptLanguage.Parsing
{
    public class ParsingContext
    {
        private ParsingContext _parentContext;

        private Dictionary<string, ParameterExpression> _variables;

        private LabelTarget _returnTarget;
        
        public LabelTarget ReturnTarget
        {
            get
            {
                if (_returnTarget == null && _parentContext != null)
                    return _parentContext.ReturnTarget;
                else
                    return _returnTarget;
            }
            init => _returnTarget = value;
        }

        public ParsingContext(ParsingContext parentContext) : this()
        {
            _parentContext = parentContext;
        }

        public ParsingContext()
        {
            _variables = new Dictionary<string, ParameterExpression>();
        }

        public bool ExistsVariable(string name)
        {
            return _variables.ContainsKey(name) || (_parentContext != null && _parentContext.ExistsVariable(name));
        }

        public bool TryGetVariable(string name, out ParameterExpression variable)
        {
            if (!_variables.TryGetValue(name, out variable))
            {
                if (_parentContext != null)
                    return _parentContext.TryGetVariable(name, out variable);

                variable = null;
                return false;
            }

            return true;
        }

        public void AddVariable(string name, ParameterExpression variable)
        {
            _variables.Add(name, variable);
        }

        public List<ParameterExpression> GetVariables()
        {
            return _variables.Values.ToList();
        }
    }
}