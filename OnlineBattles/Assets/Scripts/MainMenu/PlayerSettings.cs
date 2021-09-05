using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSettings : MonoBehaviour
{
    [SerializeField] private Button _close;
    [SerializeField] private Slider _volume;
    [SerializeField] private TMP_InputField _nickName;
    private a_Settings _anim;

    private void Start()
    {
        if (DataHolder.NickName == null)
            LoadGame();

        ShowSavesOnUI();

        _anim = GetComponent<a_Settings>();
        _close.onClick.AddListener(() => CloseNotification());
    }

    private void CloseNotification()
    {
        SaveGame();
        _anim.CloseNotification();
    }

    private void ShowSavesOnUI()
    {
        _volume.value = DataHolder.Volume;
        _nickName.text = DataHolder.NickName;
    }

    private void SaveGame()
    {
        DataHolder.Volume = _volume.value;
        DataHolder.NickName = _nickName.text;

        PlayerPrefs.SetFloat("Volume", DataHolder.Volume);
        PlayerPrefs.SetString("NickName", DataHolder.NickName);
        PlayerPrefs.Save();
    }

    private void LoadGame()
    {
        if (PlayerPrefs.HasKey("NickName"))
            DataHolder.NickName = PlayerPrefs.GetString("SavedString");

        if (PlayerPrefs.HasKey("Volume"))
            DataHolder.Volume = PlayerPrefs.GetFloat("Volume");
    }
}
