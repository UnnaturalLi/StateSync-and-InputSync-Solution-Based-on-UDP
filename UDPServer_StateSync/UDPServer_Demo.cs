using NetworkBase;
using System;
using System.Collections.Generic;

namespace UDPServer_StateSync
{
    public class UDPServer_Demo : UDPServer
    {
        public StateSyncPacket packet;
        public int Speed=5;
        public int recvinputs=0;
        protected override bool OnInit()
        {
            packet = new StateSyncPacket();
            packet.Players = new List<PlayerState>();
            return base.OnInit();
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
                
                PlayerState player=packet.Players.Find(p => p.PlayerId == id);
                if (player == null)
                {
                    Logger.LogToTerminal("Cannot find player");
                    throw new Exception("Cannot find player");
                }
                player.m_X += ((PlayerInputPacket)obj).m_X * Speed * 2;
                player.m_Z += ((PlayerInputPacket)obj).m_Z * Speed * 2;
                packet.TimeStamp=DateTime.Now;
                Broadcast(packet);
            }
        }
        public override void OnRegisterClient(int id)
        {
            SendTo(id,new PlayerIDPacket{PlayerId=id});
            packet.Players.Add(new PlayerState{PlayerId = id,m_X = 0,m_Y = 0,m_Z = 0});
            packet.TimeStamp=DateTime.Now;
            Broadcast(packet);
        }

        public override void OnDropClient(int id)
        {
            try
            {
                var ps = packet.Players.Find((p) => p.PlayerId == id);
                packet.Players.Remove(ps);
                packet.TimeStamp=DateTime.Now;
                Logger.LogToTerminal($"Remove client {id}, {packet.Players.Count}s left");
                Broadcast(packet);
            }
            catch (Exception e)
            {
                Logger.LogToTerminal(e.Message);
            }
        }
    }
}