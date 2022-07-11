using System.Diagnostics.CodeAnalysis;

namespace Enyim.LoxTempl;

public static class LoxConnectionExtensions
{
    public static LoxConnector? ByName(this IEnumerable<LoxConnector> self, string name, [DoesNotReturnIf(true)] bool throwIfMissing = true)
    {
        var retval = self.FirstOrDefault(c => c.K == name);

        if (retval == null && throwIfMissing)
        {
            throw new InvalidOperationException($"Missing Co K={name}");
        }

        return retval;
    }
}
