using System.Diagnostics;
using System.Reflection;

using X = System.Linq.Expressions.Expression;

namespace Enyim.LoxTempl;

internal class FastProperty<TInstance, TValue>
    where TInstance : class
{
    private readonly PropertyInfo property;
    private readonly Action<TInstance, TValue?> setter;
    private readonly Func<TInstance, TValue?> getter;

    public FastProperty(PropertyInfo property)
    {
        Debug.Assert(typeof(TValue) == property.PropertyType);

        if (property.CanWrite)
        {
            var setterInstanceArg = X.Parameter(typeof(TInstance));
            var setterValueArg = X.Parameter(typeof(TValue));
            var setterLambda = X.Lambda<Action<TInstance, TValue?>>(X.Assign(X.Property(setterInstanceArg, property), setterValueArg), setterInstanceArg, setterValueArg);

            setter = setterLambda.Compile();
        }
        else
        {
            setter = (_, _) => { };
        }

        if (property.CanRead)
        {
            var getterInstanceArg = X.Parameter(typeof(TInstance));
            var getterLambda = X.Lambda<Func<TInstance, TValue?>>(X.Property(getterInstanceArg, property), getterInstanceArg);

            getter = getterLambda.Compile();
        }
        else
        {
            getter = (_) => throw new NotSupportedException($"Property {property.DeclaringType}.{property.Name} is not readable");
        }

        this.property = property;
    }

    public void Set(TInstance instance, TValue? value) => setter(instance, value);
    public TValue? Get(TInstance instance) => getter(instance);

    public bool CanRead => property.CanRead;
    public bool CanWrite => property.CanWrite;

    public Type PropertyType => property.PropertyType;
    public string Name => property.Name;
}
