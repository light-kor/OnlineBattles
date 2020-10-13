﻿using System;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuScr : MonoBehaviour
{
    public Text money, id;
    public GameObject mainPanel, lvlPanel, notifPanel;
    public GameObject shield; // Блокирует нажатия на все кнопки, кроме notifPanel


    private bool makelvlChoice = false;
    private string lvlName = "";
    private DateTime timeOfChoice = DateTime.UtcNow;

    void Start()
    {

    }

    void Update()
    {
        // Принимаем сообщение о старте игры
        while (DataHolder.messageTCP.Count > 0)
        {
            string[] mes = DataHolder.messageTCP[0].Split(' ');

            if (mes[0] == "S")
            {
                DataHolder.thisGameID = Convert.ToInt32(mes[1]);
                UnityEngine.SceneManagement.SceneManager.LoadScene(lvlName);
            }
            DataHolder.messageTCP.RemoveAt(0);
        }

        // Есть 3 секунды ожидания, пока сервер не скажет начинать игру
        if (makelvlChoice)
        {
            if ((DateTime.UtcNow - timeOfChoice).TotalSeconds > 3f)
            {
                shield.SetActive(false);
                makelvlChoice = false;
            }
            //TODO: Отправить на сервер что-то про отмену игры? Или игроку уведомление, что соединение потеряно
        }


        // Если Read поймёт, что сеть прервалось, сработает это и начнётся востановление сети.
        if (DataHolder.needToReconnect)
        {
            DataHolder.needToReconnect = false;
            StartReconnect();
        }

    }

    public void SelectSingleGame()
    {
        DataHolder.GameType = 1;
        MoveMenuPanels();
    }

    public void SelectWifiGame()
    {
        DataHolder.GameType = 2;
        MoveMenuPanels();
    }

    public void SelectMultiplayerGame()
    {
        //TODO: Что делать если ты уже подключался, но сервер тебя удалил или пр, ну то есть у тебя connected, но сервер больше не отвечает
        // Если нет подключения, то коннектим 
        if (!DataHolder.Connected)
        {
            //TODO: Добавить анимацию загрузки, что было понятно, что надо подождать
            shield.SetActive(true);

            //TODO: Не работает на андроиде
            //if (!DataHolder.CheckConnection())
            //{
            //    DataHolder.ShowNotif(notifPanel, 2);
            //    return;
            //}

            DataHolder.CreateTCP();
        }

        //Если всё норм, то переходим на другую сцену
        if (DataHolder.Connected) //TODO: При каждом нажатии проверять связь с сервером, а не коннект переменную
        {
            DataHolder.GameType = 3;
            GetMoney();
            MoveMenuPanels();
            shield.SetActive(false);
        }
        else DataHolder.ShowNotif(notifPanel, 0);

    }

    public void GetMoney()
    {
        if (DataHolder.Connected)
        {
            money.text = DataHolder.Money.ToString();
            id.text = DataHolder.MyID.ToString();
        }
    }

    public void SelectGame(GameObject button)
    {
        if (button.name == "TicTacToe")
        {
            if (DataHolder.GameType == 1)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("TicTacToe_Single");
            }
            else if (DataHolder.GameType == 2)
            {

            }
            else if (DataHolder.GameType == 3)
            {
                // Выключить все кнопки перед этим, чтоб игрок никуда не мог нажать 3 секунды
                lvlName = "lvl1";
                DataHolder.ClientTCP.SendMassage("1");
                // Выключаем кнопки выбора уровней, пока ждём ответ со стартом
                shield.SetActive(true);
                timeOfChoice = DateTime.UtcNow;
                makelvlChoice = true;
            }

        }
        if (button.name == "Second")
        {
            if (DataHolder.GameType == 1)
            {
                //UnityEngine.SceneManagement.SceneManager.LoadScene("TicTacToe_Single");
            }
            else if (DataHolder.GameType == 2)
            {

            }
            else if (DataHolder.GameType == 3)
            {
                lvlName = "UdpLVL";
                DataHolder.ClientTCP.SendMassage("2");
                // Выключаем кнопки выбора уровней, пока ждём ответ со стартом
                shield.SetActive(true);
                timeOfChoice = DateTime.UtcNow;
                makelvlChoice = true;
            }
        }        
    }

    public void MoveMenuPanels()
    {
        mainPanel.SetActive(!mainPanel.activeSelf);
        lvlPanel.SetActive(!lvlPanel.activeSelf);
    }


    public void ExitNotif()
    {
        notifPanel.SetActive(false);
        shield.SetActive(false);
    }

    public void asda()
    {
        
    }

    public void StartReconnect()
    {
        DataHolder.Connected = false;
        shield.SetActive(true);
        DataHolder.ShowNotif(notifPanel, 1);
        InvokeRepeating("TryReconnect", 0.0f, 1.0f);
    }

    public void TryReconnect()
    {
        DataHolder.ClientTCP.Reconnect(notifPanel);
        if (DataHolder.Connected == true)
        {
            CancelInvoke("TryReconnect");
            shield.SetActive(false);
            notifPanel.SetActive(false);
        }
    }




    //TODO: При обрыве соединения, посылать клиент в отдыльный поток, где он несколько секунд будет висеть в ожидании соединения. Или же 
    // таки сделать общий лист всех игроков, чтоб чекать есть ли уже такой игрок и если есть то делать реконнект и возвращать в игру
    // ЕСли выкинуло во время игры, то шлёшь номер игры с подписью реконект, и тогда сервер чекнет именно в списке нужной игры и вернёт тебя, или же
    // если там пусто, то вы не успели, досвидания

    //TODO: добавить к notif panel езё одну родительску панель, которая перекроет все кнопки, и их не нужно будет ограничиваь кодом

    //TODO: Человек мжет отменить процесс переподключения, но должна появиться кнопка с возможностью заново начать переподключаться

    //TODO: Корректно завершать соединение и поток, когда выходишь из игры, или хочешь заново подключиться или при постоянном вкл и выкл udp

    //TODO: Ещё перед созданием лобби чекнуть, не прислал ли что-то игрок тк мб он хочет выйти из игры и отменить поиск

    //TODO: Отмена игры, если человек не хочет ждать, когда ему подберут противника, когд сервера пустые

    //TODO: Что-то сделать если связь оборвалась, видимо нужно заново создать  tcpconnect

    //TODO: Может всё-таки добавить реконнект

    //TODO: Обработать сообщение 404 от сервера, да и вообзе провкетить, чтоб все сообщения были учтены и обрабатывались

    //TODO: многие моменты было бы лучше реализовать через события а не общедоступные флаги

    //TODO: Коннект к лобби по рандомному ключ номеру

    //TODO: Сделать автоматический возврат в меню после игры через 10 секунд

    //TODO: ЧТо делать, если человек понял, что уже проиграл и ливнул в конце игры, он захочет начать другую игру, надо как-то запретить ему

    //TODO: [SerializeField] ////// Для чего вообще это нужно (по пунктам)

    //TODO: Мб подгружать сцены с игрой, пока идёт поиск

    //TODO: Добавить возможность остановки поиска игры

    //TODO: должна быть специальная команда проверки версии приложения, чтоб не самому создавать переменную, которую смогут изменить

    //TODO: После обновления данных о деньгах надо как-то грамотно передать их пользователю, но не в функции UpdateDBMoney

    //TODO: Если на сервере произошла критическая ошибка, то отловить её и перезапустить сервер автоматически

    //TODO: Похоже в крестиках можно продолжить шевелиться после окончания матча

    //TODO: При отмене игры тебя должно вернуть в главное меню

    //TODO: Сначала дождаться сообщения о том, что игру можно начинать, а только потом открыть нужную сцену

    //TODO: Что делать, если пользователь меняет моб инет на wifi или ещё что-то подобное

    //TODO: Настроить камеру и вообще узнать, зачем все кнопки

    //TODO: Сделать название игры картинкой и поставить за персонажей

    //TODO: Все служебные команды считывать в DataHolder, а остальные уже потом в других скриптах
    // Так, когда сервер остановится, он всем пошлёт, что на сервере технические работ. И он должен мочь менять сцены и тд, если нужно закончить игру например

    //TODO: Как сделать автонастройку объектов под разрешение

    //TODO: Делать анимации через код или юнити (тогда удалить скрипт MenuMove)

    //TODO: Интересные переходы(прокрутки) между меню, когда всё на одной сцене

    //TODO: Может нужны не отдельные потоки, а пулл потоков?

    //TODO: Раз в какае-то время можно получать деньги за рекламу, но пересылать их другим можно только с определённой суммы. Как-то защититься от взлома

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

    //TODO: Как убрать калю или чёлку на телефонах

    //TODO: Как запрашивать разрешения для приложения при установке

    //TODO: Как ускорить загрузку лого юнити

    //TODO: Страничка с запретом мультиплеера даже по wifi, пока не обновишь игру

    //TODO: Удалить все лишние пакеты Юнити (2д свет и прочая фигня в проекте)

    //TODO: Стресс-тест серверу, проверить его возможности, ну и устроить тест хотя-бы 100 человек единовременно

    //TODO: Везде ли включён UPnP? Если роутер клиента так не умеет

    //TODO: Иногда порты могут быть заняты, нужны варианты портов для проброса

    //TODO: Страничка с инфой о конфиденциальности

    //TODO: Кэширование запросов к бд? Кэширование всего, что можно)

    //TODO: Expand. Скейлить канвас не как я обычно, а по фиксированной величине, как Флатинго https://youto.be/urU6u0446qA

    //TODO: Сделать параллельность запросов к БД, сколько потоков она поддерживает? Можно ли во время игры, менять значения через студию?

    //TODO: Нужно ли каждый раз авторизовываться как в пабге?

    //TODO: Как-то генерировать ключи, хранить во внешней бд, или хз как. Главное - чтоб никто не мог зайти за чужой аккаунт

    //TODO: Общий счёт кредитов или кд по группам, если делать кланы или тп

    //TODO: Возможность передачи кредитов в игре???

    //TODO: Сделать кнопку досрочного выхода из матча, соответственно тех-победа второго (обработай сразу, без ожидания)

    //TODO: При подключении нового устройства, проверять, есть ли этот аккакунт в сети и решать проблему (либо массив подключённых id, либо в таблице bool connected)
    // ЧТоб нельзя было играть по сети с одного id на нескольких устройствах

    //TODO: Продумать подключение игроков к системе( логин и пароль, рандомные ключи, почта и пароль и тд)

    //TODO: Как пробросить порты, если клиент находится за несколькими NAT, как в матрёшке (надо ли вообще пробрасывать клиентам)

    //TODO: Если не удалось установить соединение, то поверх кнопки мультиплеера будет кнопка релоуда соединения (ну и деньги обновятся)

    //TODO: Установить на сервер защиту от DDoS

    //TODO: Регистрация пользователей Андроид через Гугл, Что для ios? Ну и сделать обычную через логин и пароль для всех

    //TODO: Проверить, загрузилась ли сцена, только потом принимать и обрабатывать сообщения

    //TODO: Что делать, если сообщения с сервера не отправляются, не просто оповестить об ошибке, а обработать

    //TODO: Что делать клиенту, если вылетит сервер

    //TODO: Если sql запрос будет выполнен криво, то сервер пришлёт пустоту, надо фиксить

    //TODO: Сервер крашится при поступлении двух клиентов с одним сокетом, что делать?

    //TODO: Пуш-уведомления, чтоб завлечь игрока в игру

    //TODO: Application.runInBackground = true; Надо ли это везде?

    //TODO: Можно заранее подгружать сцены, чтоб загружать рекламу и тд https://habr.com/ru/post/440718/

    //TODO: Какую инфу об состоянии сервера сохранять? Сделать вывод в текстовый файл по всем матчас и количеству мматчей за день

    //TODO: Сколько запросов в секунду сможет обрабатывать ноутбук и провайдер, если использовать белый ip

    //TODO: Сделать подключение к бд многопоточным, чтоб она не крашилать от этого

    //TODO: При входе загружаются деньги и устанавливается общий коннект, елси была ошибка, то просто забить до момента подключения к игре
    //Там проверить деньги ещё раз, и только после этого отсылать инфу про выбор игры, те у игрока перед всеми действиями проверять коннект, елси нет, то просить деньги 

    //TODO: Добавить возможность настройки ставки для игры

    //TODO: Клиенты перед игрой снимают у игроков по N кредитов, но на сервере происходят изменения только после окончасния игры. Хранить деньги в классе клиента, чтоб смотреть, можно линачинать игу игроку, хэватает ли денег

    //TODO: Сделать внешний сервер с белым айпи, который будет переадресовывать всё к тебе

    //TODO: Не давать зайти в игру, если недостатчно денег, при чём проверять сначала на клиенте, потом и на сервере

    //TODO: Как искать другие устройства в в игре в LAN

    //TODO: если wifi соединение, то надо пробрасывать и тд, а если нет, то просто коннект

    //TODO: нужна логика закрытия портов, если они будт разные на кажом устройстве в одной lan

    //TODO: если сообщения от сервера идут через роутер и порт, а у двух клиентов он одинаковый, то данные запутаются 
    //TODO: путь порт на каждом устройстве выдаётся рандомно, если несколько устройств подключено от одного wifi
    //TODO: клиент сначала ищет незанятый порт, а потом шлёт его на сервер, чтоб подключиться через udp

    //TODO: Нужно пробросить порты как на сервере, так и на клиенте

    //TODO: Если собрал не 4 в ряд, а больше в крестиках-ноликах, то можно накидывать сверху бонусных денег.

    //TODO: при подключении запрашивать версию игры и не запускать онлайн без обновления

    //TODO: Что делать, если оба прислали код завершения игры, но он не совпал

    //TODO: Если кто-то регулярно будет не принимать игру и заставлять других ждать, просто банить их

    //TODO: Настройка игры под все разрешения, учитывая капли и вырезы

    //TODO: Зашифровать весь код и как-то защититься от взлома

    //TODO: Обновление денег у игроков, когда они зашли в игру, и когда они вышли из матча

    //TODO: Как часто нужно обновлять данные в быстрых играх на udp

    //TODO: При игре с друзьями в локалке можно использовать широковещательную рассылку

    //TODO: видимо нужно пробрасывать порты для подключённых по wifi

    //TODO: Хранить на сервере файлы со всеми записями игр, а в бд хранить имена/ссылки итд, чтоб переходить к файлам

    //TODO: Сколько запросов одновременно обрабатывает mySQL, нужен ли мьютекс

    //TODO: Не раздавать первую 1000 id, оставить на всякий случай

    //TODO: Сделвть проверку подключения к серверу, если у сервера проблемы, то не пускать в онлайн игру

    //TODO: Не давать играть по локальной сети, если не совпаддают версии, нужно при коннекте запрашивать версию игры

    //TODO: (для турниров преимущ) если кто-то долго ждёт и вышел из приложения в меню, нужнно отследить и прислать ему пуш, когда надо будет начать

}

