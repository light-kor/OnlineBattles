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
            _playerMover.ButtonClick();
        }
    }
}
