using System.Text;
using static GameTac.Net.Server.Util.Print;
using GameTac.Net.Server.Core.Sys;

namespace GameTac.Net.Server.Core.Protocol;

public class ProtocolBase
{
    public static void Init()
    {
        Type[] types = Reflection.GetChilds<ProtocolBase>();

        foreach (var type in types)
        {
            Substruction.Instance.RegisterProtType(type.Name, type);
        }
    }

    //编码
    public static byte[] Encode(ProtocolBase msgBase)
    {
        return Sys.Encoding.Serialize(msgBase);
    }

    //解码
    public static ProtocolBase? Decode(string protoName, byte[] bytes, int offset, int count)
    {
        string s = Sys.Encoding.GetString(bytes, offset, count);

        Type? type = Substruction.Instance.GetProtType(protoName);

        if (type is null)
        {
            PrintE($"尝试解码 {protoName} 时，未能在 MsgTypeTable 中找到对应的记录！");
            return null;
        }

        object? obj = Sys.Encoding.Deserialize(s, type);

        if (obj is null)
        {

            PrintE($"尝试解码 {protoName} 时，反序列化失败！string = {s}");
            return null;
        }

        return (ProtocolBase)obj;
    }

    //编码协议名（2字节长度+字符串）
    public static byte[] EncodeName(ProtocolBase msgBase)
    {
        //名字bytes和长度
        byte[] nameBytes = Sys.Encoding.GetBytes(msgBase.GetType().Name);
        short len = (short)nameBytes.Length;
        //申请bytes数值
        byte[] bytes = new byte[2 + len];
        //组装2字节的长度信息
        bytes[0] = (byte)(len % 256);
        bytes[1] = (byte)(len / 256);
        //组装名字bytes
        Array.Copy(nameBytes, 0, bytes, 2, len);

        return bytes;
    }

    //解码协议名（2字节长度+字符串）
    public static string DecodeName(byte[] bytes, int offset, out int count)
    {
        count = 0;
        //必须大于2字节
        if (offset + 2 > bytes.Length)
        {
            return "";
        }
        //读取长度
        short len = (short)(bytes[offset + 1] << 8 | bytes[offset]);
        if (len <= 0)
        {
            return "";
        }
        //长度必须足够
        if (offset + 2 + len > bytes.Length)
        {
            return "";
        }
        //解析
        count = 2 + len;
        string name = Sys.Encoding.GetString(bytes, offset + 2, len);
        return name;
    }
}
