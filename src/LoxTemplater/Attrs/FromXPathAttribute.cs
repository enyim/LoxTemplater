namespace Enyim.LoxTempl;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class FromXPathAttribute : Attribute
{
    public FromXPathAttribute(string expression)
    {
        Expression = expression;
    }

    public string Expression { get; }
}
