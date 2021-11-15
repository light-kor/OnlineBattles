using GameEnumerations;
using UnityEngine;
using UnityEngine.UI;

namespace Game4
{
    public class GameResources_4 : GeneralController
    {
        [SerializeField] private Player _blue, _red;
        [SerializeField] private Button _blueButton, _redButton;
        [SerializeField] private BulletContainer _bulletContainer;
        public static GameResources_4 GameResources;

        public Player Blue => _blue;
        public Player Red => _red;
        public BulletContainer Bullets => _bulletContainer;

        private const float TimeForReload = 2f;
        private const int WinScore = 5;

        private void Awake()
        {
            GameResources = this;
            SetGameButtons();
        }

        //TODO: ���-�� ���������� �����������, � ������ ����� ������� ������������. ���� ��� �� ���� ��������������.
        //TODO: �������� ������������ ��������
        //TODO: �������� ������
        //TODO: ���������� ������� ������. �������� ������, � �� ����� �� ����, ��� ��� � 3 ��� ���� ����.
        //TODO: ���� ����� ������ ��������� ����������� Block �� ���� �����������. ���� ���-�� �������������� ��� 
        //TODO: � ������� Block ���� �������� ������� ��������. ��������� ��������� �������� 
        //TODO: ���������� ������, �������� � ������� ��� ����
        //TODO: ������� � �������� ��� �������. ����� ������� ��� ������ � ���������
        //TODO: ��� �� �������� ����������� � ������� rigidbody?

        public void GetHit(PlayerTypes type)
        {
            UpdateScoreAndCheckGameState(type, GameResults.Lose, WinScore, false);
        }

        public void TryReloadGuns()
        {
            if (_blue.HasBullet == false && _red.HasBullet == false)
                Invoke("ReloadGuns", TimeForReload);
        }

        public void ReloadGuns()
        {
            _blue.ReloadGun();
            _red.ReloadGun();
        }

        public void SetGameButtons()
        {
            if (DataHolder.GameType == GameTypes.WifiHost)
            {
                _redButton.gameObject.SetActive(false);
            }
            else if (DataHolder.GameType == GameTypes.WifiHost)
            {
                _redButton.transform.position = _blueButton.transform.position;
                _blueButton.gameObject.SetActive(false);
            }
        }
    }
}