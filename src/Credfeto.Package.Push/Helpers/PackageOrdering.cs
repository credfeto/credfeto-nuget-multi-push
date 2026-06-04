using System;
using System.Globalization;

namespace Credfeto.Package.Push.Helpers;

public static class PackageOrdering
{
    public static bool IsMetaPackage(string packageId)
    {
#if NET9_0_OR_GREATER
        ReadOnlySpan<char> span = packageId.AsSpan();

        Range? previousPart = null;
        foreach (Range part in span.Split("."))
        {
            if (IsInteger(span[part]))
            {
                if (previousPart is null)
                {
                    break;
                }

                return IsAllTag(span[previousPart.Value]);
            }

            previousPart = part;
        }

        return false;
#else
        string? previousPart = null;
        foreach (string part in packageId.Split("."))
        {
            if (IsInteger(part))
            {
                if (previousPart is null)
                {
                    break;
                }

                return IsAllTag(previousPart.AsSpan());
            }

            previousPart = part;
        }

        return false;
#endif
    }

    private static bool IsInteger(in ReadOnlySpan<char> part)
    {
        return int.TryParse(part, style: NumberStyles.Integer, provider: CultureInfo.InvariantCulture, out _);
    }

    public static bool IsAllTag(in ReadOnlySpan<char> part)
    {
        return part.Equals(other: "all", comparisonType: StringComparison.OrdinalIgnoreCase);
    }
}
