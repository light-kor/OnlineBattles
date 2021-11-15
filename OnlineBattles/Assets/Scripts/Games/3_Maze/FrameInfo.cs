using System;

namespace Game3
{
    [Serializable]
    public class FrameInfo
    {
        public readonly ModifiedVector3 Blue;
        public readonly ModifiedVector3 Red;

        public FrameInfo(Player blue, Player red)
        {
            Blue = new ModifiedVector3(blue.transform.position);
            Red = new ModifiedVector3(red.transform.position);
        }
    }
}