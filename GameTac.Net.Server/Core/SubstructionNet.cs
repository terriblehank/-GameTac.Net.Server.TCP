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
    public void Start(IPEndPoint ipEp)
    {
        if (!ProtLoaded)
        {
            PrintE("Start fail, Protocol 尚未被加载，请先执行LoadProtocols！");
            return;
        }

        listenfd.Bind(ipEp);
        //Listen
        listenfd.Listen(0);
        PrintM("[服务器]启动成功");

        Loop();
    }

    private void Loop()
    {
        //循环
        while (true)
        {
            ResetCheckRead();  //重置checkRead
            Socket.Select(checkRead, null, null, 1000);
            //检查可读对象
            for (int i = checkRead.Count - 1; i >= 0; i--)
            {
                Socket s = checkRead[i];
                if (s == listenfd)
                {
                    ReadListenfd(s);
                }
                else
                {
                    ReadClientfd(s);
                }
            }
        }
    }
    private void ResetCheckRead()
    {
        checkRead.Clear();
        checkRead.Add(listenfd);
        foreach (Client s in clients.Values)
        {
            checkRead.Add(s.socket);
        }
    }

    //读取Listenfd
    private void ReadListenfd(Socket listenfd)
    {
        try
        {
            Socket clientfd = listenfd.Accept();
            PrintM("Accept " + clientfd.RemoteEndPoint?.ToString());

            Client state = new()
            {
                socket = clientfd,
                lastPingTime = GetTimeStamp()
            };

            clients.Add(clientfd, state);
        }
        catch (SocketException ex)
        {
            PrintM("Accept fail" + ex.ToString());
        }
    }

    //关闭连接
    private void Close(Client state)
    {
        //关闭
        state.socket.Close();
        clients.Remove(state.socket);
    }

    //读取Clientfd
    private void ReadClientfd(Socket clientfd)
    {

        Client state = clients[clientfd];
        ByteArray readBuff = state.readBuff;
        //接收
        int count;
        //缓冲区不够，清除，若依旧不够，只能返回
        //当单条协议超过缓冲区长度时会发生
        if (readBuff.Remain <= 0)
        {
            OnReceiveData(state);
            readBuff.MoveBytes();
        };
        if (readBuff.Remain <= 0)
        {
            PrintM("Receive fail , maybe msg length > buff capacity");
            Close(state);
            return;
        }
        try
        {
            count = clientfd.Receive(readBuff.bytes, readBuff.writeIdx, readBuff.Remain, 0);
        }
        catch (SocketException ex)
        {
            PrintM("Receive SocketException " + ex.ToString());
            Close(state);
            return;
        }
        //客户端关闭
        if (count <= 0)
        {
            PrintM("Socket Close " + clientfd.RemoteEndPoint?.ToString());
            Close(state);
            return;
        }
        //消息处理
        readBuff.writeIdx += count;
        //处理二进制消息
        OnReceiveData(state);
        //移动缓冲区
        readBuff.CheckAndMoveBytes();
    }

    //数据处理
    private void OnReceiveData(Client client)
    {
        ByteArray readBuff = client.readBuff;
        //消息长度
        if (readBuff.Length <= 2)
        {
            return;
        }
        //消息体长度
        int readIdx = readBuff.readIdx;
        byte[] bytes = readBuff.bytes;
        Int16 bodyLength = (Int16)((bytes[readIdx + 1] << 8) | bytes[readIdx]);
        if (readBuff.Length < bodyLength)
        {
            return;
        }
        readBuff.readIdx += 2;
        //解析协议名
        string protoName = ProtocolBase.DecodeName(readBuff.bytes, readBuff.readIdx, out int nameCount);
        if (protoName == "")
        {
            PrintE("OnReceiveData MsgBase.DecodeName fail");
            Close(client);
            return;
        }
        readBuff.readIdx += nameCount;
        //解析协议体
        int bodyCount = bodyLength - nameCount;
        if (bodyCount <= 0)
        {
            PrintE("OnReceiveData fail, bodyCount <=0 ");
            Close(client);
            return;
        }
        ProtocolBase? msg = ProtocolBase.Decode(protoName, readBuff.bytes, readBuff.readIdx, bodyCount);
        readBuff.readIdx += bodyCount;
        readBuff.CheckAndMoveBytes();


        if (msg is null)
        {
            PrintE($"OnReceiveData fail, {protoName} 反序列化失败 ");
        }
        else
        {
            //分发消息
            protEventTable.TryGetValue(protoName, out ProtHandle? @event);
            string info = FireMsg(client, msg);
            PrintM($"{protoName} invoked, {info}");
        }

        //继续读取消息
        if (readBuff.Length > 2)
        {
            OnReceiveData(client);
        }
    }

    //分发消息
    private string FireMsg(Client client, ProtocolBase msg)
    {
        string msgName = msg.GetType().Name;
        protEventTable.TryGetValue(msgName, out ProtHandle? @event);
        if (@event is not null)
        {
            return @event.Invoke(client, msg);
        }
        else
        {
            return $"收到 {msgName} 但对应的事件回调为空！";
        }
    }

    //发送
    public static void Send(Client cs, ProtocolBase msg)
    {
        //状态判断
        if (cs == null)
        {
            return;
        }
        if (!cs.socket.Connected)
        {
            return;
        }
        //数据编码
        byte[] nameBytes = ProtocolBase.EncodeName(msg);
        byte[] bodyBytes = ProtocolBase.Encode(msg);
        int len = nameBytes.Length + bodyBytes.Length;
        byte[] sendBytes = new byte[2 + len];
        //组装长度
        sendBytes[0] = (byte)(len % 256);
        sendBytes[1] = (byte)(len / 256);
        //组装名字
        Array.Copy(nameBytes, 0, sendBytes, 2, nameBytes.Length);
        //组装消息体
        Array.Copy(bodyBytes, 0, sendBytes, 2 + nameBytes.Length, bodyBytes.Length);
        //为简化代码，不设置回调
        try
        {
            cs.socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, null, null);
        }
        catch (SocketException ex)
        {
            PrintM("Socket Close on BeginSend" + ex.ToString());
        }

    }

    //获取时间戳
    public static long GetTimeStamp()
    {
        TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return Convert.ToInt64(ts.TotalSeconds);
    }
}
