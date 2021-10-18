using System;
using UnityEngine;

namespace Game2
{
    [Serializable]
    public class FrameInfo
    {
        public float X_pos1 { get; private set; }
        public float Y_pos1 { get; private set; }
        public ModifiedQuaternion Quaternion1 { get; private set; }

        public float X_pos2 { get; private set; }
        public float Y_pos2 { get; private set; }
        public ModifiedQuaternion Quaternion2 { get; private set; }

        public FrameInfo(Player player1, Player player2)
        {
            Transform transform1 = player1.gameObject.transform, transform2 = player2.gameObject.transform;

            X_pos1 = transform1.position.x;
            Y_pos1 = transform1.position.y;
            Quaternion1 = new ModifiedQuaternion(transform1.localRotation);

            X_pos2 = transform2.position.x;
            Y_pos2 = transform2.position.y;
            Quaternion2 = new ModifiedQuaternion(transform2.localRotation);
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
