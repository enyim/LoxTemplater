using System.Linq.Expressions;

using X = System.Linq.Expressions.Expression;

namespace Enyim.LoxTempl;

internal static class FastActivator
{
    public static T Create<T>() => Cache<T>.Factory.Invoke();

    public static Func<T> GetFactory<T>() => Cache<T>.Factory;
    public static Func<object> GetFactory(Type type) => typeof(Cache<>).MakeGenericType(type).GetField("Factory")?.GetValue(null) as Func<object> ?? throw new InvalidOperationException("developer bug");

    private static class Cache<T>
    {
        private static readonly Expression<Func<T>> FactoryExpression = X.Lambda<Func<T>>(X.New(typeof(T).GetConstructor(Type.EmptyTypes) ?? throw new InvalidOperationException($"{typeof(T)} must have defult constructor")));
        public static readonly Func<T> Factory = FactoryExpression.Compile();
    }
}
