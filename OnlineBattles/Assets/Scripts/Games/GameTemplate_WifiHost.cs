using UnityEngine;

public class GameTemplate_WifiHost : MonoBehaviour
{
    protected bool _gameOn = true;
    protected string[] messege;
    private string _earlyOpponentStatus = null;
    private string _gameType = null;

    protected void BaseStart(string type)
    {
        WifiServer_Host.OpponentGaveUp += OpponentGiveUp;
        LeaveGameButton.WantLeaveTheGame += IGiveUp;
        MainMenu.SetStartMenuType(MainMenu.MenuTypes.WifiHost);
        _gameType = type;

        if (_gameType == "udp")
        {
            StartUdpConnection();
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

    private void StartUdpConnection()
    {
        DataHolder.MessageUDPget.Clear();
        Network.CreateUDP();
        DataHolder.ClientUDP.SendMessage("sss"); // ������ UDP ���������, ���� ������ ������� �������� �����       
    }

    protected bool SplitFramesAndChechTrash()
    {
        messege = DataHolder.MessageUDPget[0].Split(' ');
        if (messege[0] != "g")
        {
            DataHolder.MessageUDPget.RemoveAt(0);
            return false;
        }
        return true;
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

        new Notification(notifText, Notification.ButtonTypes.ExitSingleGame);
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

        if (_gameType == "udp")
        {
            CancelInvoke();
            Network.CloseUdpConnection();
        }
    }

    /// <summary>
    /// ���������� �������� ���������. ���������������� � ����������� �������.
    /// </summary>
    public virtual void SendAllChanges()
    {

    }

    private void OnDestroy()
    {
        WifiServer_Host.OpponentGaveUp -= OpponentGiveUp;
        LeaveGameButton.WantLeaveTheGame -= IGiveUp;
    }
}
