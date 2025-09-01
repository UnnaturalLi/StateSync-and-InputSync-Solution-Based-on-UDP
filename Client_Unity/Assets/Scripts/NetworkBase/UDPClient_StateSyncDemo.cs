using System;
using NetworkBase;
using UnityEngine;

namespace UDPClient
{
    public class UDPClient_StateSyncDemo: UDPClient
    {
        public StateSyncManager manager;

        public UDPClient_StateSyncDemo(StateSyncManager ma)
        {
            manager = ma;
        }
        protected override void OnReceive()
        {
            lock (dataQueue)
            {
                while (dataQueue.Count > 0)
                {
                    var data = dataQueue.Dequeue();
                    if (data.GetType() == typeof(HeartbeatPacket_Demo))
                    {
                        
                        Send(data);
                    } else if (data.GetType() == typeof(RTTPacket))
                    {
                        Debug.Log($"RTT:{(DateTime.Now-((RTTPacket)data).sentTime).TotalMilliseconds} ms");
                    } else if (data.GetType() == typeof(PlayerIDPacket))
                    {
                        manager.SetPlayerID(((PlayerIDPacket)data).PlayerId);
                    }else if (data.GetType() == typeof(PlayerState))
                    {
                        
                    }else if (data.GetType() == typeof(StateSyncPacket))
                    {
                        manager.SyncDataQueue.Enqueue(data as StateSyncPacket);
                        manager.NewPacket = true;
                    }
                }
            }
        }
        
    }
}