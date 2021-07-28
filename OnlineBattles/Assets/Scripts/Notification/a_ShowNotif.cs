using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class a_ShowNotif : MonoBehaviour
{
    private const float BlurSize = 3f;
    private const float AnimTime = 0.5f;

    [SerializeField] private Transform _notificationBox;
    [SerializeField] private Image _background;
    private Notification _notification;
    private float _blurProgress = 0f;

    private void Start()
    {      
        _notification = GetComponent<Notification>();
        _notification.CloseNotification += CloseNotification;
        _background.material.SetFloat("_Size", 0.0f);
        StartCoroutine(BlurProgress(1));

        _notificationBox.localPosition = new Vector2(0, -Screen.height);
        StartCoroutine(_notificationBox.gameObject.MoveLocalY(0, AnimTime, _notification.ShowNotifButton));
    }

    private void CloseNotification()
    {
        StartCoroutine(BlurProgress(-1));
        StartCoroutine(_notificationBox.gameObject.MoveLocalY(-Screen.height, AnimTime, Complete));
    }

    private void Complete()
    {
        _notification.CloseNotification -= CloseNotification;
        Destroy(gameObject);
    }

    private IEnumerator BlurProgress(int dir)
    {
        float time = 0f;
        while (time < AnimTime)
        {
            time += Time.deltaTime;
            _blurProgress += Time.deltaTime * dir * (BlurSize / AnimTime);
            _background.material.SetFloat("_Size", _blurProgress);           
            yield return null;
        }
    }    
}
