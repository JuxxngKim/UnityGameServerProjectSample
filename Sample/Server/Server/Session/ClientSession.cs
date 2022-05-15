using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ServerCore;
using System.Net;
using Google.Protobuf.Protocol;
using Google.Protobuf;
using Server.Game;
using Server.Data;
using UnityEngine;

namespace Server
{
    public class ClientSession : PacketSession
    {
        public Player MyPlayer { get; set; }
        public int SessionId { get; set; }

        object _lock = new object();
        List<ArraySegment<byte>> _reserveQueue = new List<ArraySegment<byte>>();

        long _pingpongTick = 0;
        public void Ping()
        {
            if (_pingpongTick > 0)
            {
                long delta = (System.Environment.TickCount64 - _pingpongTick);
                if (delta > 300 * 1000)
                {
                    Console.WriteLine("Disconnected by PingCheck");
                    Disconnect();
                    return;
                }
            }

            S_Ping pingPacket = new S_Ping();
            pingPacket.Time = System.DateTime.UtcNow.Ticks;
            Send(pingPacket);

            GameLogic.Instance.PushAfter(5000, Ping);
        }

        public void HandlePong()
        {
            _pingpongTick = System.Environment.TickCount64;
        }

        public void Send(IMessage packet)
        {
            string msgName = packet.Descriptor.Name.Replace("_", string.Empty);
            MsgId msgId = (MsgId)Enum.Parse(typeof(MsgId), msgName);
            ushort size = (ushort)packet.CalculateSize();
            byte[] sendBuffer = new byte[size + 4];
            Array.Copy(BitConverter.GetBytes((ushort)(size + 4)), 0, sendBuffer, 0, sizeof(ushort));
            Array.Copy(BitConverter.GetBytes((ushort)msgId), 0, sendBuffer, 2, sizeof(ushort));
            Array.Copy(packet.ToByteArray(), 0, sendBuffer, 4, size);

            lock (_lock)
            {
                _reserveQueue.Add(sendBuffer);
            }
        }

        public void FlushSend()
        {
            List<ArraySegment<byte>> sendList = null;

            lock (_lock)
            {
                if (_reserveQueue.Count <= 0)
                    return;

                sendList = _reserveQueue;
                _reserveQueue = new List<ArraySegment<byte>>();
            }

            Send(sendList);
        }

        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected : {endPoint}");

            GameLogic.Instance.Push(() =>
            {
                GameRoom room = GameLogic.Instance.Find(1);
                Vector3 spawnPos = new Vector3(144f, 0.0f, 160f);

                MyPlayer = ObjectManager.Instance.Add<Player>();
                {
                    MyPlayer.Info.Name = $"Player_{MyPlayer.Info.ObjectId}";
                    MyPlayer.Info.PosInfo.State = ActorState.Idle;
                    MyPlayer.Info.PosInfo.Direction = Vector3.down.ToFloat3();
                    MyPlayer.Info.PosInfo.Position = spawnPos.ToFloat3();
                    MyPlayer.Info.TeamType = Const.PlayerTeamType;

                    StatInfo stat = DataPresets.MakeGanyuStat(level: 1);
                    MyPlayer.Stat.MergeFrom(stat);

                    MyPlayer.Session = this;

                    MyPlayer.SyncPos();
                    MyPlayer.Init(room.Level);
                }

                room.Push(room.EnterGame, MyPlayer, Const.PlayerTeamType);
            });

            Ping();
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            PacketManager.Instance.OnRecvPacket(this, buffer);
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            GameLogic.Instance.Push(() =>
            {
                if (MyPlayer == null)
                    return;

                GameRoom room = GameLogic.Instance.Find(1);
                room.Push(room.LeaveGame, MyPlayer.Info.ObjectId);
            });

            SessionManager.Instance.Remove(this);
        }

        public override void OnSend(int numOfBytes)
        {
            //Console.WriteLine($"Transferred bytes: {numOfBytes}");
        }
    }
}
