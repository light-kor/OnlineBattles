using System;
using System.Collections;
using UnityEngine;

public class a_ChangePanel : MonoBehaviour
{
    private const float AnimTime = MainMenu.AnimTime / 2;
    [SerializeField] private CanvasGroup _canvasGroup;
    
    public void StartTransition(Action action)
    {
        StartCoroutine(Transition(action));
    }

    private IEnumerator Transition(Action action)
    {
        _canvasGroup.interactable = false;
        float time = 0f, deltaTime = 0f;

        while (time < AnimTime)
        {
            deltaTime = Time.deltaTime;
            time += deltaTime;
            _canvasGroup.alpha -= deltaTime / AnimTime;
            yield return null;
        }

        action();

        time = 0f;
        while (time < AnimTime)
        {
            deltaTime = Time.deltaTime;
            time += deltaTime;
            _canvasGroup.alpha += deltaTime / AnimTime;
            yield return null;
        }

        _canvasGroup.alpha = 1f;
        _canvasGroup.interactable = true;
    }
}
