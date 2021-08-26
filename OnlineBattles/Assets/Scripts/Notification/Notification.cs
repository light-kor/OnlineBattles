public class Notification
{
    public readonly string NotifText = null;
    public readonly NotifTypes NotifType = 0;
    public ButtonTypes ButtonType = 0;
    
    public Notification(string notif, NotifTypes notifType, ButtonTypes buttonType)
    {
        NotifText = notif;
        NotifType = notifType;
        ButtonType = buttonType;
        NotificationManager.NM.AddNotificationToQueue(this);
    }

    public Notification(string notif, ButtonTypes buttonType)
    {
        NotifText = notif;
        ButtonType = buttonType;
        NotificationManager.NM.AddNotificationToQueue(this);
    }

    public enum ButtonTypes
    {
        Null,
        Waiting, // num  0
        SimpleClose, // num 1
        StopReconnect, // num 2
        CancelGameSearch, // num 3
        ExitSingleGame, // num 4

        CancelWifiOpponent,
        AcceptWifiOpponent,
        StopTryingReconnect
    }

    public enum NotifTypes
    {
        Null,
        Connection,
        Reconnect,
        WifiRequest
    }

}
