using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Game3
{
    public class ManualCreateMapButton : MonoBehaviour
    {
        public static event UnityAction Click;

        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _time;

        private float _timer;
        private bool _waiting = false;       

        private void Start()
        {
            _button.onClick.AddListener(ButtonClick);
            _time.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (_waiting)
            {
                if (_timer < 0)
                {
                    _button.interactable = true;
                    _waiting = false;
                    _time.gameObject.SetActive(false);
                }

                _timer -= Time.deltaTime;
                _time.text = Math.Round(_timer).ToString();
            }
        }

        private void ButtonClick()
        {
            if (GeneralController.GameOn)
            {
                if (!_waiting)
                {
                    Click?.Invoke();
                    _waiting = true;
                    _button.interactable = false;
                    _timer = 9f;
                    _time.gameObject.SetActive(true);
                }
            }          
        }
    }
}
