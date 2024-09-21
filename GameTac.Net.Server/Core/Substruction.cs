using GameTac.Net.Server.Core.Data;
using GameTac.Net.Server.Core.Event;
using GameTac.Net.Server.Core.Protocol;
using GameTac.Net.Server.Core.Sys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static GameTac.Net.Server.Util.Print;

namespace GameTac.Net.Server.Core;
internal partial class Substruction
{
    public static void LoadProtocols()
    {
        if (ProtLoaded)
        {
            PrintE("尝试重复加载协议！");
            return;
        }

        ProtocolBase.Init();

        ProtEvent.Init();

        ProtLoaded = true;
    }
    public static void LoadEvents()
    {
        Type i = typeof(IService);
        string methodName = i.GetMethods()[0].Name;
        Type[] types = Reflection.GetImplements(i);
        foreach (var type in types)
        {
            MethodInfo? method = type.GetMethod(methodName);
            method?.Invoke(null, null);
        }
    }

    public void RegisterEvent(Type protType, ProtHandle protEvent)
    {
        string name = protType.Name;
        if (!protEventTable.TryAdd(name, protEvent)) PrintE("正在向ProtEventTable中重复添加Prot！name= " + name);
    }

    public void AddEvent(Type protType, ProtHandle protEvent)
    {
        string name = protType.Name;
        if (protEventTable.TryGetValue(name, out ProtHandle? value))
        {
            protEventTable[name] = value == ProtEvent.NullEvent ? protEvent : value + protEvent;
        }
        else
        {
            PrintW($"尝试添加ProtEvent时未能在表中找到对应的记录 {name}");
        }
    }

    public void RemoveEvent(Type protType, ProtHandle protEvent)
    {
        string name = protType.Name;
       
        if (protEventTable.TryGetValue(name, out ProtHandle? value))
        {
            protEventTable[name] = value - protEvent ?? ProtEvent.NullEvent;
        }
        else
        {
            PrintW($"尝试移除ProtEvent时未能在表中找到对应的记录 {name}");
        }
    }

    public void RegisterProtType(string name, Type type)
    {
        if (!msgTypeTable.TryAdd(name, type)) PrintE("正在向MsgTypeTable中重复添加Msg！name= " + name);
    }

    public Type? GetProtType(string name)
    {
        msgTypeTable.TryGetValue(name, out Type? value);
        return value;
    }

}
