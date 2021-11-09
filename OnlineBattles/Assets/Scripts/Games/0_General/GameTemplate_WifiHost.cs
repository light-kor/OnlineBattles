using GameEnumerations;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class GameTemplate_WifiHost : MonoBehaviour
{
    private ConnectTypes _connectType;
    private Coroutine _udpSender = null;

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

            _udpSender = StartCoroutine(SendGameFrameEverySecondFrame());
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
            if (_udpSender != null)
            {
                StopCoroutine(_udpSender);
                _udpSender = null;
            }

            Network.CloseUdpConnection();
        }
    }

    /// <summary>
    /// �������� ������� ���� 30 ��� � �������, ������������ �� ������� ������.
    /// </summary>
    /// <returns></returns>
    private IEnumerator SendGameFrameEverySecondFrame()
    {
        int targetCounter = Application.targetFrameRate / 30;
        int counter = 0;

        while (true)
        {
            counter++;

            if (counter == targetCounter)
            {
                counter = 0;
                SendFramesUDP();              
            }

            yield return null;
        }
        //TODO: ���� � ����� �������� ���, �� � ������� �������� ��������� ������ ��������.     
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
