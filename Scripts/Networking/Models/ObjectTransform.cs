using Assets.FlatBuffers;
using FlatBuffers;
using hololensMulti;
using hololensMultiplayer.Packets;
using LiteNetLib;
using UnityEngine;

namespace hololensMultiplayer.Models
{
    public class ObjectTransform : BaseMessageType
    {
        public int ObjectID { get; set; }
        public string ObjectType { get; set; } = "PrefabName";
        public byte OwnerID { get; set; } = 0;
        public Vector3 Pos { get; set; } = Vector3.zero;
        public Quaternion Rot { get; set; } = Quaternion.identity;
        public Vector3 Scale { get; set; } = Vector3.one;

        public new MessageTypes MsgType = MessageTypes.ObjectTransform;

        public ObjectTransform() { }

        public ObjectTransform(byte[] data)
        {
            Deserialize(data);
        }

        public override WrapperPacket Serialize()
        {
            var builder = new FlatBufferBuilder(1);
            var objectTypeOffset = builder.CreateString(ObjectType);

            ObjectFB.StartObjectFB(builder);

            ObjectFB.AddObjectID(builder, ObjectID);
            ObjectFB.AddObjectType(builder, objectTypeOffset);
            ObjectFB.AddPos(builder, Pos.ToVec3(builder));
            ObjectFB.AddRot(builder, Rot.ToQuat(builder));
            ObjectFB.AddScale(builder, Scale.ToVec3(builder));
            ObjectFB.AddOwnerID(builder, OwnerID);

            var offset = ObjectFB.EndObjectFB(builder);
            ObjectFB.FinishObjectFBBuffer(builder, offset);

            return new WrapperPacket(MsgType, builder.SizedByteArray(), DeliveryMethod.Unreliable);
        }

        public override void Deserialize(byte[] data)
        {
            ByteBuffer bb = new ByteBuffer(data);
            ObjectFB objectFB = ObjectFB.GetRootAsObjectFB(bb);

            ObjectID = objectFB.ObjectID;
            OwnerID = objectFB.OwnerID;
            ObjectType = objectFB.ObjectType;

            if (objectFB.Pos.HasValue)
                Pos = objectFB.Pos.ToVector3();

            if (objectFB.Rot.HasValue)
                Rot = objectFB.Rot.ToQuaternion();

            if (objectFB.Scale.HasValue)
                Scale = objectFB.Scale.ToVector3();
        }
    }

}
