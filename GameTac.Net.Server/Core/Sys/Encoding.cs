using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GameTac.Net.Server.Core.Sys;
internal class Encoding
{
    public static byte[] GetBytes(string str)
    {
        return System.Text.Encoding.UTF8.GetBytes(str);
    }
    public static string GetString(byte[] bytes, int offset, int count)
    {
        return System.Text.Encoding.UTF8.GetString(bytes, offset, count);
    }

    public static byte[] Serialize<T>(T obj)
    {
        return JsonSerializer.SerializeToUtf8Bytes(obj);
    }

    public static object? Deserialize(string str, Type type)
    {
        return JsonSerializer.Deserialize(str, type);
    }
}
