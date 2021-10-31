using hololensMultiplayer.Models;
using LiteNetLib;

namespace hololensMultiplayer.Packets
{

    public enum ChannelId : byte
    {
        DEFAULT = 0,
        PLAYER = 1,
        OBJECT = 2
    }

    public class WrapperPacket
    {
        public MessageTypes messageType { get; set; }

        public byte[] packetData { get; set; }

        public ChannelId UdpChannel = ChannelId.DEFAULT;
        public DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered;

        public WrapperPacket()
        {
            
        }

        public WrapperPacket(MessageTypes messageType)
        {
            this.messageType = messageType;
        }

        public WrapperPacket(MessageTypes messageType, byte[] packetData, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered)
        {
            this.messageType = messageType;
            this.packetData = packetData;
            this.deliveryMethod = deliveryMethod;
        }
    }
}
