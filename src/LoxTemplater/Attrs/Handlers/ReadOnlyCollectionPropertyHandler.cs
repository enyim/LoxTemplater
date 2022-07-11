using System.Diagnostics;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.XPath;

using X = System.Linq.Expressions.Expression;

namespace Enyim.LoxTempl;

internal sealed class ReadOnlyCollectionPropertyHandler<TInstance, TValue> : IPropertyHandler<TInstance>
    where TInstance : class
    where TValue : class
{
    private readonly FastProperty<TInstance, TValue> property;
    private readonly string expression;

    private static readonly Func<string, XElement, ISerializationContext, object> MakeListValue;

    static ReadOnlyCollectionPropertyHandler()
    {
        var (innerType, isArray) = Unwrap();

        var builder = typeof(ListBuilder<>).MakeGenericType(typeof(TInstance), typeof(TValue), innerType);

        var arg1 = X.Parameter(typeof(string));
        var arg2 = X.Parameter(typeof(XElement));
        var arg3 = X.Parameter(typeof(ISerializationContext));

        MakeListValue = X.Lambda<Func<string, XElement, ISerializationContext, object>>(X.Call(builder.GetMethod("Build")!, arg1, X.Constant(isArray), arg2, arg3), arg1, arg2, arg3).Compile();
    }

    public ReadOnlyCollectionPropertyHandler(PropertyInfo property, FromXPathAttribute attr)
    {
        Debug.Assert(property.PropertyType == typeof(TValue));

        this.property = new FastProperty<TInstance, TValue>(property);
        expression = attr.Expression;
    }

    public void Load(XElement source, TInstance instance, ISerializationContext context)
    {
        if (!property.CanWrite) return;

        var value = MakeListValue(expression, source, context);
        property.Set(instance, (TValue)value);
    }

    public void Save(TInstance instance, XElement target, ISerializationContext context)
    {
        var v = property.Get(instance);
        if (v == null) return;

        foreach (var item in (IEnumerable<LoxObject>)v)
        {
            var element = (item as IXmlBound)?.Element;

            if (element != null)
            {
                context.Write(item, element);
            }
        }
    }

    private static class ListBuilder<TInner>
        where TInner : LoxBase
    {
        public static object Build(string xpath, bool toArray, XElement source, ISerializationContext context)
        {
            var retval = new List<TInner>();
            foreach (var element in source.XPathSelectElements(xpath))
            {
                if (element == source)
                {
                    throw new InvalidOperationException($"{xpath} matches the container node in {source}");
                }

                if (context.TryLoad(element, out var item))
                {
                    retval.Add((TInner)item);
                }
                else if (context.TryLoad<TInner>(element, out var item2))
                {
                    retval.Add(item2);
                }
            }

            return toArray ? retval.ToArray() : retval;
        }
    }

    private static (Type inner, bool isArray) Unwrap()
    {
        var propType = typeof(TValue);
        if (propType.IsArray)
        {
            if (propType.GetArrayRank() != 1) throw new NotSupportedException();
            var elementType = propType.GetElementType();
            if (elementType == null) throw new NotSupportedException();

            return (elementType, true);
        }

        if (!propType.IsConstructedGenericType) throw new NotSupportedException();
        if (propType.GenericTypeArguments.Length != 1) throw new NotSupportedException();

        var arg = propType.GenericTypeArguments[0];
        Debug.Assert(arg != null);

        if (Is(typeof(IEnumerable<>))
                || Is(typeof(IReadOnlyCollection<>))
                || Is(typeof(IReadOnlyList<>))
            )
        {
            return (arg, false);
        }

        throw new NotSupportedException($"{propType} is not a supported type for collections");

        bool Is(Type openGeneric) => openGeneric.MakeGenericType(arg) == typeof(TValue);
    }
}
