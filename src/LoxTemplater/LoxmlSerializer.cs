using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Xml.Linq;

namespace Enyim.LoxTempl;

public class LoxmlSerializer
{
    private readonly TypeMatcher[] matchers;
    private readonly IReadOnlyDictionary<Type, TypeMatcher> typeMatchers;

    public LoxmlSerializer(IEnumerable<Assembly> assemblies) : this(assemblies.SelectMany(a => a.GetTypes())) { }

    public LoxmlSerializer(params Type[] types) : this(types.AsEnumerable()) { }

    public LoxmlSerializer(IEnumerable<Type> types)
    {
        var queue = new Queue<Type>();
        var seen = new HashSet<Type>();

        var matchers = new List<TypeMatcher>();

        foreach (var type in types)
        {
            if (type == typeof(LoxIn))
            {
            }

            queue.Enqueue(type);

            while (queue.TryDequeue(out var current))
            {
                if (!seen.Add(current)
                    || current.IsAbstract
                    || !current.IsClass
                    || !typeof(LoxBase).IsAssignableFrom(current))
                {
                    continue;
                }

                var wrapper = MkWrapper(current);

                matchers.AddRange(from attr in current.GetCustomAttributes()
                                  let m = attr as IXElementMatcher
                                  where m != null
                                  select new TypeMatcher(m, current, wrapper));

                foreach (var t in GetCandidateTypes(type))
                {
                    if (!seen.Contains(t))
                    {
                        queue.Enqueue(t);
                    }
                }
            }

            static IEnumerable<Type> GetCandidateTypes(Type source)
            {
                foreach (var p in source.GetProperties())
                {
                    Type pt = p.PropertyType;
                    yield return pt;

                    if (pt.IsArray && pt.HasElementType && pt.GetArrayRank() == 1)
                    {
                        yield return pt.GetElementType() ?? throw new InvalidOperationException();
                    }

                    if (pt.IsConstructedGenericType && pt.GenericTypeArguments?.Length == 1)
                    {
                        yield return pt.GenericTypeArguments[0];
                    }
                }
            }
        }

        this.matchers = matchers.ToArray();
        typeMatchers = matchers.ToDictionary(m => m.Type);

        _Wrapper MkWrapper(Type type) => (_Wrapper)(Activator.CreateInstance(typeof(_Wrapper<>).MakeGenericType(type), new _Context(this)) ?? throw new InvalidOperationException());
    }

    public IEnumerable<T> Select<T>(XNode start)
        where T : LoxBase
    {
        var t = typeof(T);
        var matchers = t.IsAbstract
                        ? typeMatchers.Where(g => t.IsAssignableFrom(g.Key)).Select(kvp => kvp.Value)
                        : new[] { typeMatchers[t] };

        foreach (var tm in matchers)
        {
            foreach (var e in tm.Logic.SelectElements(start))
            {
                var tmp = tm.Wrapper.Factory();
                tm.Wrapper.Load(e, tmp);

                yield return (T)tmp;
            }
        }
    }

    public IEnumerable<T> Select<T>(IXmlBound start)
        where T : LoxBase
    {
        return Select<T>(start.Element ?? throw new InvalidOperationException());
    }

    public IEnumerable<XElement> Elements<T>(XElement element)
    {
        var matcher = typeMatchers[typeof(T)];

        return matcher.Logic.SelectElements(element);
    }

    public bool Is<T>(XElement element)
    {
        var t = typeof(T);
        var matchers = t.IsAbstract
                        ? typeMatchers.Where(g => t.IsAssignableFrom(g.Key)).Select(kvp => kvp.Value)
                        : new[] { typeMatchers[t] };

        return matchers.Any(m => m.Logic.IsMatch(element));
    }

    public XElement Create<T>()
    {
        var t = typeof(T);
        if (t.IsAbstract) throw new InvalidOperationException("abstract types cannpt be created");

        return typeMatchers[t].Logic.Create();
    }

    public XElement Create(LoxBase instance)
    {
        return typeMatchers[instance.GetType()].Logic.Create();
    }

    public LoxBase Load(XElement element)
    {
        return TryLoad(element, out var retval) ? retval : throw new InvalidOperationException();
    }

    public T Load<T>(XElement element)
        where T : LoxBase
    {
        return TryLoad<T>(element, out var retval) ? retval : throw new InvalidOperationException();
    }

    public bool CanLoad(XElement element)
    {
        return TryMatch(element, out _);
    }

    public bool TryLoad(XElement element, [NotNullWhen(true)] out LoxBase? retval)
    {
        if (TryMatch(element, out var item))
        {
            retval = item.Wrapper.Factory();
            item.Wrapper.Load(element, retval);

            return true;
        }

        retval = default;
        return false;
    }

    public bool TryLoad<T>(XElement element, [NotNullWhen(true)] out T? retval)
        where T : LoxBase
    {
        if (typeMatchers.TryGetValue(typeof(T), out var matcher) && matcher.Logic.IsMatch(element))
        {
            retval = matcher.Load<T>(element);

            return true;
        }

        retval = default;
        return false;
    }

    private bool TryMatch(XElement element, [NotNullWhen(true)] out TypeMatcher? retval)
    {
        ArgumentNullException.ThrowIfNull(element);

        foreach (var item in matchers)
        {
            if (item.Logic.IsMatch(element))
            {
                retval = item;

                return true;
            }
        }

        retval = default;
        return false;
    }

    public void ReadFrom(XElement source, LoxBase instance)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(instance);

        if (TryMatch(source, out var item))
        {
            item.Wrapper.Load(source, instance);
        }
    }

    public void Commit(LoxBase instance)
    {
        ArgumentNullException.ThrowIfNull(instance);

        if (instance is IXmlBound b)
        {
            if (b.Element is null) throw new InvalidOperationException("Loxbase has no XML");

            typeMatchers[instance.GetType()].Wrapper.Save(instance, b.Element);
        }
    }

    public void Commit(LoxBase instance, XElement target)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(target);

        typeMatchers[instance.GetType()].Wrapper.Save(instance, target);
    }

    private record TypeMatcher(IXElementMatcher Logic, Type Type, _Wrapper Wrapper)
    {
        public T Load<T>(XElement e)
            where T : LoxBase
        {
            var retval = (T)Wrapper.Factory();
            Wrapper.Load(e, retval);

            return retval;
        }
    }

    private abstract class _Wrapper
    {
        public abstract Func<LoxBase> Factory { get; }
        public abstract void Load(XElement source, LoxBase instance);
        public abstract void Save(LoxBase instance, XElement target);
    }

    private class _Wrapper<T> : _Wrapper
        where T : LoxBase
    {
        private static readonly Func<T> factory = FastActivator.GetFactory<T>();
        private static readonly XmlSerializerLite<T> serializer = XmlSerializerLite<T>.Reflect();
        private readonly ISerializationContext owner;

        public _Wrapper(ISerializationContext owner)
        {
            this.owner = owner;
        }

        public override Func<LoxBase> Factory => factory;
        public override void Load(XElement source, LoxBase instance) => serializer.Load(source, (T)instance, owner);
        public override void Save(LoxBase instance, XElement target) => serializer.Save((T)instance, target, owner);
    }

    private class _Context : ISerializationContext
    {
        private readonly LoxmlSerializer serializer;

        public _Context(LoxmlSerializer owner) => serializer = owner;

        public XElement Create<T>() => serializer.Create<T>();

        public IEnumerable<XElement> Elements<T>(XElement e) => serializer.Elements<T>(e);

        //public XElement Create(LoxBase o) => serializer.Create(o);

        public bool TryLoad(XElement e, [NotNullWhen(true)] out LoxBase? retval) => serializer.TryLoad(e, out retval);
        public bool TryLoad<T>(XElement e, [NotNullWhen(true)] out T? retval) where T : LoxBase => serializer.TryLoad<T>(e, out retval);
        public void Write(LoxBase o, XElement target) => serializer.Commit(o, target);
    }
}
