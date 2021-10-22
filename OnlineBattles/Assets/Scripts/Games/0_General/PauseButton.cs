using GameEnumerations;
using UnityEngine;
using UnityEngine.UI;

public class PauseButton : MonoBehaviour
{   
    public event DataHolder.Pause PauseGame;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() => PauseGame?.Invoke(PauseTypes.ManualPause));
    }
}
