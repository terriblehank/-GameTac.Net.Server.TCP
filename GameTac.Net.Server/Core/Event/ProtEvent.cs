using GameTac.Net.Server.Core.Protocol;
using GameTac.Net.Server.Core.Sys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static GameTac.Net.Server.Util.Print;

namespace GameTac.Net.Server.Core.Event;
internal class ProtEvent
{
    public static void Init()
    {
        Type[] types = Reflection.GetChilds<ProtocolBase>();

        foreach (var type in types)
        {
            Substruction.Instance.RegisterEvent(type, NullEvent);
        }
    }

    public static string NullEvent(Client state, ProtocolBase prot)
    {
        return $"{prot.GetType().Name} 被接收了，但它没有设置任何回调！";
    }

    public static void Add<T>(Substruction.ProtHandle @event) where T : ProtocolBase
    {
        Substruction.Instance.AddEvent(typeof(T), @event);
    }

    public static void Remove<T>(Substruction.ProtHandle @event) where T : ProtocolBase
    {
        Substruction.Instance.RemoveEvent(typeof(T), @event);
    }
}
