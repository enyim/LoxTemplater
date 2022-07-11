using System.Diagnostics;
using System.Reflection;
using System.Xml.Linq;

namespace Enyim.LoxTempl;

internal sealed class XmlSerializerLite<TInstance>
    where TInstance : LoxBase
{
    private readonly List<IPropertyHandler<TInstance>> handlers = new();

    private XmlSerializerLite() { }

    public void Load(XElement source, TInstance instance, ISerializationContext context)
    {
        if (instance is ISerializationCallback<BeforeLoad> cb1)
        {
            cb1.Handle(new(source));
        }

        foreach (var handler in handlers)
        {
            handler.Load(source, instance, context);
        }

        if (instance is ISerializationCallback<AfterLoad> cb2)
        {
            cb2.Handle(new(source));
        }
    }

    public void Save(TInstance instance, XElement target, ISerializationContext context)
    {
        if (instance is ISerializationCallback<BeforeSave> cb1)
        {
            cb1.Handle(new(target));
        }

        foreach (var handler in handlers)
        {
            handler.Save(instance, target, context);
        }

        if (instance is ISerializationCallback<AfterSave> cb2)
        {
            cb2.Handle(new(target));
        }
    }

    public static XmlSerializerLite<TInstance> Reflect()
    {
        var retval = new XmlSerializerLite<TInstance>();

        foreach (var property in typeof(TInstance).GetProperties())
        {
            var fromAttr = property.GetCustomAttribute<FromAttributeAttribute>();
            if (fromAttr != null)
            {
                retval.handlers.Add(CreateInstance(typeof(AttributePropertyHandler<,>), property, new object[] { property, fromAttr }));
                continue;
            }

            var fromElement = property.GetCustomAttribute<FromElementAttribute>();
            if (fromElement != null)
            {
                if (property.PropertyType.IsMadeFrom(typeof(TrackingList<>)))
                {
                    retval.handlers.Add(CreateInstance(typeof(ListElementPropertyHandler<,>), property, new object[] { property, fromElement }));
                    continue;
                }

                if (!typeof(LoxBase).IsAssignableFrom(property.PropertyType))
                {
                    throw new InvalidOperationException($"{nameof(FromElementAttribute)} only supports complex ({nameof(LoxBase)}) types");
                }

                retval.handlers.Add(CreateInstance(typeof(ElementPropertyHandler<,>), property, new object[] { property, fromElement }));
                continue;
            }

            var fromXPath = property.GetCustomAttribute<FromXPathAttribute>();
            if (fromXPath != null)
            {
                retval.handlers.Add(CreateInstance(typeof(ReadOnlyCollectionPropertyHandler<,>), property, new object[] { property, fromXPath }));
                continue;
            }
        }
        return retval;
    }

    private static IPropertyHandler<TInstance> CreateInstance(Type attrType, PropertyInfo property, params object[] args)
    {
        var instance = Activator.CreateInstance(attrType.MakeGenericType(typeof(TInstance), property.PropertyType), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, args, null);
        var retval = instance as IPropertyHandler<TInstance>;
        Debug.Assert(retval != null);

        return retval;
    }
}
