using UnityEngine;
using UnityEngine.UI;

namespace Game4
{
    public class GameButton : MonoBehaviour
    {
        [SerializeField] Player _playerMover;
        private GameResources_4 GR;

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(() => ChangeDirection());
        }

        private void Start()
        {
            GR = GameResources_4.GameResources;
        }

        private void ChangeDirection()
        {
            if (GR.GameOn)
                _playerMover.ButtonClick();
        }
    }
}
