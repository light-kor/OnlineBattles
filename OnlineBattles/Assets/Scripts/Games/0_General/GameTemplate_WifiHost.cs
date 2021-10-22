using GameEnumerations;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class GameTemplate_WifiHost : MonoBehaviour
{
    public static event DataHolder.Pause BackgroundPause;

    private ConnectTypes _connectType;

    protected void BaseStart(ConnectTypes type)
    {
        GeneralController.OpponentLeftTheGame += OpponentLeftTheGame;
        PauseMenu.LeaveTheGame += LeaveTheGame;

        _connectType = type;

        if (_connectType == ConnectTypes.UDP)
        {
            Network.UDPMessagesBig.Clear();
            Network.CreateUDP();
            Network.ClientUDP.SendMessage("sss"); // ������ UDP ���������, ���� ������ ������� �������� �����   
            InvokeRepeating("SendFramesUDP", 0f, WifiServer_Host.UpdateRate);
        }
    }

    /// <summary>
    /// ���������� ����. ����� ����������� � �������� ���� ����������.
    /// </summary>
    /// <param name="opponentStatus">������ ����������.</param>
    protected static void EndOfGame(string opponentStatus)
    {
        WifiServer_Host.Opponent.SendTcpMessage(opponentStatus);
        string notifText = null;
        if (opponentStatus == "drawn")
            notifText = "�����";
        else if (opponentStatus == "lose")
            notifText = "�� ��������";
        else if (opponentStatus == "win")
            notifText = "�� ���������";

        new Notification(notifText, Notification.ButtonTypes.MenuButton);
    } //TODO: ������� ��� � ��������� ��� ���� �� ����� �������

    private void LeaveTheGame()
    {
        CloseAll();
        WifiServer_Host.Opponent.SendTcpMessage("LeftTheGame");
        SceneManager.LoadScene("mainMenu");
    }

    private void OpponentLeftTheGame()
    {
        CloseAll();
        new Notification("��������� ������", Notification.ButtonTypes.MenuButton);
    }

    public static void SendScore(PlayerTypes player, GameResults result, bool setPause)
    {
        if (WifiServer_Host.Opponent != null)
            WifiServer_Host.Opponent.SendTcpMessage($"UpdateScore {player} {result} {setPause}");
    }

    protected void CloseAll()
    {
        BackgroundPause?.Invoke(PauseTypes.BackgroundPause);

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
        PauseMenu.LeaveTheGame -= LeaveTheGame;
    }
}
