using GameEnumerations;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PauseButton : MonoBehaviour
{   
    public event UnityAction<PauseTypes> PauseGame;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() => PauseGame?.Invoke(PauseTypes.ManualPause));
    }
}
