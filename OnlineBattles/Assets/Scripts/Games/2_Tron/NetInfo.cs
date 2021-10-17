using System;
using UnityEngine;

namespace Game2
{
    [Serializable]
    public class NetInfo
    {
        public float X_pos1 { get; private set; }
        public float Y_pos1 { get; private set; }
        public float Z_rotation1 { get; private set; }

        public float X_pos2 { get; private set; }
        public float Y_pos2 { get; private set; }
        public float Z_rotation2 { get; private set; }

        public NetInfo(Player player1, Player player2)
        {
            Transform transform1 = player1.gameObject.transform, transform2 = player2.gameObject.transform;

            X_pos1 = transform1.position.x;
            Y_pos1 = transform1.position.y;
            Z_rotation1 = transform1.rotation.z;

            X_pos2 = transform2.position.x;
            Y_pos2 = transform2.position.y;
            Z_rotation2 = transform2.rotation.z;
        }
    }
}
