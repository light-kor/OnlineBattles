using System;
using UnityEngine;

namespace Game2
{
    [Serializable]
    public class FrameInfo
    {
        public readonly PlayerInfo Blue;
        public readonly PlayerInfo Red;
        public readonly long Ticks;
        public readonly float TimeSinceLevelLoad;

        public FrameInfo(Player blue, Player red)
        {
            Transform transform1 = blue.gameObject.transform, transform2 = red.gameObject.transform;
            ModifiedQuaternion rotation_blue = new ModifiedQuaternion(transform1.localRotation);
            ModifiedQuaternion rotation_red = new ModifiedQuaternion(transform2.localRotation);

            Blue = new PlayerInfo(transform1.position.x, transform1.position.y, rotation_blue);
            Red = new PlayerInfo(transform2.position.x, transform2.position.y, rotation_red);

            Ticks = DateTime.UtcNow.Ticks;
            TimeSinceLevelLoad = Time.timeSinceLevelLoad;
        }       
    }

    [Serializable]
    public class ModifiedQuaternion
    {
        public readonly float X;
        public readonly float Y;
        public readonly float Z;
        public readonly float W;

        public ModifiedQuaternion(Quaternion quaternion)
        {
            X = quaternion.x;
            Y = quaternion.y;
            Z = quaternion.z;
            W = quaternion.w;           
        }
    }

    [Serializable]
    public class PlayerInfo
    {
        public readonly float X_pos;
        public readonly float Y_pos;
        public readonly ModifiedQuaternion Rotation;

        public PlayerInfo(float x, float y, ModifiedQuaternion rotation)
        {
            X_pos = x;
            Y_pos = y;
            Rotation = rotation;
        }

        public Quaternion GetQuaternion()
        {
            return new Quaternion(Rotation.X, Rotation.Y, Rotation.Z, Rotation.W);
        }

        public Vector2 GetPosition()
        {
            return new Vector2(X_pos, Y_pos);
        }
    }
}
