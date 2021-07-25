using System.Collections;
using UnityEngine;

public class a_ChangePanel : MonoBehaviour
{
    public static event DataHolder.Notification ChangePanel;

    [SerializeField] private CanvasGroup _canvasGroup;
    private Kino.AnalogGlitch _glitch;
    private float _animTime = 0;

    private void Start()
    {
        MainMenu.ChangePanel += Change;
        _glitch = Camera.main.gameObject.GetComponent<Kino.AnalogGlitch>();
        _animTime = MainMenu.AnimTime;
    }

    private void Change()
    {
        StartCoroutine(Transition());
    }

    private IEnumerator Transition()
    {
        float time = 0f, deltaTime = 0f;

        while (time < _animTime)
        {
            deltaTime = Time.deltaTime;
            time += deltaTime;
            _glitch.verticalJump += deltaTime / _animTime;
            _canvasGroup.alpha -= deltaTime / _animTime;
            yield return null;
        }

        ChangePanel?.Invoke();

        time = 0f;
        while (time < _animTime)
        {
            deltaTime = Time.deltaTime;
            time += deltaTime;
            _glitch.verticalJump -= deltaTime / _animTime;
            _canvasGroup.alpha += deltaTime / _animTime;
            yield return null;
        }

        _glitch.verticalJump = 0.01f;
        _canvasGroup.alpha = 1f;
    }
}
