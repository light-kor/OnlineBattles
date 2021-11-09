using System;
using UnityEngine;

namespace Game4
{
    [Serializable]
    public class FrameInfo
    {
        public readonly ModifiedVector3 Blue_Pos;
        public readonly ModifiedVector3 Red_Pos;
        public readonly long Ticks;
        public readonly float TimeSinceLevelLoad;

        public FrameInfo(Player blue, Player red)
        {
            Blue_Pos = new ModifiedVector3(blue.gameObject.transform.position);
            Red_Pos = new ModifiedVector3(red.gameObject.transform.position);
          
            Ticks = DateTime.UtcNow.Ticks;
            TimeSinceLevelLoad = Time.timeSinceLevelLoad;
        }
    }
}
