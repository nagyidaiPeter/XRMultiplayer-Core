
using Assets.FlatBuffers;
using FlatBuffers;
using hololensMulti;
using hololensMultiplayer.Models;
using hololensMultiplayer.Packets;
using LiteNetLib;
using System.Collections.Generic;
using UnityEngine;

namespace hololensMultiModels
{
    public class PlayerTransform : BaseMessageType
    {
        public Vector3 Pos { get; set; } = Vector3.zero;
        public Quaternion Rot { get; set; } = Quaternion.identity;
        public Vector3 QrRotationOffset { get; set; } = Vector3.zero;

        public bool RHActive { get; set; }
        public FingersState RHFingers { get; set; }
        public Vector3 RHPos { get; set; } = Vector3.zero;
        public Quaternion RHRot { get; set; } = Quaternion.identity;
        public Vector3 RHPointerPos { get; set; } = Vector3.zero;

        public bool LHActive { get; set; }
        public FingersState LHFingers { get; set; }
        public Vector3 LHPos { get; set; } = Vector3.zero;
        public Quaternion LHRot { get; set; } = Quaternion.identity;
        public Vector3 LHPointerPos { get; set; } = Vector3.zero;

        public new MessageTypes MsgType = MessageTypes.PlayerTransform;

        public PlayerTransform()
        {

        }

        public PlayerTransform(byte[] data)
        {
            Deserialize(data);
        }

        public override WrapperPacket Serialize()
        {
            var builder = new FlatBufferBuilder(1);
            TransformFB.StartTransformFB(builder);

            TransformFB.AddPlayerID(builder, SenderID);

            TransformFB.AddPos(builder, Pos.ToVec3(builder));
            TransformFB.AddRot(builder, Rot.ToQuat(builder));
            TransformFB.AddQrOffset(builder, QrRotationOffset.ToVec3(builder));

            TransformFB.AddRHActive(builder, RHActive);
            TransformFB.AddRHState(builder, HandState.CreateHandState(builder, RHFingers.Pinky, RHFingers.Ring, RHFingers.Middle, RHFingers.Index, RHFingers.Thumb));
            TransformFB.AddRHPos(builder, RHPos.ToVec3(builder));
            TransformFB.AddRHRot(builder, RHRot.ToQuat(builder));
            TransformFB.AddRHPointerPos(builder, RHPointerPos.ToVec3(builder));

            TransformFB.AddLHActive(builder, LHActive);
            TransformFB.AddLHState(builder, HandState.CreateHandState(builder, LHFingers.Pinky, LHFingers.Ring, LHFingers.Middle, LHFingers.Index, LHFingers.Thumb));
            TransformFB.AddLHPos(builder, LHPos.ToVec3(builder));
            TransformFB.AddLHRot(builder, LHRot.ToQuat(builder));
            TransformFB.AddLHPointerPos(builder, LHPointerPos.ToVec3(builder));

            var offset = TransformFB.EndTransformFB(builder);
            TransformFB.FinishTransformFBBuffer(builder, offset);

            return new WrapperPacket(MsgType, builder.SizedByteArray(), DeliveryMethod.Unreliable);
        }

        public static List<PlayerTransform> DeserializeStack(byte[] data)
        {
            List<PlayerTransform> playerTransforms = new List<PlayerTransform>();

            ByteBuffer bb = new ByteBuffer(data);
            PlayerTransformStack transformStack = PlayerTransformStack.GetRootAsPlayerTransformStack(bb);

            for (int i = 0; i < transformStack.PlayerTransformsLength; i++)
            {
                var transformData = transformStack.PlayerTransforms(i);
                if (transformData is TransformFB transformFB)
                {
                    PlayerTransform playerTransform = new PlayerTransform();
                    playerTransform.Deserialize(transformFB);
                    playerTransforms.Add(playerTransform);
                }
            }

            return playerTransforms;
        }

        public static List<WrapperPacket> Serialize(List<PlayerTransform> transforms)
        {
            List<WrapperPacket> PlayerStacks = new List<WrapperPacket>();
            var builder = new FlatBufferBuilder(1);

            int counter = 0;
            int allCounter = 0;
            int MaxStackSize = 10;
            List<Offset<TransformFB>> playerStack = new List<Offset<TransformFB>>();

            foreach (var playerTransform in transforms)
            {
                TransformFB.StartTransformFB(builder);

                TransformFB.AddPlayerID(builder, playerTransform.SenderID);

                TransformFB.AddPos(builder, playerTransform.Pos.ToVec3(builder));
                TransformFB.AddRot(builder, playerTransform.Rot.ToQuat(builder));
                TransformFB.AddQrOffset(builder, playerTransform.QrRotationOffset.ToVec3(builder));

                TransformFB.AddRHActive(builder, playerTransform.RHActive);
                TransformFB.AddRHState(builder, HandState.CreateHandState(builder, playerTransform.RHFingers.Pinky, playerTransform.RHFingers.Ring, playerTransform.RHFingers.Middle, playerTransform.RHFingers.Index, playerTransform.RHFingers.Thumb));
                TransformFB.AddRHPos(builder, playerTransform.RHPos.ToVec3(builder));
                TransformFB.AddRHRot(builder, playerTransform.RHRot.ToQuat(builder));
                TransformFB.AddRHPointerPos(builder, playerTransform.RHPointerPos.ToVec3(builder));

                TransformFB.AddLHActive(builder, playerTransform.LHActive);
                TransformFB.AddLHState(builder, HandState.CreateHandState(builder, playerTransform.LHFingers.Pinky, playerTransform.LHFingers.Ring, playerTransform.LHFingers.Middle, playerTransform.LHFingers.Index, playerTransform.LHFingers.Thumb));
                TransformFB.AddLHPos(builder, playerTransform.LHPos.ToVec3(builder));
                TransformFB.AddLHRot(builder, playerTransform.LHRot.ToQuat(builder));
                TransformFB.AddLHPointerPos(builder, playerTransform.LHPointerPos.ToVec3(builder));

                playerStack.Add(TransformFB.EndTransformFB(builder));

                counter++;
                allCounter++;

                if (counter >= MaxStackSize || allCounter == transforms.Count)
                {
                    counter = 0;

                    var vectorOffset = PlayerTransformStack.CreatePlayerTransformsVector(builder, playerStack.ToArray());

                    PlayerTransformStack.StartPlayerTransformStack(builder);
                    PlayerTransformStack.AddPlayerTransforms(builder, vectorOffset);

                    var offset = PlayerTransformStack.EndPlayerTransformStack(builder);

                    PlayerTransformStack.FinishPlayerTransformStackBuffer(builder, offset);

                    WrapperPacket packet = new WrapperPacket(playerTransform.MsgType, builder.SizedByteArray(), DeliveryMethod.Unreliable);
                    PlayerStacks.Add(packet);

                    playerStack = new List<Offset<TransformFB>>();

                    //Maybe unnecessary, TODO: check if it works without this
                    builder = new FlatBufferBuilder(1);
                }
            }

            return PlayerStacks;
        }

        public override void Deserialize(byte[] data)
        {
            ByteBuffer bb = new ByteBuffer(data);
            TransformFB transformFB = TransformFB.GetRootAsTransformFB(bb);

            Deserialize(transformFB);
        }

        public void Deserialize(TransformFB transformFB)
        {
            SenderID = transformFB.PlayerID;
            RHActive = transformFB.RHActive;
            LHActive = transformFB.LHActive;

            if (transformFB.Pos.HasValue)
                Pos = transformFB.Pos.ToVector3();

            if (transformFB.Rot.HasValue)
                Rot = transformFB.Rot.ToQuaternion();

            if (transformFB.QrOffset.HasValue)
                QrRotationOffset = transformFB.QrOffset.ToVector3();

            if (transformFB.RHState.HasValue)
                RHFingers = new FingersState(transformFB.RHState.Value.Pinky, transformFB.RHState.Value.Ring, transformFB.RHState.Value.Middle, transformFB.RHState.Value.Index, transformFB.RHState.Value.Thumb);

            if (transformFB.RHPos.HasValue)
                RHPos = transformFB.RHPos.ToVector3();

            if (transformFB.RHRot.HasValue)
                RHRot = transformFB.RHRot.ToQuaternion();

            if (transformFB.RHPointerPos.HasValue)
                RHPointerPos = transformFB.RHPointerPos.ToVector3();

            if (transformFB.LHState.HasValue)
                LHFingers = new FingersState(transformFB.LHState.Value.Pinky, transformFB.LHState.Value.Ring, transformFB.LHState.Value.Middle, transformFB.LHState.Value.Index, transformFB.LHState.Value.Thumb);

            if (transformFB.LHPos.HasValue)
                LHPos = transformFB.LHPos.ToVector3();

            if (transformFB.LHRot.HasValue)
                LHRot = transformFB.LHRot.ToQuaternion();

            if (transformFB.LHPointerPos.HasValue)
                LHPointerPos = transformFB.LHPointerPos.ToVector3();
        }
    }

}
