using FlatBuffers;
using hololensMulti;
using hololensMultiplayer.Packets;

namespace hololensMultiplayer.Models
{
    public class DisconnectMessage : BaseMessageType
    {
        public byte DisconnectedUserID { get; set; }

        public new MessageTypes MsgType = MessageTypes.Disconnect;

        public DisconnectMessage() { }

        public DisconnectMessage(byte[] data)
        {
            Deserialize(data);
        }

        public override WrapperPacket Serialize()
        {
            var builder = new FlatBufferBuilder(1);

            DisconnectFB.StartDisconnectFB(builder);

            DisconnectFB.AddPlayerID(builder, DisconnectedUserID);

            var offset = DisconnectFB.EndDisconnectFB(builder);
            DisconnectFB.FinishDisconnectFBBuffer(builder, offset);

            return new WrapperPacket(MsgType, builder.SizedByteArray());
        }

        public override void Deserialize(byte[] data)
        {
            ByteBuffer bb = new ByteBuffer(data);
            DisconnectFB disconnectFB = DisconnectFB.GetRootAsDisconnectFB(bb);

            SenderID = disconnectFB.PlayerID;
            DisconnectedUserID = disconnectFB.PlayerID;
        }
    }

}
