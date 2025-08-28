using System;
using System.Collections.Generic;

namespace NetworkBase
{
    public class HeartbeatPacket_Demo : INetPacket
    {
        public byte[] ToBytes()
        {
            return new byte[0];
        }
        public void FromBytes(byte[] data)
        {
            
        }
    }
    public class RTTPacket : INetPacket
    {
        public DateTime sentTime;
        public byte[] ToBytes()
        {
            return BitConverter.GetBytes(sentTime.Ticks);
        }

        public void FromBytes(byte[] data)
        {
            sentTime=new DateTime(BitConverter.ToInt64(data, 0));
        }
    }

    public class PlayerRequestIDPacket : INetPacket
    {
        public byte[] ToBytes()
        {
            return null;
        }
        public void FromBytes(byte[] data)
        {
        }
    }
    public class PlayerIDPacket : INetPacket
    {
        public int PlayerId;
        public byte[] ToBytes()
        {
            return BitConverter.GetBytes(PlayerId);
        }

        public void FromBytes(byte[] data)
        {
            PlayerId=BitConverter.ToInt32(data, 0);
        }
    }
    public class PlayerState: INetPacket
    {
        public int PlayerId;
        public int m_X {  set;  get; }
        public int m_Y{  set;  get; }
        public int m_Z{  set;  get; }
        public float x{get{return (float)m_X/10000;} set{m_X = (int)(value*10000);}}
        public float y{get{return (float)m_Y/10000;} set{m_Y = (int)(value*10000);}}
        public float z{get{return (float)m_Z/10000;} set{m_Z = (int)(value*10000);}}
        public byte[] ToBytes()
        {
            byte[] data = new byte[16];
            Array.Copy(BitConverter.GetBytes(PlayerId),0,data,0,4);
            Array.Copy(BitConverter.GetBytes(m_X),0,data,4,4);
            Array.Copy(BitConverter.GetBytes(m_Y),0,data,8,4);
            Array.Copy(BitConverter.GetBytes(m_Z),0,data,12,4);
            return data;
        }
        public void FromBytes(byte[] data)
        {
            if (data.Length != 16)
            {
                return;
            }
            PlayerId = BitConverter.ToInt32(data, 0);
            m_X = BitConverter.ToInt32(data, 4);
            m_Y = BitConverter.ToInt32(data, 8);
            m_Z = BitConverter.ToInt32(data, 12);
        }
    }
    
    public class StateSyncPacket : INetPacket
    {
        public List<PlayerState> Players;
        public byte[] ToBytes()
        {
            if (Players == null)
            {
                return null;
            }
            byte[] data=new byte[Players.Count*16];
            for (int i = 0; i < Players.Count; i++)
            {
                Array.Copy(Players[i].ToBytes(),0,data,i*16,16);
            }

            return data;
        }

        public void FromBytes(byte[] data)
        {
            if (data.Length % 16 != 0)
            {
                return;
            }
            Players=new List<PlayerState>();
            try
            {
                for (int i = 0; i < data.Length / 16; i++)
                {
                    PlayerState state=new PlayerState();
                    state.PlayerId=BitConverter.ToInt32(data,i*16);
                    state.m_X=BitConverter.ToInt32(data,i*16+4);
                    state.m_Y=BitConverter.ToInt32(data,i*16+8);
                    state.m_Z=BitConverter.ToInt32(data,i*16+12);
                    Players.Add(state);
                }
            }
            catch (Exception e)
            {
                Logger.LogToTerminal(e.Message);
            }
        }
    }
    public class PlayerInputPacket:INetPacket
    {
        public int PlayerId;
        public int m_X {  set;  get; }
        public int m_Z{  set;  get; }
        public float x{get{return (float)m_X/100;} set{m_X = (int)(value*100);}}
        public float z{get{return (float)m_Z/100;} set{m_Z = (int)(value*100);}}
        public byte[] ToBytes()
        {
            byte[] data = new byte[12];
            Array.Copy(BitConverter.GetBytes(PlayerId),0,data,0,4);
            Array.Copy(BitConverter.GetBytes(m_X),0,data,4,4);
            Array.Copy(BitConverter.GetBytes(m_Z),0,data,8,4);
            return data;
        }

        public void FromBytes(byte[] data)
        {
            if (data.Length != 12)
            {
                return;
            }
            PlayerId = BitConverter.ToInt32(data, 0);
            m_X = BitConverter.ToInt32(data, 4);
            m_Z = BitConverter.ToInt32(data, 8);
        }
    }
    
}