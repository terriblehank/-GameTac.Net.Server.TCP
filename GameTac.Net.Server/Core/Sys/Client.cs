using GameTac.Net.Server.Core.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GameTac.Net.Server.Core.Sys;
internal class Client
{
    public required Socket socket;
    public ByteArray readBuff = new();
    //Ping
    public long lastPingTime = 0;
}
