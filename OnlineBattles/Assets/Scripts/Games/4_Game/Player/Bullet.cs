using UnityEngine;

namespace Game4
{
    public class Bullet : MonoBehaviour
    {
        [SerializeField] private float _speed = 3f;

        public Vector3 Direction => _direction;

        private Vector3 _direction = Vector3.zero;
        private bool _getDirection = false;

        private void Start()
        {
            if (_getDirection == false)
            {
                if (transform.position.y < 0)
                    _direction = Vector3.up;
                else
                    _direction = Vector3.down;
            }
        }        

        private void Update()
        {
            transform.Translate(_direction * _speed * Time.deltaTime);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.TryGetComponent(out Bullet bullet)) 
            {
                Destroy(gameObject);
            }
        }

        public void ChangeDirectionAngel(float angel, Vector3 dir)
        {
            _direction = Quaternion.AngleAxis(angel, Vector3.forward) * dir;
            _getDirection = true;  // Чтоб клоны пропустили  назначение скорости в Старте
        }

        public void ReflectDirection(Vector2 normal) 
        {  
            Vector2 newDirection = Vector2.Reflect(_direction, normal);
            _direction = newDirection;
        }

        public void AddSpeed(bool plusSign)
        {
            float step = 1.5f;

            if (plusSign == true)
                _speed += step;
            else
                _speed -= step;
        }
    }
}
