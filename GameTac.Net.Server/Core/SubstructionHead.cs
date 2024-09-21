using GameTac.Net.Server.Core.Protocol;
using GameTac.Net.Server.Core.Sys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GameTac.Net.Server.Core;
internal partial class Substruction
{
    private readonly static Substruction _instance = new();
    public static Substruction Instance
    {
        get
        {
            return _instance;
        }
    }

    private Substruction() { }

    //是否已加载协议
    public static bool ProtLoaded { get; private set; }
    //监听Socket
    private readonly Socket listenfd = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    //客户端Socket及状态信息
    private readonly Dictionary<Socket, Client> clients = [];
    //Select的检查列表
    private readonly List<Socket> checkRead = [];
    //Msg名/类型对照表
    private readonly Dictionary<string, Type> msgTypeTable = [];
    //Handle委托
    public delegate string ProtHandle(Client state, ProtocolBase body);
    //Handle注册表
    private readonly Dictionary<string, ProtHandle> protEventTable = [];
}
