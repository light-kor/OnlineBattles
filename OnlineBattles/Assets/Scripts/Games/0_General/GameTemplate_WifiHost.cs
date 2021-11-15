using GameEnumerations;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class GameTemplate_WifiHost : MonoBehaviour
{
    private ConnectTypes _connectType;
    private float counter = 0, targetCounter = 2;

    public void newStart(ConnectTypes connectType)
    {
        _connectType = connectType;
        WifiServer_Host.OpponentIsReady = false;
        GeneralController.OpponentLeftTheGame += OpponentLeftTheGame;
        GeneralController.RemotePause += SendPauseRequest;
        GeneralController.RemoteResume += SendResumeRequest;
        PauseMenu.LeaveTheGame += LeaveTheGame;

        if (_connectType == ConnectTypes.UDP)
        {
            Network.UDPMessagesBig.Clear();
            Network.CreateUDP();
            Network.ClientUDP.SendMessage("sss"); // Именно UDP сообщение, чтоб сервер получил удалённый адрес
        }
    }

    public void newFixedUpdate()
    {
        if (GeneralController.GameOn && Network.ClientUDP != null)
        {
            TrySendFrameUDP();
        }
    }

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
        WifiServer_Host.Opponent.SendTcpMessage($"UpdateScore {player} {result}");
    }

    private void CloseAll()
    {
        if (_connectType == ConnectTypes.UDP)
        {
            Network.CloseUdpConnection();
        }
    }

    private void TrySendFrameUDP()
    {
        counter++;
        if (counter == targetCounter)
        {
            counter = 0;
            CreateUDPFrame();
            // Так как вызывается в FixedUpdate, получается отправка UDP сообщений 25 раз в секунду.
        }
    }

    protected virtual void CreateUDPFrame()
    {

    }

    public void newOnDestroy()
    {
        GeneralController.OpponentLeftTheGame -= OpponentLeftTheGame;
        GeneralController.RemotePause -= SendPauseRequest;
        GeneralController.RemoteResume -= SendResumeRequest;
        PauseMenu.LeaveTheGame -= LeaveTheGame;
    }
}
