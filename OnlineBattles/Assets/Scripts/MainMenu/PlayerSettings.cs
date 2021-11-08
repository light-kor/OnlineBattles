using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSettings : MonoBehaviour
{
    [SerializeField] private a_ShowMovingPanel _anim;
    [SerializeField] private Button _close;
    [SerializeField] private Slider _volume;
    [SerializeField] private TMP_InputField _nickName;  

    private void Awake()
    {
        _close.onClick.AddListener(() => ClosePanel());
        _anim.gameObject.SetActive(false);
        LoadSettings();     
    }

    public void ShowPanel()
    {
        DisplayValues();
        _anim.gameObject.SetActive(true);
        _anim.ShowPanel(null);
    }

    private void ClosePanel()
    {
        SaveSettings();
        _anim.ClosePanel();
    }

    private void DisplayValues()
    {
        _volume.value = DataHolder.Volume;
        _nickName.text = DataHolder.NickName;
    }

    private void SaveSettings()
    {
        DataHolder.Volume = _volume.value;
        PlayerPrefs.SetFloat("Volume", DataHolder.Volume);

        if (string.IsNullOrWhiteSpace(_nickName.text) == false)
        {
            DataHolder.NickName = _nickName.text;
            PlayerPrefs.SetString("NickName", DataHolder.NickName);
        }
                     
        PlayerPrefs.Save();
    }

    private void LoadSettings()
    {
        if (DataHolder.NickName == null)
        {
            if (PlayerPrefs.HasKey("NickName"))
                DataHolder.NickName = PlayerPrefs.GetString("NickName");

            if (PlayerPrefs.HasKey("Volume"))
                DataHolder.Volume = PlayerPrefs.GetFloat("Volume");
        }        
    }
}