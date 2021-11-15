using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReadyButton : MonoBehaviour
{
    [SerializeField] private Color _green;
    [SerializeField] private Color _red;
    [SerializeField] private TMP_Text _notReady;

    private Image _image;
    private Button _button;
    private bool _ready = false;

    private void Start()
    {
        _image = GetComponent<Image>();
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnClick);
        SetNotReady();
    }

    private void OnClick()
    {
        if (Network.ClientTCP != null)
        {           
            if (_ready == false)
                SetReady();
            else
                SetNotReady();

            Network.ClientTCP.SendMessage($"ReadyPlay {_ready}");
        }       
    }

    private void SetReady()
    {
        _ready = true;
        _image.color = _red;
        _notReady.text = "Отмена";
    }

    private void SetNotReady()
    {
        _ready = false;
        _image.color = _green;
        _notReady.text = "Готов";
    }

    private void OnDestroy()
    {
        _button.onClick.RemoveListener(OnClick);
    }
}
