using System.Collections;
using UnityEngine;

public class a_ChangePanel : MonoBehaviour
{
    public static event DataHolder.Notification ChangePanel;

    [SerializeField] private CanvasGroup _canvasGroup;
    private float _animTime = 0;

    private void Awake()
    {
        MainMenu.ChangePanel += StartTransition;
        _animTime = MainMenu.AnimTime / 2;
    }

    private void StartTransition()
    {
        StartCoroutine(Transition());
    }

    private IEnumerator Transition()
    {
        _canvasGroup.interactable = false;
        float time = 0f, deltaTime = 0f;

        while (time < _animTime)
        {
            deltaTime = Time.deltaTime;
            time += deltaTime;
            _canvasGroup.alpha -= deltaTime / _animTime;
            yield return null;
        }

        ChangePanel?.Invoke();

        time = 0f;
        while (time < _animTime)
        {
            deltaTime = Time.deltaTime;
            time += deltaTime;
            _canvasGroup.alpha += deltaTime / _animTime;
            yield return null;
        }

        _canvasGroup.alpha = 1f;
        _canvasGroup.interactable = true;
    }

    private void OnDestroy()
    {
        MainMenu.ChangePanel -= StartTransition;
    }
}
