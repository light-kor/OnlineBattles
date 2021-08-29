using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class a_ShowCancelButton : MonoBehaviour
{
    private const float AnimTime = MainMenu.AnimTime;
    private Image _image;
    private Button _button;
    private Color _color;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _image = GetComponent<Image>();
        _color = _image.color;
        _color.a = 0f;
        _image.color = _color;
        _button.interactable = false;
    }

    public void ShowButton()
    {
        StartCoroutine(ChangeTransparency(1, SetInteractableTrue));        
    }

    public void HideButton()
    {
        _button.interactable = false;
        StartCoroutine(ChangeTransparency(-1, EraseButton));
    }

    private IEnumerator ChangeTransparency(int dir, Action SetOnComplete)
    {
        float time = 0f;
        while (time < AnimTime)
        {
            time += Time.deltaTime;
            _color.a += Time.deltaTime * dir / AnimTime;
            _image.color = _color;
            yield return null;
        }

        SetOnComplete();
    }

    private void SetInteractableTrue()
    {
        _button.interactable = true;
        _color.a = 1f;
        _image.color = _color;
    }

    private void EraseButton()
    {       
        _color.a = 0f;
        _image.color = _color;
    }
}
