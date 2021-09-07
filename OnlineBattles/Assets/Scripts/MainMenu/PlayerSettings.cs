using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSettings : MonoBehaviour
{
    [SerializeField] private Button _close;
    [SerializeField] private Slider _volume;
    [SerializeField] private TMP_InputField _nickName;
    private a_ShowMovingPanel _anim;

    private void Awake()
    {
        _anim = GetComponent<a_ShowMovingPanel>();
        _close.onClick.AddListener(() => ClosePanel());
    }

    private void OnEnable()
    {
        ShowSavesOnUI();
        _anim.ShowPanel(null);
    }

    private void ClosePanel()
    {
        SaveSettings();
        _anim.ClosePanel();
    }

    private void ShowSavesOnUI()
    {
        _volume.value = DataHolder.Volume;
        _nickName.text = DataHolder.NickName;
    }

    private void SaveSettings()
    {
        DataHolder.Volume = _volume.value;
        DataHolder.NickName = _nickName.text;

        PlayerPrefs.SetFloat("Volume", DataHolder.Volume);
        PlayerPrefs.SetString("NickName", DataHolder.NickName);
        PlayerPrefs.Save();
    }

    public void LoadSettings()
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
