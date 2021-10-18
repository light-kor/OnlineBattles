using GameEnumerations;
using UnityEngine;
using UnityEngine.UI;

public class PauseButton : MonoBehaviour
{
    public delegate void Pause(PauseType pauseType);
    public event Pause PauseGame;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() => PauseGame?.Invoke(PauseType.ManualPause));
    }
}
