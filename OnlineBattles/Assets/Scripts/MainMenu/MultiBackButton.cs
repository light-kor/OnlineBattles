using GameEnumerations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MultiBackButton : MonoBehaviour
{
    [SerializeField] private MenuView _menuView;
    [SerializeField] private a_TextReplacement _textPane;
    [SerializeField] private TMP_Text _text;
    private string _newText = string.Empty;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(() => MultiBack());
    }

    public void ShowMultiBackButton(BackButtonTypes type)
    {
        _text.text = SelectButtonText(type);
        gameObject.SetActive(true);
    }

    public void UpdateMultiBackButton(BackButtonTypes type)
    {
        _newText = SelectButtonText(type);
        _textPane.ReplaceText(ChangeText);
    }

    private void MultiBack()
    {
        if (DataHolder.GameType == GameTypes.WifiClient)
        {
            Network.CloseWifiServerSearcher();

            if (Network.ClientTCP != null)
            {
                Network.ClientTCP.SendMessage("disconnect");
                Network.CloseTcpConnection();
            }
        }
        else if (DataHolder.GameType == GameTypes.WifiHost)
        {
            if (WifiServer_Host.Opponent != null)
            {
                WifiServer_Host.Opponent.SendTcpMessage("disconnect");
                WifiServer_Host.CloseConnection(); // Это если игрок уже подключён
            }
            else
                WifiServer_Host.CancelConnect(); // Это если игрок ещё не нашёлся

            //TODO: Добавить отмену для мультиплеера
        }
        _menuView.ChangePanelWithAnimation(_menuView.ActivateMainMenu);
    }

    private string SelectButtonText(BackButtonTypes type)
    {
        string text = "";
        if (type == BackButtonTypes.Disconnect)
            text = "[ Отключиться] ";
        else if (type == BackButtonTypes.Cancel)
            text = "[ Отмена ]";
        else if (type == BackButtonTypes.Back)
            text = "[ Назад ]";

        return text;
    }
          
    private void ChangeText()
    {
        _text.text = _newText;
        _newText = string.Empty;
    }

    public void DeactivateButton()
    {
        gameObject.SetActive(false);
    }   
}