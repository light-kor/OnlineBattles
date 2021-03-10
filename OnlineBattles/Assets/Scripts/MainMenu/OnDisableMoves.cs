using System.Collections;
using UnityEngine;

public class OnDisableMoves : MonoBehaviour
{
    private bool _waiting;
    private float duration;

    private void OnApplicationPause()
    {
        duration = 5f;
        _waiting = true;
    }

    private void OnApplicationFocus()
    {
        _waiting = false;
    }

    private void Update()
    {
        if (_waiting)
        {
            duration -= Time.deltaTime;

            if (duration < 0)
            {
                _waiting = false;
                CloseAll();
                //TODO: Если вдруг ты реально просто отлучился, то надо себе на экране показать, что тебя отключило за отсутствие
            }
        }
    }

    private void CloseAll()
    {
        if (DataHolder.GameType == 22 && WifiServer_Host._opponent != null)
        {
            WifiServer_Host.SendTcpMessage("disconnect");
            WifiServer_Host.CloseAll();
        }
    }

    //TODO: Сделать большие обобщённые функции дисконнекта. Оповещать вторую сторону, даже если это он присалал тебе сообщение. и включать/включать вообще всё и всегда, чтоб не было случайных лишних элементов
}
