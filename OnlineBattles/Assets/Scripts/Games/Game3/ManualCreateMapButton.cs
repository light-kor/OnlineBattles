using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ManualCreateMapButton : MonoBehaviour, IPointerClickHandler
{
    public static event DataHolder.Notification Click;
    private float _timer = 9f;
    private Text _time;
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

    void Start()
    {
        _time = GetComponentInChildren<Text>();
        _time.gameObject.SetActive(false);
    }

    void Update()
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
