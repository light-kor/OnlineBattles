using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MultiBackButton : MonoBehaviour
{
    [SerializeField] private MainMenu _mainMenu;
    [SerializeField] private a_TextReplacement _textPane;
    [SerializeField] private TMP_Text _text;
    private string _newText = "";

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(() => _mainMenu.MultiBack());
    }

    public void ShowMultiBackButton(ButtonTypes type)
    {       
        _text.text = SelectText(type);
        gameObject.SetActive(true);
    }

    public void UpdateButtonText(ButtonTypes type)
    {
        gameObject.SetActive(true);
        _newText = SelectText(type);
        StartCoroutine(_textPane.ReplaceText(ChangeText));
    }

    private string SelectText(ButtonTypes type)
    {
        string text = "";
        if (type == ButtonTypes.Disconnect)
            text = "[ Отключиться] ";
        else if (type == ButtonTypes.Cancel)
            text = "[ Отмена ]";
        else if (type == ButtonTypes.Back)
            text = "[ Назад ]";

        return text;
    }
          
    private void ChangeText()
    {
        _text.text = _newText;
        _newText = "";
    }

    public void DeactivateButton()
    {
        gameObject.SetActive(false);
    }

    public enum ButtonTypes
    {
        Disconnect,
        Cancel,
        Back
    }
}
