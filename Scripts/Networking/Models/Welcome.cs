using FlatBuffers;
using hololensMulti;
using hololensMultiplayer.Packets;

namespace hololensMultiplayer.Models
{
    public class Welcome : BaseMessageType
    {
        public string Name { get; set; } = "Player";

        public new MessageTypes MsgType = MessageTypes.Welcome;

        public Welcome() { }

        public Welcome(byte[] data)
        {
            Deserialize(data);
        }

        public override WrapperPacket Serialize()
        {
            var builder = new FlatBufferBuilder(1);
            var playerName = builder.CreateString(Name);

            WelcomeFB.StartWelcomeFB(builder);

            WelcomeFB.AddPlayerID(builder, SenderID);
            WelcomeFB.AddPlayerName(builder, playerName);

            var offset = WelcomeFB.EndWelcomeFB(builder);
            WelcomeFB.FinishWelcomeFBBuffer(builder, offset);

            return new WrapperPacket(MsgType, builder.SizedByteArray());
        }

        public override void Deserialize(byte[] data)
        {
            ByteBuffer bb = new ByteBuffer(data);
            WelcomeFB welcomeFB = WelcomeFB.GetRootAsWelcomeFB(bb);

            SenderID = welcomeFB.PlayerID;
            Name = welcomeFB.PlayerName;
        }
    }
}
