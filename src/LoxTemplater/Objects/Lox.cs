namespace Enyim.LoxTempl;

public static class Lox
{
    public static string NewId() => Guid.NewGuid().ToString("D").Remove(23, 1);
}
