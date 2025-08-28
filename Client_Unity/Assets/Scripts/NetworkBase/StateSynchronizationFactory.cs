namespace NetworkBase
{
    public class StateSynchronizationFactory: PacketFactoryBase
    {
        protected override bool OnInit()
        {
            PacketTypeDic.Add(0,typeof(HeartbeatPacket_Demo));
            PacketTypeDic.Add(1,typeof(RTTPacket));
            PacketTypeDic.Add(2,typeof(PlayerRequestIDPacket));
            PacketTypeDic.Add(3,typeof(PlayerIDPacket));
            PacketTypeDic.Add(4,typeof(PlayerState));
            PacketTypeDic.Add(5,typeof(StateSyncPacket));
            PacketTypeDic.Add(6,typeof(PlayerInputPacket));
            return true;
        }
    }
    
}