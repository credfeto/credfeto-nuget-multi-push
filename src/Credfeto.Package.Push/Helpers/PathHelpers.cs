using System.IO;

namespace Credfeto.Package.Push.Helpers;

public static class PathHelpers
{
    public static string ConvertToNative(string path)
    {
        return ConvertToNative(path: path, nativeSeparator: Path.DirectorySeparatorChar);
    }

    public static string ConvertToNative(string path, char nativeSeparator)
    {
        if (nativeSeparator == '\\')
        {
            return path.Replace(oldChar: '/', newChar: nativeSeparator);
        }

        return path.Replace(oldChar: '\\', newChar: nativeSeparator);
    }
}
