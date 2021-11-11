using GameEnumerations;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class GameTemplate_WifiHost : MonoBehaviour
{
    private ConnectTypes _connectType;
    private float counter = 0, targetCounter = 2;

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
            Network.ClientUDP.SendMessage("sss"); // ������ UDP ���������, ���� ������ ������� �������� �����
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
        new Notification("��������� ������", Notification.ButtonTypes.MenuButton);
    }

    public static void SendScore(PlayerTypes player, GameResults result)
    {
        WifiServer_Host.Opponent.SendTcpMessage($"UpdateScore {player} {result}");
    }

    protected void CloseAll()
    {
        if (_connectType == ConnectTypes.UDP)
        {
            Network.CloseUdpConnection();
        }
    }

    protected void TrySendFrameUDP()
    {
        counter++;
        if (counter == targetCounter)
        {
            counter = 0;
            CreateUDPFrame();
            // ��� ��� ���������� � FixedUpdate, ���������� �������� UDP ��������� 25 ��� � �������.
        }
    }

    protected virtual void CreateUDPFrame()
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
