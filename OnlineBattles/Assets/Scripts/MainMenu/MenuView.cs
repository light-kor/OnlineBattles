using GameEnumerations;
using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(a_ChangePanel))]
public class MenuView : MonoBehaviour
{
    [SerializeField] private GameObject _mainPanel, _wifiPanel, _waitingForLevelSelection;
    [SerializeField] private LevelSelection _lvlPanel;
    [SerializeField] private MultiBackButton _multiBackButton;
    [SerializeField] private ScrollRect _lvlScrollbar;

    private WifiMenuComponents _wifiComponents;
    private a_ChangePanel _panelAnim;

    private void Start()
    {
        _wifiComponents = GetComponent<WifiMenuComponents>();
        _panelAnim = GetComponent<a_ChangePanel>();
        ChoseStartView();
    }

    public void ChoseStartView() //TODO: Мб добавить _panelAnim ко всему.
    {
        if (DataHolder.GameType == GameTypes.Null)
            ActivateMainMenu();
        else if (DataHolder.GameType == GameTypes.Single)
            ActivateSingleplayerMenu();
        else if (DataHolder.GameType == GameTypes.WifiHost)
        {
            ActivateCreateWifiMenu();
            _wifiComponents.ChangeOpponentNameAndButton(false);
        }
        else if (DataHolder.GameType == GameTypes.WifiClient)
            ActivateWaitingWifiLvl();
        else if (DataHolder.GameType == GameTypes.Multiplayer)
            ActivateMultiplayerMenu();
    }

    public void ChangePanelWithAnimation(Action action)
    {
        _panelAnim.StartTransition(action);
    }

    #region ActivatePanels

    public void ActivateMainMenu() // # 1
    {
        DeactivateAllPanels();
        _mainPanel.SetActive(true);
    }

    public void ActivateSingleplayerMenu() // # 2
    {
        _mainPanel.SetActive(false);
        ActivateLvlPanel();
        _multiBackButton.ShowMultiBackButton(BackButtonTypes.Back);
    }

    public void ActivateWifiMenu() // # 3
    {
        _mainPanel.SetActive(false);
        _wifiPanel.SetActive(true);
        _multiBackButton.ShowMultiBackButton(BackButtonTypes.Back);
    }

    public void ActivateCreateWifiMenu()  // # 3.1
    {
        _mainPanel.SetActive(false); // Чтоб после игры она не вылезала (т.к. она вкл в инспекторе)
        _wifiPanel.SetActive(false);
        ActivateLvlPanel();
        _multiBackButton.ShowMultiBackButton(BackButtonTypes.Cancel);
        _wifiComponents.ShowOpponentNameText();
    }

    public void ActivateConnectWifiMenu()  // # 3.2
    {
        _wifiPanel.SetActive(false);
        _wifiComponents.ActivateServerSearchPanel();
        _multiBackButton.ShowMultiBackButton(BackButtonTypes.Cancel);
    }

    public void ActivateWaitingWifiLvl() // # 3.2.1
    {
        _mainPanel.SetActive(false); // Чтоб после игры она не вылезала (т.к. она вкл в инспекторе)
        _wifiComponents.DeactivateServerSearchAndName();
        _waitingForLevelSelection.SetActive(true);
        _multiBackButton.ShowMultiBackButton(BackButtonTypes.Disconnect);
    }

    public void ActivateMultiplayerMenu()  // # 4
    {
        _mainPanel.SetActive(false);
        ActivateLvlPanel();
        _multiBackButton.ShowMultiBackButton(BackButtonTypes.Disconnect); //TODO: Это вроде нигде не обрабатывается
    }

    public void ActivateLvlPanel()  // # 4
    {
        _lvlScrollbar.verticalNormalizedPosition = 2f;
        // 1f - это просто наверху, 0f - внизу, а с 2f контент выкатывается снизу к нормальному положению
        _lvlPanel.gameObject.SetActive(true);
    }

    public void DeactivateAllPanels()
    {
        _mainPanel.SetActive(false);
        _lvlPanel.gameObject.SetActive(false);
        _wifiPanel.SetActive(false);
        _waitingForLevelSelection.SetActive(false);

        _wifiComponents.DeactivateServerSearchAndName();
        _multiBackButton.DeactivateButton();
    }

    #endregion
}