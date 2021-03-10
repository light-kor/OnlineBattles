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
                //TODO: ���� ����� �� ������� ������ ���������, �� ���� ���� �� ������ ��������, ��� ���� ��������� �� ����������
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

    //TODO: ������� ������� ���������� ������� �����������. ��������� ������ �������, ���� ���� ��� �� �������� ���� ���������. � ��������/�������� ������ �� � ������, ���� �� ���� ��������� ������ ���������
}
