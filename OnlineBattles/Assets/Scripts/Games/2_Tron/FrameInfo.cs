using System;
using UnityEngine;

namespace Game2
{
    [Serializable]
    public class FrameInfo
    {
        public float X_blue { get; private set; }
        public float Y_blue { get; private set; }
        public ModifiedQuaternion Quaternion_blue { get; private set; }

        public float X_red { get; private set; }
        public float Y_red { get; private set; }
        public ModifiedQuaternion Quaternion_red { get; private set; }

        public FrameInfo(Player blue, Player red)
        {
            Transform transform1 = blue.gameObject.transform, transform2 = red.gameObject.transform;

            X_blue = transform1.position.x;
            Y_blue = transform1.position.y;
            Quaternion_blue = new ModifiedQuaternion(transform1.localRotation);

            X_red = transform2.position.x;
            Y_red = transform2.position.y;
            Quaternion_red = new ModifiedQuaternion(transform2.localRotation);
        }

        public Quaternion GetQuaternion(ModifiedQuaternion quaternion)
        {
            return new Quaternion(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);
        }
    }

    [Serializable]
    public class ModifiedQuaternion
    {
        public float X { get; private set; }
        public float Y { get; private set; }
        public float Z { get; private set; }
        public float W { get; private set; }

        public ModifiedQuaternion(Quaternion quaternion)
        {
            X = quaternion.x;
            Y = quaternion.y;
            Z = quaternion.z;
            W = quaternion.w;
        }
    }
}
