using UnityEngine;

public abstract class GameTemplate_WifiHost : MonoBehaviour
{
    protected bool _gameOn = true;
    protected string[] _messages;
    private string _earlyOpponentStatus = null;
    private DataHolder.ConnectType _gameType;

    protected void BaseStart(DataHolder.ConnectType type)
    {
        WifiServer_Host.OpponentGaveUp += OpponentGiveUp;
        PauseMenu.WantLeaveTheGame += IGiveUp;
        _gameType = type;

        if (_gameType == DataHolder.ConnectType.UDP)
        {
            Network.UDPMessagesBig.Clear();
            Network.CreateUDP();
            Network.ClientUDP.SendMessage("sss"); // ������ UDP ���������, ���� ������ ������� �������� �����   
            InvokeRepeating("SendAllChanges", 0f, WifiServer_Host.UpdateRate);
        }
    }

    protected virtual void Update()
    {        
        if (_earlyOpponentStatus != null)
        {
            CloseAll();
            EndOfGame(_earlyOpponentStatus);
            _earlyOpponentStatus = null;
        }
    }

    /// <summary>
    /// ���������� ����. ����� ����������� � �������� ���� ����������.
    /// </summary>
    /// <param name="opponentStatus">������ ����������.</param>
    protected static void EndOfGame(string opponentStatus)
    {
        WifiServer_Host.SendTcpMessage(opponentStatus);
        string notifText = null;
        if (opponentStatus == "drawn")
            notifText = "�����";
        else if (opponentStatus == "lose")
            notifText = "�� ��������";
        else if (opponentStatus == "win")
            notifText = "�� ���������";

        new Notification(notifText, Notification.ButtonTypes.MenuButton);
    }

    private void IGiveUp()
    {
        _earlyOpponentStatus = "win";
    }

    private void OpponentGiveUp()
    {
        _earlyOpponentStatus = "lose";
    }

    protected void CloseAll()
    {
        _gameOn = false;

        if (_gameType == DataHolder.ConnectType.UDP)
        {
            CancelInvoke();
            Network.CloseUdpConnection();
        }
    }

    private void OnDestroy()
    {
        WifiServer_Host.OpponentGaveUp -= OpponentGiveUp;
        PauseMenu.WantLeaveTheGame -= IGiveUp;
    }
}
