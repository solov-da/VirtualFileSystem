using System;
using System.Collections.Generic;
using System.Linq;

namespace FileSystem.Library.Common;

internal static class Extensions
{
    public static string CombineEntry(this string path, string name)
    {
        return (path == Constants.Path.Delimiter) ? $"{path}{name}" : $"{path}{Constants.Path.Delimiter}{name}";
    }

    public static void TruncateBytes(this Dictionary<long, byte> dict, long length)
    {
        if (dict is null)
            throw new ArgumentNullException(nameof(dict));
        
        var keys = dict
            .Keys
            .Where(p => p >= length)
            .ToArray();

        foreach (var key in keys)
        {
            dict.Remove(key);
        }
    }
}