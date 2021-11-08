using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NameField : MonoBehaviour
{   
    [SerializeField] private a_ShowMovingPanel _anim;
    [SerializeField] private Button _close;
    [SerializeField] private TMP_InputField _nickName;

    private void Awake()
    {
        _close.onClick.AddListener(() => ClosePanel());
    }

    public bool CheckNickNameAvailability()
    {
        if (string.IsNullOrEmpty(DataHolder.NickName))
        {
            ShowPanel();
            return false;
        }
        else return true;
    }

    private void ShowPanel()
    {
        _anim.gameObject.SetActive(true);
        _anim.ShowPanel(null);
    }

    private void ClosePanel()
    {
        SaveName();
        _nickName.text = null;
        _anim.ClosePanel();       
    }

    private void SaveName()
    {
        if (string.IsNullOrWhiteSpace(_nickName.text) == false)
        {
            DataHolder.NickName = _nickName.text;
            PlayerPrefs.SetString("NickName", DataHolder.NickName);
            PlayerPrefs.Save();
        }       
    }
}