using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public static event DataHolder.GameNotification ShowGameNotification;
    public GameObject _lvlChoseWaiting, _lvlPanel;
    [SerializeField] private GameObject _mainPanel, _wifiPanel, _multiBackButton;

    private WifiMenuComponents WifiMenu;
    private string _lvlName { get; set; } = "";

    private void Start()
    {
        WifiMenu = GetComponent<WifiMenuComponents>();
        Network.TcpConnectionIsDone += TcpConnectionIsReady;
        ChoseStartView();       
    }

    private void Update()
    {
        Network.ConnectionLifeSupport(); //TODO: Может вынести как-то поудобнее

        if (DataHolder.MessageTCPforGame.Count > 0)
        {
            string[] mes = DataHolder.MessageTCPforGame[0].Split(' ');
            if (mes[0] == "S")
            {
                DataHolder.IDInThisGame = Convert.ToInt32(mes[1]);
                DataHolder.LobbyID = Convert.ToInt32(mes[2]);
                SceneManager.LoadScene(_lvlName);
                DataHolder.NotifPanels.NotificatonMultyButton(0);
            }
            // Значит до этого игрок вылетел, и сейчас может восстановиться в игре
            else if (mes[0] == "goto")
            {
                DataHolder.IDInThisGame = Convert.ToInt32(mes[2]);
                DataHolder.LobbyID = Convert.ToInt32(mes[3]);
                DataHolder.SelectedServerGame = Convert.ToInt32(mes[1]);
                if (DataHolder.SelectedServerGame == 2)
                {                  
                    SceneManager.LoadScene("lvl2");
                }                    
            }
            else if (mes[0] == "wifi_go")
            {
                SceneManager.LoadScene(mes[1]);
            }
            else if (mes[0] == "disconnect")
            {
                Network.CloseTcpConnection();
                _lvlChoseWaiting.SetActive(false);
                ActivateMenuPanel();
                ShowGameNotification?.Invoke("Сервер отключён", 1);
            }
            DataHolder.MessageTCPforGame.RemoveAt(0);
        }                          
    }

    public void SelectGame(int lvlNum)
    {
        DataHolder.SelectedServerGame = lvlNum;

        if (DataHolder.GameType == "OnPhone")
        {
            SceneManager.LoadScene("lvl" + lvlNum);
        }
        else if (DataHolder.GameType == "WifiServer")
        {
            if (WifiServer_Host.OpponentIsReady == false)
            {
                ShowGameNotification?.Invoke("Ожидайте второго игрока", 1);
            }
            else
            {
                WifiServer_Host.SendTcpMessage("wifi_go " + "lvl" + lvlNum);
                SceneManager.LoadScene("lvl" + lvlNum);
                //TODO: Отправить инфу второму игроку
            }
        }
        else if (DataHolder.GameType == "Multiplayer")
        {
            //TODO: Добавить анимацию ожидания.
            ShowGameNotification?.Invoke("Поиск игры", 3);
            _lvlName = "lvl" + lvlNum;
            DataHolder.ClientTCP.SendMessage($"game {lvlNum}");           
        }
    }

    public void ChoseStartView()
    {
        DeactivatePanels();

        if (DataHolder.StartMenuView == "WifiHost")
        {
            WifiMenu.WriteOpponentName();
            ActivatePanel(_lvlPanel);
        }
        else if (DataHolder.StartMenuView == "WifiClient")
        {            
            _lvlChoseWaiting.SetActive(true);
            ShowMultiBackButton("Отключиться");
        }
        else 
            ActivateMenuPanel();

        DataHolder.StartMenuView = null;
    }

    /// <summary>
    /// Обработка кнопки и установка режима одиночной игры.
    /// </summary>
    public void SelectSingleGame()
    {
        DataHolder.GameType = "OnPhone";
        ActivatePanel(_lvlPanel);
    }

    /// <summary>
    /// Обработка кнопки и установка режима игры по wifi.
    /// </summary>
    public void SelectWifiGame()
    {             
        ActivatePanel(_wifiPanel);
    }

    /// <summary>
    /// Обработка кнопки, проверка/установка соединения с сервером и установка режима мультиплеера.
    /// </summary>
    public void SelectMultiplayerGame()
    {
        DataHolder.GameType = "Multiplayer";
        if (!DataHolder.Connected)
            Network.CreateTCP();

        TcpConnectionIsReady();
    }
       
    public void TcpConnectionIsReady()
    {
        if (DataHolder.Connected)
        {           
            DataHolder.NotifPanels.NotificatonMultyButton(1);

            if (DataHolder.GameType == "Multiplayer")
                ActivatePanel(_lvlPanel);
        }
    }  

    public void MultiBackButton() //TODO: !!Добавить кнопку и завершение всех соединений на момент, когда ты уже имеешь подключённого чела и хочешь выйти!!
    {
        if (DataHolder.GameType == "WifiClient")
        {
            Network.CloseWifiServerSearcher();
            _lvlChoseWaiting.SetActive(false);

            if (DataHolder.ClientTCP != null)
            {
                DataHolder.ClientTCP.SendMessage("disconnect");
                Network.CloseTcpConnection();
            }                           
        }
        else if (DataHolder.GameType == "WifiServer")
        {
            if (WifiServer_Host._opponent != null)
            {
                WifiServer_Host.SendTcpMessage("disconnect");
                WifiServer_Host.CloseAll();
                WifiMenu.HideOpponentName();
            }
            else
            {
                WifiServer_Host.CancelWaiting();
            }
        }
        ActivateMenuPanel();
    }

    public void StopListener()
    {
        
    }

    #region ActivatePanels
    public void ActivatePanel(GameObject panel) // В основном для кнопки Back в меню
    {
        DeactivatePanels();
        panel.SetActive(true);

        if ((DataHolder.GameType == "WifiServer" && WifiServer_Host._opponent == null) || DataHolder.GameType == "WifiClient")
            ShowMultiBackButton("Отмена");
        else
            ShowMultiBackButton("Назад");           
    }

    public void ActivateMenuPanel() // В основном для кнопки Back в меню
    {
        DataHolder.GameType = null;
        DeactivatePanels();
        _mainPanel.SetActive(true);
    }

    public void ShowMultiBackButton(string text)
    {
        _multiBackButton.GetComponentInChildren<TMP_Text>().text = text;
        _multiBackButton.SetActive(true);
    }

    public void DeactivatePanels()
    {
        _mainPanel.SetActive(false);
        _lvlPanel.SetActive(false);
        _wifiPanel.SetActive(false);
        WifiMenu.DeactivateServerSearchPanel();

        _multiBackButton.SetActive(false);
    }
    #endregion

    //TODO: Настроить диапазон пикселей для появления серверов

    //TODO: Добавил "g" к каждому игровому udp сообщению. НО НЕ УЧЁЛ ЭТО НА СЕРВЕРЕ!!!!

    //TODO: Сбрасывать значения в DataHolder после онлайн матча

    //TODO: Если ты потерял сеть во время игры, всё ли будет потом нормально? Ты сможешь продолжить играть? Будет ли ждать противник? Надо как-то отправлять подтверждение, что ты вернулся и готов

    //TODO: Присылать инфу о том, что противник отключился не только если игрок сам что-то делает, а сразу после истечения таймера

    //TODO: Переделать все листы в Queue млм подобное, что не так трудозатратно

    //TODO: Если чел вышел из игры во время матча, то сервер говорит второму, что тот выиграл. Но надо ещё добавить, чтоб сразу дисконнектить первого с сервера.

    //TODO: Если вырубить сервер, то бесконечно на экране будет пустое уведомление, которое не убрать. Переработать  уведомления и кнопки

    //TODO: Проверить соответствие всех сообщений от сервера и игрока (сообщение 404 только на сервере).

    //TODO: Пусть кнопка отмены поиска появится не сразу, тогда не получится спамить на сервер

    //TODO: Добавить на сервере try для всех Convert.ToInt32 и всех возможных несостыковок типов данных

    //TODO: Создать словари для всех string

    //TODO: В начале каждой игры сделать заставку, чтоб успели прийти первые данные с позиционированием игроков, до того, как они попытаются ходить

    //TODO: Если пришло несколько сообщений подряд, то нужно все их отработать на клиенте. А то одно может отменять предыдущее и тд.

    //TODO: Если на экране одно уведомление, но второе сейчас важнее, то что? Как они накладываются друг на друга?

    //TODO: Сейчас награда добавляется в SplitByLobby, но это надо куда-то переместить и добавить выбор

    //TODO: Возможно вынести коннект с сервером ещё до главного меню

    //TODO: Всё ещё не сразу появляется уведомление, если всё хорошо подключается, получаетя секундная заминка

    //TODO: Досрочный выход из udp игры и отслеживание вылетов и автоматическая победа второго с таймером и тд

    //TODO: Если свернуть приложение, то не будет сообщений оразрыве связи, но ничего отправляться не будет

    //TODO: Написать чек-лист для каждого нового уровня

    //TODO: Нужно не автоматически перезагружать, а спрашивать, хочетли игрок вернуться

    //TODO: Учесть, чтоб постоянно убирать вышедших из AllLoginClients на сервере. Если человек вышел сразу после игры, это обработается?

    //TODO: Тут куча инфы про разные типы роутеров, и что где-то моё udp соединение может не срабоать - https://gamedev.ru/code/forum/?id=231916

    //TODO: Не пускать пользователя в игры, пока он в главном меню не пришлёт сообщение для логина

    //TODO: Ещё перед созданием лобби чекнуть, не прислал ли что-то игрок тк мб он хочет выйти из игры и отменить поиск

    //TODO: Коннект к лобби по рандомному ключ номеру

    //TODO: Сделать автоматический возврат в меню после игры через 10 секунд

    //TODO: ЧТо делать, если человек понял, что уже проиграл и ливнул в конце игры, он захочет начать другую игру, надо как-то запретить ему

    //TODO: Мб подгружать сцены с игрой, пока идёт поиск

    //TODO: должна быть специальная команда проверки версии приложения, чтоб не самому создавать переменную, которую смогут изменить

    //TODO: После обновления данных о деньгах надо как-то грамотно передать их пользователю, но не в функции UpdateDBMoney

    //TODO: Настроить камеру и вообще узнать, зачем все кнопки

    //TODO: Интересные переходы(прокрутки) между меню, когда всё на одной сцене

    //TODO: Раз в какае-то время можно получать монеты за рекламу, но пересылать их другим можно только с определённой суммы. Как-то защититься от взлома

    //TODO: Лого и название компании

    //TODO: Поместить на главный экран атрибуты из миниигр и как-то заанимировать. Те крестики, револьверы и прочее из всех игр

    //TODO: Добавить хотя-бы русский и английский

    //TODO: Анимации на каждую кнопку

    //TODO: Ширина области касания в 45 – 57 пикселей. Это норм размер для пальца

    //TODO: Несколько разрешений экрана, чтоб менять уровень графики

    //TODO: Можно для каждого игрока сохранять его победы, время победы, длительность игры и тд, а так же указывать противника. 
    //Записывать все его пополнения денег и просмотр спец рекламы, чтоб полностью контролить количество денег

    //TODO: Как запрашивать разрешения для приложения при установке

    //TODO: Как ускорить загрузку лого юнити

    //TODO: Удалить все лишние пакеты Юнити (2д свет и прочая фигня в проекте)

    //TODO: Стресс-тест серверу, проверить его возможности, ну и устроить тест хотя-бы 100 человек единовременно

    //TODO: Страничка с инфой о конфиденциальности

    //TODO: Кэширование запросов к бд? Кэширование всего, что можно)

    //TODO: Сделать параллельность запросов к БД, сколько потоков она поддерживает? Можно ли во время игры, менять значения через студию?

    //TODO: Нужно ли каждый раз авторизовываться как в пабге?

    //TODO: Как-то генерировать ключи, хранить во внешней бд, или хз как. Главное - чтоб никто не мог зайти за чужой аккаунт

    //TODO: Общий счёт кредитов или кд по группам, если делать кланы или тп

    //TODO: Возможность передачи кредитов в игре???

    //TODO: Продумать подключение игроков к системе( логин и пароль, рандомные ключи, почта и пароль и тд)

    //TODO: Установить на сервер защиту от DDoS

    //TODO: Регистрация пользователей Андроид через Гугл, Что для ios? Ну и сделать обычную через логин и пароль для всех

    //TODO: Проверить, загрузилась ли сцена, только потом принимать и обрабатывать сообщения

    //TODO: Пуш-уведомления, чтоб завлечь игрока в игру

    //TODO: Application.runInBackground = true; Надо ли это везде?

    //TODO: Можно заранее подгружать сцены, чтоб загружать рекламу и тд https://habr.com/ru/post/440718/

    //TODO: Какую инфу об состоянии сервера сохранять? Сделать вывод в текстовый файл по всем матчас и количеству мматчей за день

    //TODO: Сколько запросов в секунду сможет обрабатывать ноутбук и провайдер, если использовать белый ip

    //TODO: Сделать подключение к бд многопоточным, чтоб она не крашилать от этого

    //TODO: Добавить возможность настройки ставки для игры

    //TODO: Клиенты перед игрой снимают у игроков по N кредитов, но на сервере происходят изменения только после окончасния игры. Хранить деньги в классе клиента, чтоб смотреть, можно линачинать игу игроку, хэватает ли денег

    //TODO: Не давать зайти в онлайн игру, если недостатчно денег, при чём проверять сначала на клиенте, потом и на сервере

    //TODO: при подключении запрашивать версию игры и не запускать онлайн и не давать играть по локальной сети без обновления

    //TODO: Если кто-то регулярно будет не принимать игру и заставлять других ждать, просто банить их

    //TODO: Зашифровать весь код и как-то защититься от взлома

    //TODO: Обновление денег у игроков, когда они зашли в игру, и когда они вышли из матча

    //TODO: Хранить на сервере файлы со всеми записями игр, а в бд хранить имена/ссылки итд, чтоб переходить к файлам

    //TODO: Сколько запросов одновременно обрабатывает mySQL, нужен ли мьютекс

    //TODO: Не раздавать первую 1000 id, оставить на всякий случай

    //TODO: (для турниров преимущ) если кто-то долго ждёт и вышел из приложения в меню, нужнно отследить и прислать ему пуш, когда надо будет начать

}

