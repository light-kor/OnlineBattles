using GameEnumerations;
using UnityEngine;

namespace Game4
{
    public class Bullet : MonoBehaviour
    {    
        public Vector3 Direction => _direction;

        private float _speed = NormalSpeed;
        private Vector3 _direction = Vector3.zero;
        private bool _hitFirstObject = false;

        private const float NormalSpeed = 5.5f;

        private void Update()
        {
            if (GeneralController.GameOn)
                transform.Translate(_direction * _speed * Time.deltaTime);
        }

        public void SetStartBullet(PlayerTypes playerType)
        {
            if (playerType == PlayerTypes.BluePlayer)
                _direction = Vector3.up;
            else if (playerType == PlayerTypes.RedPlayer)
                _direction = Vector3.down;

            _hitFirstObject = false;
            _speed = NormalSpeed;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.TryGetComponent(out Bullet bullet)) 
            {
                gameObject.SetActive(false);
            }
        }

        public void ChangeDirectionAngel(float angel, Vector3 dir)
        {
            _direction = Quaternion.AngleAxis(angel, Vector3.forward) * dir;
        }

        public void ReflectDirection(Vector2 normal) 
        {  
            Vector2 newDirection = Vector2.Reflect(_direction, normal);
            _direction = newDirection;
        }

        public void SpeedDown()
        {
            _speed = NormalSpeed / 2;
        }

        private void TrySpeedUp()
        {
            if (_speed == NormalSpeed)
                _speed = NormalSpeed * 1.3f;
        }

        public bool TryReleaseFirstBlock()
        {
            if (_hitFirstObject == false)
            {
                TrySpeedUp();
                _hitFirstObject = true;
                return true;
            }
            else return false;
        }
    }
}
