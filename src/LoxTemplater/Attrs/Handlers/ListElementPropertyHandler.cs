using System.Diagnostics;
using System.Reflection;
using System.Xml.Linq;

namespace Enyim.LoxTempl;

internal sealed class ListElementPropertyHandler<TInstance, TValue> : IPropertyHandler<TInstance>
    where TInstance : class
{
    private static readonly Func<IListBuilder> ListBuilderFactory;

    private readonly FastProperty<TInstance, TValue> property;
    private readonly XName? name;
    private readonly IListBuilder builder;

    static ListElementPropertyHandler()
    {
        var innerType = Unwrap();
        var factory = FastActivator.GetFactory(typeof(ListBuilder<>).MakeGenericType(typeof(TInstance), typeof(TValue), innerType));

        ListBuilderFactory = () => (IListBuilder)factory();
    }

    public ListElementPropertyHandler(PropertyInfo property, FromElementAttribute attr)
    {
        Debug.Assert(property.PropertyType == typeof(TValue));

        this.property = new FastProperty<TInstance, TValue>(property);
        name = attr.Name;
        builder = ListBuilderFactory();
    }

    public void Load(XElement source, TInstance instance, ISerializationContext context)
    {
        if (!property.CanWrite) return;

        var element = (name == null ? source : source.Element(name)) ?? throw new InvalidOperationException($"Cannot find element {name} in {source}");

        var list = builder.Load(element, context);
        property.Set(instance, (TValue)list);
    }

    public void Save(TInstance instance, XElement target, ISerializationContext context)
    {
        if (!property.CanRead) return;

        var element = (name == null ? target : target.Element(name)) ?? throw new InvalidOperationException($"Cannot find element {name} in {target}");
        var value = property.Get(instance);

        builder.Save(value, element, context);
    }

    private interface IListBuilder
    {
        object Load(XElement source, ISerializationContext context);
        void Save(object? value, XElement target, ISerializationContext context);
    }

    private class ListBuilder<TInner> : IListBuilder
        where TInner : LoxBase
    {
        public TrackingList<TInner> Load(XElement source, ISerializationContext context)
        {
            var retval = new TrackingList<TInner>();

            foreach (var element in source.Elements())
            {
                if (context.TryLoad<TInner>(element, out var item))
                {
                    retval.Add(item);
                }
            }

            return retval;
        }

        public void Save(TrackingList<TInner>? list, XElement target, ISerializationContext context)
        {
            if (list == null)
            {
                //TODO remove all?
                return;
            }

            foreach (var item in list)
            {
                if (item is IXmlBound bound && bound.Element is not null)
                {
                    context.Write(item, bound.Element);
                }
                else
                {
                    var element = context.Create<TInner>();
                    context.Write(item, element);
                    target.Add(element);
                }
            }

            foreach (var removed in list.Removed)
            {
                if (removed is IXmlBound bound && bound.Element?.Parent is not null)
                {
                    bound.Element.Remove();
                }
            }
        }

        object ListElementPropertyHandler<TInstance, TValue>.IListBuilder.Load(XElement source, ISerializationContext context)
            => Load(source, context);

        void ListElementPropertyHandler<TInstance, TValue>.IListBuilder.Save(object? value, XElement target, ISerializationContext context)
            => Save((TrackingList<TInner>?)value, target, context);
    }

    private static Type Unwrap()
    {
        var propType = typeof(TValue);

        if (propType.IsMadeFrom(typeof(TrackingList<>)))
        {
            return propType.GenericTypeArguments[0];
        }

        throw new NotSupportedException($"{propType} is not a supported type for editable lists");
    }
}
