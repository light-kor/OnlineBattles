using System;
using UnityEngine;

namespace Game4
{
    public class InteractiveObject : MonoBehaviour
    {
        private Action<Point> _releasePoint;
        private Point _point;

        public void SetObject(Point point, Action<Point> releasePoint)
        {
            _point = point;
            _releasePoint = releasePoint;
            gameObject.SetActive(true);
            transform.position = point.TakePosition();
        }

        public void ReleaseObject()
        {
            _releasePoint(_point);
            gameObject.SetActive(false);
        }
    }
}
