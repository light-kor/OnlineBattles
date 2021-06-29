using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class a_ShowNotif : MonoBehaviour
{
    private const float BlurSize = 3f;
    private const float AnimTime = 0.5f;

    [SerializeField] private Transform _messageBox;
    [SerializeField] private Image _background;
    private float _blurProgress = 0f;

    private void OnEnable()
    {
        _background.material.SetFloat("_Size", 0.0f);
        GetComponent<Notification>().CloseNotification += CloseNotification;
        StartCoroutine(BlurProgress(1, AnimTime));

        _messageBox.localPosition = new Vector2(0, -Screen.height);
        _messageBox.LeanMoveLocalY(0, AnimTime).setEaseOutExpo().delay = 0.1f;
    }

    private void CloseNotification()
    {
        StartCoroutine(BlurProgress(-1, AnimTime));      
        _messageBox.LeanMoveLocalY(-Screen.height, AnimTime).setEaseInExpo().setOnComplete(Complete);
    }

    private void Complete()
    {
        GetComponent<Notification>().CloseNotification -= CloseNotification;
        Destroy(gameObject);
    }

    private IEnumerator BlurProgress(int dir, float delay)
    {
        float time = 0f;
        while (time < delay)
        {
            time += Time.deltaTime;
            _blurProgress += Time.deltaTime * dir * (BlurSize / delay);
            _background.material.SetFloat("_Size", _blurProgress);           
            yield return null;
        }
    }
}
