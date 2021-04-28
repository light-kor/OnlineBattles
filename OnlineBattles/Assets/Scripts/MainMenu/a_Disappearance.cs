using UnityEngine;
using UnityEngine.UI;

public class a_Disappearance : MonoBehaviour
{
    [SerializeField] private float _speed = 10f;
    private Image _image;
    private Color _color;

    void Start()
    {
        _image = GetComponent<Image>();
        _color = _image.color;
    }

    void Update()
    {
        _color.a -= Time.deltaTime * _speed / 10;
        _image.color = _color;
    }
}
