namespace NetworkBase
{
    public class InputSynchronizationFactory : PacketFactoryBase
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
            PacketTypeDic.Add(7,typeof(InputSyncPacket));
            PacketTypeDic.Add(8,typeof(DropClientPacket));
            PacketTypeDic.Add(9,typeof(RegisterNewClientPacket));
            return true;
        }
    }
}