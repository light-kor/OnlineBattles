using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public const float AnimTime = 0.5f;
    private const int FrameRate = 60;

    [HideInInspector] public a_ChangePanel _panelAnim;

    [SerializeField] private GameObject _mainPanel, _wifiPanel, _settings, _nameField, _waitingForLevelSelection, _lvlPanel;
    [SerializeField] private MultiBackButton _multiBackButton;
    [SerializeField] private ScrollRect _lvlScrollbar;
    private WifiMenuComponents _wifiComponents;
    private string _lvlName = "";

    private void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = FrameRate;
        Network.TcpConnectionIsDone += TcpConnectionIsReady;

        _wifiComponents = GetComponent<WifiMenuComponents>();
        _panelAnim = GetComponent<a_ChangePanel>();
        _settings.GetComponent<PlayerSettings>().LoadSettings(); // Нужно это сделать тут, а то тот объект со скриптом в начале неактивен
        DataHolder.ResetScore();
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
                NotificationManager.NM.CloseNotification();
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
                _panelAnim.StartTransition(ActivateMainMenu);
                new Notification("Сервер отключён", Notification.ButtonTypes.SimpleClose);
            }
            DataHolder.MessageTCPforGame.RemoveAt(0);
        }
    }

    public void SelectGame(int lvlNum)
    {
        DataHolder.SelectedServerGame = lvlNum;

        if (DataHolder.GameType == DataHolder.GameTypes.Single)
        {
            SceneManager.LoadScene("lvl" + lvlNum);
        }
        else if (DataHolder.GameType == DataHolder.GameTypes.WifiHost)
        {
            if (WifiServer_Host.OpponentIsReady == false)
            {
                new Notification("Ожидайте второго игрока", Notification.ButtonTypes.SimpleClose);
            }
            else
            {
                WifiServer_Host.SendTcpMessage("wifi_go " + "lvl" + lvlNum);
                SceneManager.LoadScene("lvl" + lvlNum);
                //TODO: Отправить инфу второму игроку
            }
        }
        else if (DataHolder.GameType == DataHolder.GameTypes.Multiplayer)
        {
            //TODO: Добавить анимацию ожидания.
            new Notification("Поиск игры", Notification.ButtonTypes.CancelGameSearch);
            _lvlName = "lvl" + lvlNum;
            DataHolder.ClientTCP.SendMessage($"game {lvlNum}");
        }
    }

    public void ChoseStartView() //TODO: Мб добавить _panelAnim ко всему.
    {
        if (DataHolder.GameType == DataHolder.GameTypes.Null)
            ActivateMainMenu();
        else if (DataHolder.GameType == DataHolder.GameTypes.Single)
            ActivateSingleplayerMenu();
        else if (DataHolder.GameType == DataHolder.GameTypes.WifiHost)
        {
            ActivateCreateWifiMenu();
            _wifiComponents.ChangeOpponentNameAndButton(false);
        }
        else if (DataHolder.GameType == DataHolder.GameTypes.WifiClient)
            ActivateWaitingWifiLvl();
        else if (DataHolder.GameType == DataHolder.GameTypes.Multiplayer)
            ActivateMultiplayerMenu();

        //DataHolder.GameType = DataHolder.GameTypes.Null; // Видимо это будет мешать
    }

    /// <summary>
    /// Обработка кнопки и установка режима одиночной игры.
    /// </summary>
    public void SelectSingleGame()
    {
        DataHolder.GameType = DataHolder.GameTypes.Single;
        _panelAnim.StartTransition(ActivateSingleplayerMenu);
    }

    /// <summary>
    /// Обработка кнопки и установка режима игры по wifi.
    /// </summary>
    public void SelectWifiGame()
    {
        if (!CheckNickNameAvailability()) return;

        _panelAnim.StartTransition(ActivateWifiMenu);
    }

    /// <summary>
    /// Обработка кнопки, проверка/установка соединения с сервером и установка режима мультиплеера.
    /// </summary>
    public void SelectMultiplayerGame()
    {
        new Notification("Сервер недоступен", Notification.ButtonTypes.SimpleClose); //TODO: Временная строка

        //if (!CheckNickNameAvailability()) return;

        //DataHolder.GameType = "Multiplayer";
        //if (!DataHolder.Connected)
        //    Network.CreateTCP();

        //TcpConnectionIsReady();
    }

    public void Settings()
    {
        _settings.SetActive(true);
    }

    public void TcpConnectionIsReady()
    {
        if (DataHolder.GameType == DataHolder.GameTypes.Multiplayer) 
            _panelAnim.StartTransition(ActivateMultiplayerMenu);
    }

    public void MultiBack() //TODO: !!Добавить кнопку и завершение всех соединений на момент, когда ты уже имеешь подключённого чела и хочешь выйти!!
    {
        if (DataHolder.GameType == DataHolder.GameTypes.WifiClient)
        {
            Network.CloseWifiServerSearcher();

            if (DataHolder.ClientTCP != null)
            {
                DataHolder.ClientTCP.SendMessage("disconnect");
                Network.CloseTcpConnection();
            }
        }
        else if (DataHolder.GameType == DataHolder.GameTypes.WifiHost)
        {
            if (WifiServer_Host.Opponent != null)
            {
                WifiServer_Host.SendTcpMessage("disconnect");
                WifiServer_Host.CloseConnection(); // Это если игрок уже подключён
            }
            else
                WifiServer_Host.CancelConnect(); // Это если игрок ещё не нашёлся

            //TODO: Добавть мультиплеер
        }
        _panelAnim.StartTransition(ActivateMainMenu);
    }

    private bool CheckNickNameAvailability()
    {
        if (DataHolder.NickName == null || DataHolder.NickName == "")
        {
            _nameField.SetActive(true);
            return false;
        }
        else return true;
    }

    #region ActivatePanels

    public void ActivateMainMenu() // # 1
    {
        DeactivateAllPanels();
        _mainPanel.SetActive(true);
    }

    private void ActivateSingleplayerMenu() // # 2
    {
        _mainPanel.SetActive(false);
        ActivateLvlPanel();
        _multiBackButton.ShowMultiBackButton(MultiBackButton.ButtonTypes.Back);
    }

    private void ActivateWifiMenu() // # 3
    {
        _mainPanel.SetActive(false);
        _wifiPanel.SetActive(true);
        _multiBackButton.ShowMultiBackButton(MultiBackButton.ButtonTypes.Back);
    }

    public void ActivateCreateWifiMenu()  // # 3.1
    {
        _mainPanel.SetActive(false); // Чтоб после игры она не вылезала (т.к. она вкл в инспекторе)
        _wifiPanel.SetActive(false);
        ActivateLvlPanel();
        _multiBackButton.ShowMultiBackButton(MultiBackButton.ButtonTypes.Cancel);
        _wifiComponents.ShowOpponentNameObj();
    }

    public void ActivateConnectWifiMenu()  // # 3.2
    {
        _wifiPanel.SetActive(false);
        _wifiComponents.ActivateServerSearchPanel();
        _multiBackButton.ShowMultiBackButton(MultiBackButton.ButtonTypes.Cancel);
    }

    public void ActivateWaitingWifiLvl() // # 3.2.1
    {
        _mainPanel.SetActive(false); // Чтоб после игры она не вылезала (т.к. она вкл в инспекторе)
        _wifiComponents.DeactivateServerSearchAndName();
        _waitingForLevelSelection.SetActive(true);
        _multiBackButton.ShowMultiBackButton(MultiBackButton.ButtonTypes.Disconnect);
    }

    private void ActivateMultiplayerMenu()  // # 4
    {
        _mainPanel.SetActive(false);
        ActivateLvlPanel();
        _multiBackButton.ShowMultiBackButton(MultiBackButton.ButtonTypes.Disconnect); //TODO: Это вроде нигде не обрабатывается
    }

    private void ActivateLvlPanel()  // # 4
    {
        _lvlScrollbar.verticalNormalizedPosition = 2f; 
        // 1f - это просто наверху, 0f - внизу, а с 2f контент выкатывается снизу к нормальному положению
        _lvlPanel.SetActive(true);
    }

    private void DeactivateAllPanels()
    {
        _mainPanel.SetActive(false);
        _lvlPanel.SetActive(false);
        _wifiPanel.SetActive(false);
        _waitingForLevelSelection.SetActive(false);

        _wifiComponents.DeactivateServerSearchAndName();
        _multiBackButton.DeactivateButton();
    }

    #endregion

    private void OnDestroy()
    {
        Network.TcpConnectionIsDone -= TcpConnectionIsReady;
    }


    //TODO: Добавить camera shake. Видос в вк

    //TODO: На всех сценах сделать match width or height

    //TODO: Если к тебе кто-то подключается, может выпасть пустое сообщение с опросом. Видимо уведомление загружается быстрее, чем контент для него

    //TODO: Вроде как может появитьяс баг, если ты клиент, и нажал отмену через секунду после начала коннекта.

    //TODO: Добавить проверку на то, готов ли играть оппонент. Если вы уже закончили игру, тебе надо дождаться, чтоб он вышел из прошлой игры.

    //TODO: Если тебя отклонили, то просто закрой уведомление и обнови список. Не выкидывать в меню

    //TODO: Добавить Abort и Join всех тредов в коде

    //TODO: Проблемы с анимацией скрытия и показа кнопки. Надо сделать задержку.

    //TODO: Если ты сам отменил коннект или сервер недоступен, то очисти список серверов

    //TODO: Пока отключил возможность реконнкта в TCPConnect.

    //TODO: Если чел попытался нажать на игру недождавшись второго игрока, то выйдет уведомление, и оно будет мешать уведомлению с одобрением 

    //TODO: Если противник ддисконнекнулся, а ты хост, то тебя без анимации перебросит в меню. Ну или мне кажется

    //TODO: Если человек зашёл в wifi игру, то надо чекнуть, подключен ли он к wifi, и вывести маленкое уведомление. Ну или вообще не разрешать играть

    //TODO: Как-то сравнивать версии приложения и не пускать в игру, если они сильно различаются

    //TODO: В настройках добавить вкладку "О приложении", и там хранить инфу о версии и прочее.

    //TODO: Пересмотреть всю систему поддержания жизни соединения.

    //TODO: Перед выходом из матча добавить уведомление с уточнением. Точно ли игрок хочет выйти.

    //TODO: Полностью просмотреть всю логику дисконнекта во время wifi игры. Оппонент не всегда корректно реагирует

    //TODO: Сделать маленькие уведомления - подсказки. Чтоб вылетали снизу и сами исчезали через время

    //TODO: Что делать, если оппонент вышел из поиска, а только потом ты ео принял. Надо как-то обработать.

    //TODO: Если ты отменяешь ожидание подключения, то надо послать серверу инфу, что ты отказываешься. (В том числе и по wifi).
    //Мб показать хосту как уведомление. ТОгда можно выделить отдельный тип уведомлений (wifi..) и тогда емё не надо будет нажимать, уведомление само обновится

    //TODO: Мб добвить уведомление что Wifi запрос принят. (Возможно маленькое уведомление снизу)

    //TODO: Добавить имя wifi соперника в уведомление, что он ливнул

    //TODO: Кто первый ходит в кресттиках ноликах. Там вроде баг, любой модет начать первым. Ну и надо бы показать игрокам, кто первый

    //TODO: Разобраться со static gameObject. Поставил много где в главном меню, ну и в префабе уведомлений

    //TODO: Нужен ли CanStartReconnect

    //TODO: DataHolder.Connected нужен ли он вообще

    //TODO: Игрок должен посылать сообщение "Готов" после выхода из игры, что б сервер не смог начать новыую игру, пока игрок ещё не вышел из прошлой

    //TODO: УБРАТЬ "g" к каждому игровому udp сообщению, ну или везде придумать общую систему.

    //TODO: Добавил "g" к каждому игровому udp сообщению. НО НЕ УЧЁЛ ЭТО НА СЕРВЕРЕ!!!!

    //TODO: Сбрасывать значения в DataHolder после онлайн матча

    //TODO: Если ты потерял сеть во время игры, всё ли будет потом нормально? Ты сможешь продолжить играть? Будет ли ждать противник? Надо как-то отправлять подтверждение, что ты вернулся и готов

    //TODO: Присылать инфу о том, что противник отключился не только если игрок сам что-то делает, а сразу после истечения таймера

    //TODO: Переделать все листы в Queue млм подобное, что не так трудозатратно

    //TODO: Если чел вышел из игры во время матча, то сервер говорит второму, что тот выиграл. Но надо ещё добавить, чтоб сразу дисконнектить первого с сервера.

    //TODO: Проверить соответствие всех сообщений от сервера и игрока (сообщение 404 только на сервере).

    //TODO: Добавить на сервере try для всех Convert.ToInt32 и всех возможных несостыковок типов данных

    //TODO: Создать словари для всех string, чтоб добавить несколько языков

    //TODO: В начале каждой игры сделать заставку, чтоб успели прийти первые данные с позиционированием игроков, до того, как они попытаются ходить

    //TODO: Если пришло несколько сообщений подряд, то нужно все их отработать на клиенте. А то одно может отменять предыдущее и тд.

    //TODO: Сейчас награда добавляется в SplitByLobby, но это надо куда-то переместить и добавить выбор

    //TODO: Возможно вынести коннект с сервером ещё до главного меню

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

    //TODO: Раз в какае-то время можно получать монеты за рекламу, но пересылать их другим можно только с определённой суммы. Как-то защититься от взлома

    //TODO: Лого и название компании

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

