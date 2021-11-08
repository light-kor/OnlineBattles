using GameEnumerations;
using UnityEngine;

namespace Game4
{
    public class GameResources_4 : GeneralController
    {
        [SerializeField] private Player _blue, _red;
        [SerializeField] private BulletContainer _bulletContainer;

        public static GameResources_4 GameResources;
        public BulletContainer Bullets => _bulletContainer;

        private const float TimeForReload = 2f;
        private readonly int WinScore = 5;

        protected override void Awake()
        {
            base.Awake();
            GameResources = this;
        }

        //TODO: �������� ������������ ��������
        //TODO: �������� ������
        //TODO: ���������� ������� ������. �������� ������, � �� ����� �� ����, ��� ��� � 3 ��� ���� ����.
        //TODO: ���� ����� ������ ��������� ����������� Block �� ���� �����������. ���� ���-�� �������������� ��� 
        //TODO: � ������� Block ���� �������� ������� ��������. ��������� ��������� �������� 
        //TODO: ���������� ������, �������� � ������� ��� ����
        //TODO: ������� � �������� ��� �������. ����� ������� ��� ������ � ���������

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
    }
}
