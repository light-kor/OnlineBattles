using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class a_Settings : MonoBehaviour
{
    private const float BlurSize = 4f;
    private const float AnimTime = MainMenu.AnimTime;

    [SerializeField] private Transform _notificationBox;
    [SerializeField] private Image _background;
    [SerializeField] private bool _blurOn = true; // ����� ����� ���� ��������� � ����������..
    private float _blurProgress = 0f;

    private void OnEnable()
    {
        if (_blurOn)
        {
            _background.material.SetFloat("_Size", 0.0f);
            _blurProgress = 0f;
            StartCoroutine(BlurProgress(1));
        }

        _notificationBox.localPosition = new Vector2(0, -Screen.height);
        StartCoroutine(_notificationBox.gameObject.MoveLocalY(0, AnimTime));
    }

    public void CloseNotification()
    {
        if (_blurOn)
            StartCoroutine(BlurProgress(-1));

        StartCoroutine(_notificationBox.gameObject.MoveLocalY(-Screen.height, AnimTime, Complete));
    }

    private void Complete()
    {
        gameObject.SetActive(false);
    } //TODO: � ���� ������� ������, ����� ��� ��� �� ���������

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
