using XRMultiplayer.Packets;

namespace XRMultiplayer.Models
{
    public enum MessageTypes : byte
    {
        PlayerTransform,
        Welcome,
        Disconnect,
        ObjectTransform
    }

    public abstract class BaseMessageType
    {
        public MessageTypes MsgType { get; set; }

        public byte SenderID { get; set; }

        public abstract WrapperPacket Serialize();

        public abstract void Deserialize(byte[] data);
    }
}
