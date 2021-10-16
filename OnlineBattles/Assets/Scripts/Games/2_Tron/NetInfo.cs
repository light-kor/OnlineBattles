using System;
using UnityEngine;

namespace Game2
{
    [Serializable]
    public class NetInfo
    {
        private Vector3 _firstPosition, _secondPosition;
        private Quaternion _firstRotation, _secondRotation;

        public NetInfo(Player player1, Player player2)
        {
            _firstPosition = player1.gameObject.transform.position;
            _secondPosition = player2.gameObject.transform.position;

            _firstRotation = player1.gameObject.transform.rotation;
            _secondRotation = player2.gameObject.transform.rotation;
        }
    }
}
