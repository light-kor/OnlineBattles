using UnityEngine;
using UnityEngine.UI;

namespace Game4
{
    public class GameButton : MonoBehaviour
    {
        [SerializeField] Player _playerMover;

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(() => ChangeDirection());
        }

        private void ChangeDirection()
        {
            if (GeneralController.GameOn)
                _playerMover.ButtonClick();
        }
    }
}
