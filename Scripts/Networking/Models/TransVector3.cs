using UnityEngine;

namespace Assets.Scripts.SERVER.Models
{
    public class TransVector3
    {

        public TransVector3(float x, float y, float z)
        {
            TV3 = $"{x};{y};{z}";
        }

        public string TV3 { get; set; }

        public Vector3 GetVector()
        {
            var splitted = TV3.Split(';');
            float x, y, z;
            float.TryParse(splitted[0], out x);
            float.TryParse(splitted[1], out y);
            float.TryParse(splitted[2], out z);
            return new Vector3(x, y, z);
        }

    }

    public static class Vector3Helpers
    {
        public static TransVector3 ToTransVector(this Vector3 value)
        {
            return new TransVector3(value.x, value.y, value.z);
        }
    }
}
