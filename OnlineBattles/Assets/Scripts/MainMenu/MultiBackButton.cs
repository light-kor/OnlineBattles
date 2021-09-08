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
        if (gameObject.activeSelf == false) //TODO: �� ������ ��������� �����������, ����� ���� ��������� �� wifi. �� ����� ������, ��� �� ������ �������.
        {
            _text.text = SelectText(type);
            gameObject.SetActive(true);
        }
        else
        {
            _newText = SelectText(type);
            _textPane.ReplaceText(ChangeText);
        }
    }

    private string SelectText(ButtonTypes type)
    {
        string text = "";
        if (type == ButtonTypes.Disconnect)
            text = "[ �����������] ";
        else if (type == ButtonTypes.Cancel)
            text = "[ ������ ]";
        else if (type == ButtonTypes.Back)
            text = "[ ����� ]";

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
