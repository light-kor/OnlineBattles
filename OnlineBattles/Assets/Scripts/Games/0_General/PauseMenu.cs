using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public static event DataHolder.Notification WantLeaveTheGame;
    public event DataHolder.Notification ResumeGame;

    [SerializeField] private Button _left, _right;
    [SerializeField] private TMP_Text _messageText; //TODO: В онлайн режимах может быть другой текст
    private a_ShowMovingPanel _anim;

    private void Awake()
    {
        _anim = GetComponent<a_ShowMovingPanel>();
        _left.onClick.AddListener(() => ResumeTheGame());
        _right.onClick.AddListener(() => QuitTheGame());
        //_left.GetComponentInChildren<TMP_Text>().text = "";
        //_right.GetComponentInChildren<TMP_Text>().text = "";
    }

    private void OnEnable()
    {
        _anim.ShowPanel(null);
    }

    private void QuitTheGame()
    {
        if (DataHolder.GameType == DataHolder.GameTypes.Single || DataHolder.GameType == DataHolder.GameTypes.Null)
        {
            SceneManager.LoadScene("mainMenu");
            _anim.ClosePanel();
        }
        else
        {
            WantLeaveTheGame?.Invoke();
            //TODO: Сделать анимацию ожидания
        }              
    }

    private void ResumeTheGame()
    {
        if (DataHolder.GameType == DataHolder.GameTypes.Single || DataHolder.GameType == DataHolder.GameTypes.Null)
        {
            ResumeGame?.Invoke();
        }
        else
        {

        }

        _anim.ClosePanel();
    }
}
