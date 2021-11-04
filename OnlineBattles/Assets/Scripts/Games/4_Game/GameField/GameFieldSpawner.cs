using UnityEngine;

namespace Game4
{
    public class GameFieldSpawner : MonoBehaviour
    {
        [SerializeField] private Multiplier _multiplierPrefab;
        [SerializeField] private SpeedChanger _speedChangerPrefab;
        [SerializeField] private Reflector _reflectorPrefab;

        private const float MaxFieldHeight = 2f;
        private const float MaxFieldWidth = 2f;
        private const int MaxObjectsCount = 7;

        private int _speedChangerCount = 0;
        private int _multiplierCount = 0;
        private int _objectsCount = 0;

        private void Start()
        {
            while (_objectsCount < MaxObjectsCount)
            {
                CreateNewObject();
            }
        }

        //TODO: Сделать удаление и возобновление ловушек
        //TODO: Анимации исчезновения объектов
        //TODO: Привязать счёт
        //TODO: Добавить онлайн
        //TODO: После первого удара об рефлектор, можно сильно увеличить скорость пули


        private void CreateNewObject()
        {            
            float xPos = Random.Range(-MaxFieldWidth, MaxFieldWidth);
            float yPos = Random.Range(-MaxFieldHeight, MaxFieldHeight);
            Vector2 pos = new Vector2(xPos, yPos);

            if (_speedChangerCount < 1)
            {
                Instantiate(_speedChangerPrefab, pos, Quaternion.identity);
                _speedChangerCount++;
            }
            else if (_multiplierCount < 2)
            {
                Instantiate(_multiplierPrefab, pos, Quaternion.identity);
                _multiplierCount++;
            }
            else
                Instantiate(_reflectorPrefab, pos, Quaternion.identity);

            _objectsCount++;
        }

    }
}
