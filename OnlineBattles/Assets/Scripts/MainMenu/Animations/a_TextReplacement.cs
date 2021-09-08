using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class a_TextReplacement : MonoBehaviour
{
    private const float AnimTime = MainMenu.AnimTime / 2;
    private Image _image;

    private void Awake()
    {
        _image = GetComponent<Image>();
    }

    public void ReplaceText(Action ChangeText)
    {
        StartCoroutine(ReplaceAnim(ChangeText));
    }

    private IEnumerator ReplaceAnim(Action ChangeText)
    {
        _image.fillOrigin = (int)Image.OriginVertical.Top;
        _image.fillAmount = 1f;
        float time = 0f;
        while (time < AnimTime)
        {
            time += Time.deltaTime;
            _image.fillAmount -= Time.deltaTime / AnimTime;
            yield return null;
        }
        _image.fillAmount = 0f;

        ChangeText();

        _image.fillOrigin = (int)Image.OriginVertical.Bottom;
        _image.fillAmount = 0f;
        time = 0f;
        while (time < AnimTime)
        {
            time += Time.deltaTime;
            _image.fillAmount += Time.deltaTime / AnimTime;
            yield return null;
        }
        _image.fillAmount = 1f;
    }
}
