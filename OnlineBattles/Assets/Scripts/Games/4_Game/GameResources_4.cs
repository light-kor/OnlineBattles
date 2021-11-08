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

        //TODO: Анимации исчезновения объектов
        //TODO: Добавить онлайн
        //TODO: Переделать систему спавна. Спавнить бонусы, а не блоки не чаще, чем раз в 3 или типо того.
        //TODO: Пуля может задеть несколько коллайдеров Block за одно прохождение. Надо как-то деактивировать его 
        //TODO: В скрипте Block надо добавить разброс градусов. Небольшое рандомное смещение 
        //TODO: Нормальный спрайт, анимации и частицы для пули
        //TODO: Спрайты и анимайии для игроков. Можно частицы при ходьбе и ввыстреле

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
