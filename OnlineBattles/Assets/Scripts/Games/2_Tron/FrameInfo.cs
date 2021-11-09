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

            Blue = new PlayerInfo(new ModifiedVector3(transform1.position), new ModifiedQuaternion(transform1.rotation));
            Red = new PlayerInfo(new ModifiedVector3(transform2.position), new ModifiedQuaternion(transform2.rotation));

            Ticks = DateTime.UtcNow.Ticks;
            TimeSinceLevelLoad = Time.timeSinceLevelLoad;
        }       
    }

    [Serializable]
    public class PlayerInfo
    {
        public readonly ModifiedVector3 Position;
        public readonly ModifiedQuaternion Rotation;

        public PlayerInfo(ModifiedVector3 position, ModifiedQuaternion rotation)
        {
            Position = position;
            Rotation = rotation;
        }
    }
}