using GameEnumerations;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EndRoundFlash : MonoBehaviour
{
    [SerializeField] private Color _blue, _red, _both, _nobody;
    [SerializeField] private Image _image;

    private Coroutine _coroutine;
    private PlayerTypes _lastWinner;

    private void Start()
    {
        Score.RoundWinner += SetLastWinner;
        _image.transform.localScale = Vector3.zero;
    }

    public void FlashAnimation()
    {
        if (_coroutine != null)
            StopCoroutine(_coroutine);

        if (_lastWinner == PlayerTypes.BluePlayer)
            _coroutine = StartCoroutine(ChangeScale(_blue));
        else if (_lastWinner == PlayerTypes.RedPlayer)
            _coroutine = StartCoroutine(ChangeScale(_red));
        else if (_lastWinner == PlayerTypes.Both)
            _coroutine = StartCoroutine(ChangeScale(_both));
        else if (_lastWinner == PlayerTypes.Null)
            _coroutine = StartCoroutine(ChangeScale(_nobody));
    }

    private IEnumerator ChangeScale(Color color)
    {
        _image.color = color;
        float x = 0f, Cubic_X = 0f;
        Vector3 scale;

        while (Cubic_X < 30f)
        {
            x += Time.deltaTime * 5f;
            Cubic_X = EaseInCubic(x);
            scale = new Vector3(Cubic_X, Cubic_X, Cubic_X);
            _image.gameObject.transform.localScale = scale;
            yield return null;
        }

        Color colorBuffer = color;
        float a = colorBuffer.a;

        while (colorBuffer.a != 0f)
        {
            a -= Time.deltaTime * 3f;
            colorBuffer.a = EaseInCubic(a);
            _image.color = colorBuffer;
            yield return null;
        }

        _image.transform.localScale = Vector3.zero;
    }

    private void SetLastWinner(PlayerTypes player)
    {
        _lastWinner = player;
    }

    private float EaseInCubic(float x)
    {
        return x * x * x;
    }
}
