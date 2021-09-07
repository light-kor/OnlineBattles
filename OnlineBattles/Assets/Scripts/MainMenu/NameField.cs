using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NameField : MonoBehaviour
{
    [SerializeField] private Button _close;
    [SerializeField] private TMP_InputField _nickName;
    private a_ShowMovingPanel _anim;

    private void Awake()
    {
        _anim = GetComponent<a_ShowMovingPanel>();
        _close.onClick.AddListener(() => ClosePanel());
    }

    private void OnEnable()
    {
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
        if (_nickName.text != "")
        {
            DataHolder.NickName = _nickName.text;
            PlayerPrefs.SetString("NickName", DataHolder.NickName);
            PlayerPrefs.Save();
        }       
    }
}
