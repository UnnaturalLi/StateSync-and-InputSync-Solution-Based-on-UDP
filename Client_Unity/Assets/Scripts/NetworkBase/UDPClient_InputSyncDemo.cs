using System;
using NetworkBase;
using UnityEngine;

namespace UDPClient
{
    public class UDPClient_InputSyncDemo: UDPClient
    {
        public InputSyncManager manager;
        
        public UDPClient_InputSyncDemo(InputSyncManager manager)
        {
            this.manager = manager;
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
                        
                    }else if (data.GetType() == typeof(InputSyncPacket))
                    {
                        manager.SyncDataQueue.Enqueue(data as InputSyncPacket);
                        manager.NewPacket = true;
                    }else if (data.GetType() == typeof(DropClientPacket))
                    {
                        manager.DropClientQueue.Enqueue(data as DropClientPacket);
                    }else if (data.GetType() == typeof(RegisterNewClientPacket))
                    {
                        manager.RegisterNewClient(data as RegisterNewClientPacket);
                    }else if (data.GetType() == typeof(StateSyncPacket))
                    {
                        manager.StateDataQueue.Enqueue(data as StateSyncPacket);
                        manager.NewPacket = true;
                    }
                }
            }
        }
        
    }
}