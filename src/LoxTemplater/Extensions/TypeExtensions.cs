namespace Enyim.LoxTempl;

public static class TypeExtensions
{
    public static bool IsMadeFrom(this Type self, Type openGeneric)
    {
        if (!self.IsConstructedGenericType) return false;

        var def = self.GetGenericTypeDefinition();

        return def == openGeneric;
    }
}
