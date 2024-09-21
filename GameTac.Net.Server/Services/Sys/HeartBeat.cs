using GameTac.Net.Server.Core.Protocol;
using GameTac.Net.Server.Core;
using GameTac.Net.Server.Protocols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameTac.Net.Server.Core.Event;
using GameTac.Net.Server.Core.Sys;

namespace GameTac.Net.Server.Services.Sys;
internal class HeartBeat : IService
{
    public static void BindEvents()
    {
        Dashboard.AddProtEvent<ProtPing>(ProtPing);
    }

    static string ProtPing(Client c, ProtocolBase body)
    {
        c.lastPingTime = Substruction.GetTimeStamp();
        ProtPong msgPong = new();
        Substruction.Send(c, msgPong);
        return "ping";
    }
}
