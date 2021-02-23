using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuScr : MonoBehaviour
{
    public static event DataHolder.GameNotification ShowGameNotification;
    [SerializeField] private Text _money, _id;
    [SerializeField] private TMP_Text _opponent;
    [SerializeField] private GameObject _mainPanel, _lvlPanel, _wifiPanel, _serverSearchPanel;
    [SerializeField] private GameObject _serverPrefab, _waitingAnim;

    private string _serverAnswer = null;
    private bool _canReadServerAnswer = false;
    private List<string> WifiServers = new List<string>();
    private string lvlName { get; set; } = "";

    private void Start()
    {
        WifiServer_Host.AcceptOpponent += ShowOpponentName;
        WifiServer_Host.CleanHostingUI += StopWaitingWifiPlayers;      
        Network.TcpConnectionIsDone += TcpConnectionIsReady;
        WifiServer_Connect.AddWifiServerToScreen += GetNewWifiServer;
        Network.WifiServerAnswer += WifiServerAnswerProcessing;
        ActivateMenuPanel();
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
                UnityEngine.SceneManagement.SceneManager.LoadScene(lvlName);
                DataHolder.NotifPanels.NotificatonMultyButton(0);
            }
            // Значит до этого игрок вылетел, и сейчас может восстановиться в игре
            else if (mes[0] == "goto") //TODO: А вот это больше почему-то не работает.
            {
                DataHolder.IDInThisGame = Convert.ToInt32(mes[2]);
                DataHolder.LobbyID = Convert.ToInt32(mes[3]);
                DataHolder.SelectedServerGame = Convert.ToInt32(mes[1]);
                if (DataHolder.SelectedServerGame == 2)
                {                  
                    UnityEngine.SceneManagement.SceneManager.LoadScene("lvl2");
                }                    
            }
            DataHolder.MessageTCPforGame.RemoveAt(0);
        }

        if (WifiServers.Count > 0)
        {
            CreateWifiServerCopy(WifiServers[0]);
            WifiServers.RemoveAt(0);
        }

        if (_canReadServerAnswer)
        {
            WifiServerAnswerProcessing();
            _canReadServerAnswer = false;
        }
            

    }

    public void SelectGame(int lvlNum)
    {
        DataHolder.SelectedServerGame = lvlNum;

        if (DataHolder.GameType == 1)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("TicTacToe_Single"); //TODO: Сделать по шаблону мультиплеера.
        }
        else if (DataHolder.GameType == 2)
        {
            if (WifiServer_Host.OpponentIsReady == false)
            {
                ShowGameNotification?.Invoke("Ожидайте второго игрока", 1);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("lvl" + lvlNum);
                //TODO: Отправить инфу второму игроку
            }
        }
        else if (DataHolder.GameType == 3)
        {
            //TODO: Добавить анимацию ожидания.
            ShowGameNotification?.Invoke("Поиск игры", 3);
            lvlName = "lvl" + lvlNum;
            DataHolder.ClientTCP.SendMessage($"game {lvlNum}");           
        }
    }

    
    /// <summary>
    /// Обработка кнопки и установка режима одиночной игры.
    /// </summary>
    public void SelectSingleGame()
    {
        DataHolder.GameType = 1;
        ActivatePanel(_lvlPanel);
    }

    /// <summary>
    /// Обработка кнопки и установка режима игры по wifi.
    /// </summary>
    public void SelectWifiGame()
    {
        DataHolder.GameType = 2;        
        ActivatePanel(_wifiPanel);
    }

    /// <summary>
    /// Обработка кнопки, проверка/установка соединения с сервером и установка режима мультиплеера.
    /// </summary>
    public void SelectMultiplayerGame()
    {
        DataHolder.GameType = 3;
        if (!DataHolder.Connected)
            Network.CreateTCP();

        TcpConnectionIsReady();
    }

    private void GetNewWifiServer(string text)
    {
        WifiServers.Add(text);
    }

    private void StopWaitingWifiPlayers()
    {
        _waitingAnim.SetActive(false);
        _opponent.gameObject.SetActive(false);
        ActivateMenuPanel();
    }

    private void CreateWifiServerCopy(string text)
    {
        string[] mes = text.Split(' ');       
        float x = 0, y = 0;
        while (x < 150 && x > -150)
        {
            x = UnityEngine.Random.Range(-360, 360);
        }

        while (y < 150 && y > -150)
        {
            y = UnityEngine.Random.Range(-640, 640);
        }
        GameObject pref = Instantiate(_serverPrefab, _serverSearchPanel.transform);
        pref.transform.localPosition = new Vector3(x, y, 0);
        pref.GetComponent<WifiServerSelect>().SetNameAndIP(text);
    }

    /// <summary>
    /// Переход в меню выбора мультиплеерных игр, показ money и id в UI.
    /// </summary>
    public void TcpConnectionIsReady()
    {
        if (DataHolder.Connected)
        {           
            DataHolder.NotifPanels.NotificatonMultyButton(1);

            if (DataHolder.GameType == 3)
                ActivatePanel(_lvlPanel);
        }
    }

    /// <summary>
    /// Установить на экране значение MyServerID и Money.
    /// </summary>
    private void GetMoney()
    {
        if (DataHolder.Connected)
        {
            _money.text = DataHolder.Money.ToString();
            _id.text = DataHolder.MyServerID.ToString();
        }
    }

    private void WifiServerAnswerProcessing(string text)
    {
        _serverAnswer = text;
        _canReadServerAnswer = true;
    }

    private void WifiServerAnswerProcessing()
    {
        if (_serverAnswer == "denied")
            ActivateMenuPanel();
        else if (_serverAnswer == "accept")
            DeactivatePanels();

        _serverAnswer = null;
    }

    void DestroyAllWifiServersIcons()
    {
        foreach (Transform g in _serverSearchPanel.GetComponentsInChildren<Transform>())
        {
            if (g.name.Contains("ServerSelect"))
                Destroy(g.gameObject);
        }
    }

    private void ShowOpponentName()
    {
        _waitingAnim.SetActive(false);
        _opponent.gameObject.SetActive(true);
        _opponent.text = "Подключён: " + WifiServer_Host._opponent.PlayerName;
    }

    public void SetHost()
    {        
        WifiServer_Host.StartHosting();
        _waitingAnim.SetActive(true);
        ActivatePanel(_lvlPanel);
    }

    public void ConnectToWifi()
    {
        DestroyAllWifiServersIcons();
        WifiServer_Connect.StartSearching();
        ActivatePanel(_serverSearchPanel);
    }

    public void StopListener()
    {

    }


    #region ActivatePanels
    public void ActivatePanel(GameObject panel) // В основном для кнопки Back в меню
    {
        DeactivatePanels();
        panel.SetActive(true);
    }

    public void ActivateMenuPanel() // В основном для кнопки Back в меню
    {
        DeactivatePanels();
        _mainPanel.SetActive(true);
    }

    private void DeactivatePanels()
    {
        _mainPanel.SetActive(false);
        _lvlPanel.SetActive(false);
        _wifiPanel.SetActive(false);
        _serverSearchPanel.SetActive(false);
    }
    #endregion


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

    //TODO: В начале каждой сцены выключать щит, панель и все кнопки, а то они могут залагать, и хер ты их выключишь

    //TODO: Если пришло несколько сообщений подряд, то нужно все их отработать на клиенте. А то одно может отменять предыдущее и тд.

    //TODO: Если на экране одно уведомление, но второе сейчас важнее, то что? Как они накладываются друг на друга?

    //TODO: Декодировать сообщения по битам, а не по пробелам

    //TODO: Сейчас награда добавляется в SplitByLobby, но это надо куда-то переместить и добавить выбор

    //TODO: Возможно вынести коннект с сервером ещё до главного меню

    //TODO: Всё ещё не сразу появляется уведомление, если всё хорошо подключается, получаетя секундная заминка

    //TODO: Досрочный выход из udp игры и отслеживание вылетов и автоматическая победа второго с таймером и тд

    //TODO: Если свернуть приложение, то не будет сообщений оразрыве связи, но ничего отправляться не будет

    //TODO: Написать чек-лист для каждого нового уровня

    //TODO: Нужно не автоматически перезагружать, а спрашивать, хочетли игрок вернуться

    //TODO: Перед стартом игры нужно ставить NotifPanel, NotifPanel, Shield на false. Тк игрок мог некорректно завершить прошлую игру, и в ной он начнёт с включёной этой хуйнёй

    //TODO: Учесть, чтоб постоянно убирать вышедших из AllLoginClients на сервере. Если человек вышел сразу после игры, это обработается?

    //TODO: Тут куча инфы про разные типы роутеров, и что где-то моё udp соединение может не срабоать - https://gamedev.ru/code/forum/?id=231916

    //TODO: Модно одновременно прислать S и Cancel, тогданадо как-то это обрабоать

    //TODO: Не пускать пользователя в игры, пока он в главном меню не пришлёт сообщение для логина

    //TODO: в udp не нужны |, но всё же на приёме нужен трай тк может не полностью сообщение прийти

    //TODO: Ещё перед созданием лобби чекнуть, не прислал ли что-то игрок тк мб он хочет выйти из игры и отменить поиск

    //TODO: многие моменты было бы лучше реализовать через события а не общедоступные флаги

    //TODO: Коннект к лобби по рандомному ключ номеру

    //TODO: Сделать автоматический возврат в меню после игры через 10 секунд

    //TODO: ЧТо делать, если человек понял, что уже проиграл и ливнул в конце игры, он захочет начать другую игру, надо как-то запретить ему

    //TODO: Мб подгружать сцены с игрой, пока идёт поиск

    //TODO: должна быть специальная команда проверки версии приложения, чтоб не самому создавать переменную, которую смогут изменить

    //TODO: После обновления данных о деньгах надо как-то грамотно передать их пользователю, но не в функции UpdateDBMoney

    //TODO: Настроить камеру и вообще узнать, зачем все кнопки

    //TODO: Как сделать автонастройку объектов под разрешение

    //TODO: Делать анимации через код или юнити (тогда удалить скрипт MenuMove)

    //TODO: Интересные переходы(прокрутки) между меню, когда всё на одной сцене

    //TODO: Раз в какае-то время можно получать монеты за рекламу, но пересылать их другим можно только с определённой суммы. Как-то защититься от взлома

    //TODO: Лого и название компании

    //TODO: Поместить на главный экран атрибуты из миниигр и как-то заанимировать. Те крестики, револьверы и прочее из всех игр

    //TODO: Добавить хотя-бы русский и английский

    //TODO: Выбрать шрифт для всех надписей в игре

    //TODO: Анимации на каждую кнопку

    //TODO: Ширина области касания в 45 – 57 пикселей. Это норм размер для пальца

    //TODO: Несколько разрешений экрана, чтоб менять уровень графики

    //TODO: Мб добавить ботов для игры одному (Хотя это уже извращение)

    //TODO: Можно для каждого игрока сохранять его победы, время победы, длительность игры и тд, а так же указывать противника. 
    //Записывать все его пополнения денег и просмотр спец рекламы, чтоб полностью контролить количество денег

    //TODO: Как убрать каплю или чёлку на телефонах

    //TODO: Как запрашивать разрешения для приложения при установке

    //TODO: Как ускорить загрузку лого юнити

    //TODO: Страничка с запретом мультиплеера даже по wifi, пока не обновишь игру

    //TODO: Удалить все лишние пакеты Юнити (2д свет и прочая фигня в проекте)

    //TODO: Стресс-тест серверу, проверить его возможности, ну и устроить тест хотя-бы 100 человек единовременно

    //TODO: Страничка с инфой о конфиденциальности

    //TODO: Кэширование запросов к бд? Кэширование всего, что можно)

    //TODO: Expand. Скейлить канвас не как я обычно, а по фиксированной величине, как Флатинго https://youto.be/urU6u0446qA

    //TODO: Сделать параллельность запросов к БД, сколько потоков она поддерживает? Можно ли во время игры, менять значения через студию?

    //TODO: Нужно ли каждый раз авторизовываться как в пабге?

    //TODO: Как-то генерировать ключи, хранить во внешней бд, или хз как. Главное - чтоб никто не мог зайти за чужой аккаунт

    //TODO: Общий счёт кредитов или кд по группам, если делать кланы или тп

    //TODO: Возможность передачи кредитов в игре???

    //TODO: Сделать кнопку досрочного выхода из матча, соответственно тех-победа второго (обработай сразу, без ожидания)

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

    //TODO: Как искать другие устройства в в игре в LAN

    //TODO: при подключении запрашивать версию игры и не запускать онлайн и не давать играть по локальной сети без обновления

    //TODO: Если кто-то регулярно будет не принимать игру и заставлять других ждать, просто банить их

    //TODO: Настройка игры под все разрешения, учитывая капли и вырезы

    //TODO: Зашифровать весь код и как-то защититься от взлома

    //TODO: Обновление денег у игроков, когда они зашли в игру, и когда они вышли из матча

    //TODO: При игре с друзьями в локалке можно использовать широковещательную рассылку

    //TODO: Хранить на сервере файлы со всеми записями игр, а в бд хранить имена/ссылки итд, чтоб переходить к файлам

    //TODO: Сколько запросов одновременно обрабатывает mySQL, нужен ли мьютекс

    //TODO: Не раздавать первую 1000 id, оставить на всякий случай

    //TODO: (для турниров преимущ) если кто-то долго ждёт и вышел из приложения в меню, нужнно отследить и прислать ему пуш, когда надо будет начать

}

