using GameEnumerations;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class GameTemplate_WifiHost : MonoBehaviour
{
    private ConnectTypes _connectType;

    protected void BaseStart(ConnectTypes type)
    {
        GeneralController.OpponentLeftTheGame += OpponentLeftTheGame;
        GeneralController.RemotePause += SendPauseRequest;
        GeneralController.RemoteResume += SendResumeRequest;
        PauseMenu.LeaveTheGame += LeaveTheGame;

        _connectType = type;

        if (_connectType == ConnectTypes.UDP)
        {
            Network.UDPMessagesBig.Clear();
            Network.CreateUDP();
            Network.ClientUDP.SendMessage("sss"); // Именно UDP сообщение, чтоб сервер получил удалённый адрес   
            InvokeRepeating("SendFramesUDP", 0f, WifiServer_Host.UpdateRate);
        }
    }

    /// <summary>
    /// Завершение игры. Вывод уведомления и отправка инфы противнику.
    /// </summary>
    /// <param name="opponentStatus">Статус противника.</param>
    protected static void EndOfGame(string opponentStatus)
    {
        WifiServer_Host.Opponent.SendTcpMessage(opponentStatus);
        string notifText = null;
        if (opponentStatus == "drawn")
            notifText = "Ничья";
        else if (opponentStatus == "lose")
            notifText = "Вы победили";
        else if (opponentStatus == "win")
            notifText = "Вы проиграли";

        new Notification(notifText, Notification.ButtonTypes.MenuButton);
    } //TODO: Удалить это и перевести все игры на новую систему

    private void LeaveTheGame()
    {
        CloseAll();
        WifiServer_Host.Opponent.SendTcpMessage("LeftTheGame");
        SceneManager.LoadScene("mainMenu");
    }

    private void SendPauseRequest()
    {
        WifiServer_Host.Opponent.SendTcpMessage("Pause");
    }

    private void SendResumeRequest()
    {
        WifiServer_Host.Opponent.SendTcpMessage("Resume");
    }

    private void OpponentLeftTheGame()
    {
        CloseAll();
        new Notification("Противник сдался", Notification.ButtonTypes.MenuButton);
    }

    public static void SendScore(PlayerTypes player, GameResults result)
    {
        if (WifiServer_Host.Opponent != null)
            WifiServer_Host.Opponent.SendTcpMessage($"UpdateScore {player} {result}");
    }

    protected void CloseAll()
    {
        if (_connectType == ConnectTypes.UDP)
        {
            CancelInvoke();
            Network.CloseUdpConnection();
        }
    }

    protected virtual void SendFramesUDP()
    {

    }

    private void OnDestroy()
    {
        GeneralController.OpponentLeftTheGame -= OpponentLeftTheGame;
        GeneralController.RemotePause -= SendPauseRequest;
        GeneralController.RemoteResume -= SendResumeRequest;
        PauseMenu.LeaveTheGame -= LeaveTheGame;
    }
}
