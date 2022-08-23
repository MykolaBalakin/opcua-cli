using System.CommandLine.Invocation;
using System.CommandLine.Parsing;

namespace Balakin.CommandLine;

public abstract class ArgumentBase
{
    public abstract string Name { get; }
    public abstract Type Type { get; }

    public abstract void LoadValue(InvocationContext context);
}

public abstract class ArgumentBase<TValue> : ArgumentBase
{
    private TValue? _value;
    private bool _hasValue;
    private bool _valueIsLoaded;

    public override Type Type => typeof(TValue);

    public TValue Value => _valueIsLoaded ? _value! : throw new InvalidOperationException("Value should be loaded first");
    public bool HasValue => _hasValue;

    public override void LoadValue(InvocationContext context)
    {
        if (_valueIsLoaded)
        {
            throw new InvalidOperationException("Value can be loaded only once");
        }

        if (TryGetArgument(context, out _value))
        {
            _hasValue = true;
        }
        else if (TryGetOption(context, out _value))
        {
            _hasValue = true;
        }

        _valueIsLoaded = true;
    }

    private bool TryGetArgument(InvocationContext context, out TValue value)
    {
        return TryGetByAlias<ArgumentResult>(context, Name, symbol => symbol.GetValueOrDefault<TValue>(), out value);
    }

    private bool TryGetOption(InvocationContext context, out TValue value)
    {
        return TryGetByAlias<OptionResult>(context, "--" + Name, symbol => symbol.GetValueOrDefault<TValue>(), out value);
    }

    private bool TryGetByAlias<TSymbol>(InvocationContext context, string name, Func<TSymbol, TValue?> getValueOrDefault, out TValue value)
    {
        var symbolResult = context.ParseResult.CommandResult.Children.GetByAlias(name);
        if (symbolResult is TSymbol symbol)
        {
            var valueOrDefault = getValueOrDefault(symbol);
            if (valueOrDefault != null)
            {
                value = valueOrDefault;
                return true;
            }
        }

        value = default!;
        return false;
    }
}