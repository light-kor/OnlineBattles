using GameEnumerations;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public static event UnityAction LeaveTheGame;
    public event UnityAction ResumeGame;

    [SerializeField] private Button _left, _right;
    private a_ShowMovingPanel _anim;

    private void Awake()
    {
        _anim = GetComponent<a_ShowMovingPanel>();
        _left.onClick.AddListener(() => ResumeTheGame());
        _right.onClick.AddListener(() => QuitTheGame());
    }

    private void OnEnable()
    {
        _anim.ShowPanel(null);
    }

    private void QuitTheGame()
    {
        if (DataHolder.GameType == GameTypes.Local)
        {
            SceneManager.LoadScene("mainMenu");
            _anim.ClosePanel();
        }
        else
            LeaveTheGame?.Invoke();              
    }

    private void ResumeTheGame()
    {
        ResumeGame?.Invoke();
        _anim.ClosePanel();
    }
}