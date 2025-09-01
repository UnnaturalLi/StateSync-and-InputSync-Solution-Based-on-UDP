using System.Collections.Generic;
using System.Threading;
using UDPServer_StateSync;
using NetworkBase;
using System;
namespace UDPServer_InputSync
{
    public class UDPServer_Demo_InputSync: UDPServer
    {
        public InputSyncPacket packet;
        public StateSyncPacket StatePacket;
        public Thread BroadcastThread;
        protected override bool OnInit()
        {
            packet = new InputSyncPacket();
            packet.PlayersInput = new List<PlayerInputPacket>();
            StatePacket = new StateSyncPacket();
            StatePacket.Players = new List<PlayerState>();
            BroadcastThread = new Thread(BroadCastInputPacket)
            {
                IsBackground = true,
                Priority = ThreadPriority.Normal
            };
            return base.OnInit();
        }

        protected override void OnStart()
        {
            base.OnStart();
            BroadcastThread.Start();
        }

        protected override void OnStop()
        {
            BroadcastThread.Abort();
            base.OnStop();
        }

        public override void OnReceiveObj(int id, INetPacket obj)
        {
            if (obj.GetType() == typeof(RTTPacket))
            {
                SendTo(id,obj);
            }else if (obj.GetType() == typeof(PlayerRequestIDPacket))
            {
                SendTo(id,new PlayerIDPacket { PlayerId = id }); 
            }else if (obj.GetType() == typeof(PlayerInputPacket))
            {
                var player=StatePacket.Players.Find(p => p.PlayerId == id);
                player.m_X += (obj as PlayerInputPacket).m_X * 10;
                player.m_Z += (obj as PlayerInputPacket).m_Z * 10;
                lock (packet)
                {
                    packet.PlayersInput.Add(obj as PlayerInputPacket);
                }
            }
        }
        public override void OnRegisterClient(int id)
        {
            SendTo(id,new PlayerIDPacket{PlayerId=id});
            StatePacket.Players.Add(new PlayerState { PlayerId = id });
            Broadcast(StatePacket);
        }

        public override void OnDropClient(int id){
            Logger.LogToTerminal($"Client Dropped{id}");
            DropClientPacket dropPacket = new DropClientPacket();
            dropPacket.idList=new List<int>();
            dropPacket.idList.Add(id);
            var player=StatePacket.Players.Find(p => p.PlayerId == id);
            StatePacket.Players.Remove(player);
            Broadcast(dropPacket);
        }

        public void BroadCastInputPacket()
        {
            while (true)
            {
                lock (packet)
                {
                    Broadcast(packet);
                    packet.PlayersInput.Clear();
                }
                Thread.Sleep(20);
            }
        }
    }
}