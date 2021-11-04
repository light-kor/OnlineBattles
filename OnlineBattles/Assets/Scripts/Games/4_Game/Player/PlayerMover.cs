using GameEnumerations;
using System;
using System.Collections;
using UnityEngine;

namespace Game4
{
    public class PlayerMover : MonoBehaviour
    {
        private bool _moving = true; //TODO: œŒ“ŒÃ ”ƒ¿À»“‹

        private Vector3 _targetPosition = Vector3.zero;
        private float _speed = 1.6f;

        private const float X_Pos = 1.9f, Y_Pos = 4f;
        private const float StoppingRatio = 2f;

        private void Start()
        {
            PlayerTypes type = GetComponent<Player>().PlayerType;
            if (type == PlayerTypes.BluePlayer)
                _targetPosition = new Vector3(-X_Pos, -Y_Pos);
            else if (type == PlayerTypes.RedPlayer)
                _targetPosition = new Vector3(X_Pos, Y_Pos);

        }

        private void Update()
        {
            if (_moving)
                MoveToPosition();
        }

        private void MoveToPosition()
        {
            if (transform.position != _targetPosition)
                transform.position = Vector3.MoveTowards(transform.position, _targetPosition, _speed * Time.deltaTime);
            else
                ChangeTarget();
        }

        public void ChangeTarget()
        {
            _targetPosition.x *= -1;
        }

        public void StopBeforeFiring(Action action)
        {
            StartCoroutine(StopingAndFiring(action));
        }

        private IEnumerator StopingAndFiring(Action action)
        {
            float normalSpeed = _speed;

            while (_speed > 0f)
            {
                _speed -= Time.deltaTime * StoppingRatio;
                yield return null;
            }
            _speed = 0;

            action();

            while (_speed < normalSpeed)
            {
                _speed += Time.deltaTime * StoppingRatio;
                yield return null;
            }

            _speed = normalSpeed;
        }
    }
}
