using FlatBuffers;
using hololensMulti;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.FlatBuffers
{
    public static class FlatBuffersExtensions
    {
        public static Offset<Vec3> ToVec3(this Vector3 vector, FlatBufferBuilder builder)
        {
            return Vec3.CreateVec3(builder, vector.x, vector.y, vector.z);
        }

        public static Offset<Quat> ToQuat(this Quaternion quaternion, FlatBufferBuilder builder)
        {
            return Quat.CreateQuat(builder, quaternion.x, quaternion.y, quaternion.z, quaternion.w);
        }

        public static Vector3 ToVector3(this Vec3 vec)
        {
            return new Vector3(vec.X, vec.Y, vec.Z);
        }

        public static Quaternion ToQuaternion(this Quat quat)
        {
            return new Quaternion( quat.X, quat.Y, quat.Z, quat.W);
        }

        public static Vector3 ToVector3(this Vec3? vec)
        {
            return new Vector3(vec.Value.X, vec.Value.Y, vec.Value.Z);
        }

        public static Quaternion ToQuaternion(this Quat? quat)
        {
            return new Quaternion(quat.Value.X, quat.Value.Y, quat.Value.Z, quat.Value.W);
        }
    }
}
