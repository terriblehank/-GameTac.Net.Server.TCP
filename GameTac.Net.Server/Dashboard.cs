using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using GameTac.Net.Server.Core;
using System.Reflection;
using GameTac.Net.Server.Core.Protocol;
using GameTac.Net.Server.Core.Event;
using GameTac.Net.Server.Core.Sys;
using static GameTac.Net.Server.Util.Print;

namespace GameTac.Net.Server;

internal class Dashboard
{
    private readonly static Dashboard _instance = new();
    internal static Dashboard Instance
    {
        get
        {
            return _instance;
        }
    }
    private Dashboard() { }

    private readonly Substruction state = Substruction.Instance;

    public static void LoadProtocols()
    {
        Substruction.LoadProtocols();
        Substruction.LoadEvents();
    }

    public void Start(string ip, int listenPort)
    {
        //Bind
        IPAddress ipAdr = IPAddress.Parse(ip);
        IPEndPoint ipEp = new(ipAdr, listenPort);

        state.Start(ipEp);
    }

    public static bool AddProtEvent<T>(Substruction.ProtHandle @event) where T : ProtocolBase
    {
        if (!Substruction.ProtLoaded)
        {
            PrintE("尝试添加ProtEvent但协议还没有被加载！");
            return false;
        }

        ProtEvent.Add<T>(@event);
        return true;
    }

    public static bool RemoveProtEvent<T>(Substruction.ProtHandle @event) where T : ProtocolBase
    {
        if (!Substruction.ProtLoaded)
        {
            PrintE("尝试移除ProtEvent但协议还没有被加载！");
            return false;
        }
        ProtEvent.Remove<T>(@event);
        return true;
    }
}

