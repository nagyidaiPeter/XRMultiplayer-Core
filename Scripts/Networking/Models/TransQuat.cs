using UnityEngine;

namespace Assets.Scripts.SERVER.Models
{
    public class TransQuat
    {

        public TransQuat(float x, float y, float z, float w)
        {
            TV3 = $"{x};{y};{z};{w}";
        }

        public string TV3 { get; set; }

        public Quaternion GetQuaternion()
        {
            var splitted = TV3.Split(';');
            float x, y, z, w;
            float.TryParse(splitted[0], out x);
            float.TryParse(splitted[1], out y);
            float.TryParse(splitted[2], out z);
            float.TryParse(splitted[3], out w);
            return new Quaternion(x, y, z, w);
        }

    }

    public static class QuaternionHelpers
    {
        public static TransQuat ToTransQuat(this Quaternion value)
        {
            return new TransQuat(value.x, value.y, value.z, value.w);
        } 
    }
}
