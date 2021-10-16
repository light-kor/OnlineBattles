using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game3
{
    public class ManualCreateMapButton : MonoBehaviour, IPointerClickHandler
    {
        public static event DataHolder.Notification Click;
        private float _timer;
        private TMP_Text _time;
        private bool _waiting = false;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_waiting)
            {
                Click?.Invoke();
                _waiting = true;
                GetComponent<Button>().interactable = false;
                _timer = 9f;
                _time.gameObject.SetActive(true);
            }
        }

        private void Start()
        {
            _time = GetComponentInChildren<TMP_Text>();
            _time.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (_waiting)
            {
                if (_timer < 0)
                {
                    GetComponent<Button>().interactable = true;
                    _waiting = false;
                    _time.gameObject.SetActive(false);
                }

                _timer -= Time.deltaTime;
                _time.text = Math.Round(_timer).ToString();
            }
        }
    }
}
