using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace FlKr.ScriptLanguage.Parsing;

public class ParsingContext
{
    private readonly ParsingContext _parentContext;

    private readonly LabelTarget _returnTarget;

    private readonly Dictionary<string, ParameterExpression> _variables;

    public ParsingContext(ParsingContext parentContext) : this()
    {
        _parentContext = parentContext;
    }

    public ParsingContext()
    {
        _variables = new Dictionary<string, ParameterExpression>();
    }

    public LabelTarget ReturnTarget
    {
        get
        {
            if (_returnTarget == null && _parentContext != null)
                return _parentContext.ReturnTarget;
            return _returnTarget;
        }
        init => _returnTarget = value;
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