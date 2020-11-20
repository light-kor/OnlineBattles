﻿using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuScr : MonoBehaviour
{
    public Text Money, ID;
    public GameObject MainPanel, LvlPanel;

    private string lvlName = "";

    void Start()
    {

    }

    void Update()
    {         
        if (DataHolder.MessageTCPforGame.Count > 0)
        {
            string[] mes = DataHolder.MessageTCPforGame[0].Split(' ');

            if (mes[0] == "S")
            {
                DataHolder.ThisGameID = Convert.ToInt32(mes[1]);
                DataHolder.GameId = Convert.ToInt32(mes[2]);
                UnityEngine.SceneManagement.SceneManager.LoadScene(lvlName);
                //NetworkScript.CancelGameSearch(); //TODO: Надо ли? Всё равно загружается новая сцена и всё сбросится. Если включишь, то надо убрть в функции строку с отправкой сообщения об отмене.
            }
            // Значит до этого игрок вылетел, и сейчас может востановиться в игре
            else if (mes[0] == "goto")
            {
                DataHolder.ThisGameID = Convert.ToInt32(mes[2]);
                DataHolder.GameId = Convert.ToInt32(mes[3]);
                if (mes[1] == "2")
                {                  
                    UnityEngine.SceneManagement.SceneManager.LoadScene("UdpLVL");
                }
                    
            }

            DataHolder.MessageTCPforGame.RemoveAt(0); //TODO: Сделать нормальное централизованное удаление всех этих штук.                                                       // Можно свитчём, и потом с помощью goto отправлтять всех на удаление.
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

    public async void SelectMultiplayerGame()
    {
        //TODO: Может сразу после нажатия уже показывать уведомление, н6у или хотя бы счит поставить
        if (!DataHolder.Connected)
            DataHolder.NetworkScript.CreateTCP();                
        else
        {
            DataHolder.ClientTCP.SendMassage("Check"); // Если соединение уже было создано, то надо затестить  
            await Task.Delay(1000); //TODO: Надо ли? Да хз
        }
              
        // Если сеть была, но отлетела, то после Check Начнётся реконнект.
        GoToMultiplayerMenu();
    }

    public void GoToMultiplayerMenu()
    {
        if (DataHolder.Connected)
        {
            DataHolder.GameType = 3;
            DataHolder.NetworkScript.NotificatonMultyButton(1);
            MoveMenuPanels();
            GetMoney();
        }
    }

    public void GetMoney()
    {
        if (DataHolder.Connected)
        {
            Money.text = DataHolder.Money.ToString();
            ID.text = DataHolder.MyID.ToString();
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
                DataHolder.ClientTCP.SendMassage("game1");

                // Выключаем кнопки выбора уровней, пока ждём ответ со стартом
                DataHolder.NetworkScript.ShowNotif("Поиск игры", 3);
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
                DataHolder.ClientTCP.SendMassage("game2");

                // Выключаем кнопки выбора уровней, пока ждём ответ со стартом
                //TODO: Добавить анимацию ожидания
                DataHolder.NetworkScript.ShowNotif("Поиск игры", 3);
            }
        }        
    }

    public void MoveMenuPanels()
    {
        MainPanel.SetActive(!MainPanel.activeSelf);
        LvlPanel.SetActive(!LvlPanel.activeSelf);
    }

    //TODO: А если в какой-то момент я начал юзать ТСПотправку в нескольких местах одновременно. Всё же нахуй сломается.

    //TODO: Даже если игра нормально завершилась, удаляется ли udp экземпяры и потоки? Не помню, чтоб обрабатывал это!

    //TODO: В начале каждой игры сделать заставку, чтоб успели прийти первые данные с позиционированием игроков, до того, как они попытаются ходить

    //TODO: В начале кажодй сцены выключать щит, панель и все кнопки, а то они могут залагать, и хер ты их выключишь

    //TODO: Что если реконнект нанётся до окончания коннекта

    //TODO: Как-то сохранить, при каком действии произошёл дисконнект и выполнить его после востановления соединения

    //TODO: Если пришло несколько сообщений полряд, то нужно все их отраоать. А то одно может отменять предыдущее и тд.

    //TODO: Если игрок во время udp игры сменит сеть, то он об этом не узнает. Udp продолжится, а вот tcp упадёт

    //TODO: скрипт network стоит первый в загрузке, поэтому он первый скажет о себе

    //TODO: Если на экране одно уведомление, но второе сейчас важнее, то что? Как они накладываются друг на друга?

    //TODO: Полностью чистить DataHolder.MessageUDPget перед началом каждой игры

    //TODO: В конце чистить все листы

    //TODO: на телефоне не коннектится с первого раза к сервреу

    //TODO: Декодировать сообщения по битам, а ене по пробелам

    //TODO: Сейчас награда добавляется в SplitByLobby, но это надо куда-то переместить и добавить выбор

    //TODO: DataHolder.ClientUDP.SendMessage("Y"); - это я так сообщаю серверу, что я живой

    //TODO: Возможно вынести коннект с сервером ещё до главного меню

    //TODO: Всё ещё не сразу появляется уведомление, если всё хорошо подключается, получаетя секундная заминка

    //TODO: Обработать отмену поиска игры на сервере

    //TODO: Досрочный выход из udp игры и отслеживание вылетов и автоматическая победа второго с таймером и тд

    //TODO: Если свернуть приложение, то не будет сообщений оразрыве связи, но ничего отправляться не будет

    //TODO: Лобби почему-то не удаляется после завершения матча

    //TODO: Отмена поиска игры обработать на сервере

    //TODO: Написать чек-лист для каждого нового уровня

    //TODO: Нужно не автоматически перезагружать, а спрашивать, хочетли игрок вернуться

    //TODO: Обработать досрочный выход из игры в главное меню, если не хочешь играть

    //TODO: Как сервер поймёт, если кто-то вылетел из udp игры

    //TODO: Перед стартом игры нужно ставить NotifPanel, NotifPanel, Shield на false. Тк игрок мог некорректно завершить прошлую игру, и в ной он начнёт с включёной этой хуйнёй

    //TODO: Если сервер упал и заново включлся и ты нажимаешь мультиплеер, то приходится нажимать два раза

    //TODO: Учксть, чтоб постоянно убирать вышедших из AllLoginClients на сервере. Если человек вышел сразу после игры, это обработается?

    //TODO: Тут куча инфы про разные типы роутеров, и что где-то моё udp соединение может не срабоать - https://gamedev.ru/code/forum/?id=231916

    //TODO: Модно одновременно прислать S и Cancel, тогданадо как-то это обрабоать

    //TODO: Не пускать пользователя в игры, пока он в главном меню не пришлёт сообщение для логина

    //TODO: в udp не нужны |, но всё же на приёме нужен трай тк может не полностью сообщение прийти

    //TODO: При обрыве соединения, посылать клиент в отдыльный поток, где он несколько секунд будет висеть в ожидании соединения. Или же 
    // таки сделать общий лист всех игроков, чтоб чекать есть ли уже такой игрок и если есть то делать реконнект и возвращать в игру
    // ЕСли выкинуло во время игры, то шлёшь номер игры с подписью реконект, и тогда сервер чекнет именно в списке нужной игры и вернёт тебя, или же
    // если там пусто, то вы не успели, досвидания

    //TODO: добавить к notif panel езё одну родительску панель, которая перекроет все кнопки, и их не нужно будет ограничиваь кодом

    //TODO: Человек мжет отменить процесс переподключения, но должна появиться кнопка с возможностью заново начать переподключаться

    //TODO: Корректно завершать соединение и поток, когда выходишь из игры, или хочешь заново подключиться или при постоянном вкл и выкл udp

    //TODO: Ещё перед созданием лобби чекнуть, не прислал ли что-то игрок тк мб он хочет выйти из игры и отменить поиск

    //TODO: Отмена игры, если человек не хочет ждать, когда ему подберут противника, когд сервера пустые

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

    //TODO: Похоже в крестиках можно продолжить шевелиться после окончания матча

    //TODO: Что делать, если пользователь меняет моб инет на wifi или ещё что-то подобное

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

    //TODO: При подключении нового устройства, проверять, есть ли этот аккакунт в сети и решать проблему (либо массив подключённых id, либо в таблице bool connected)
    // ЧТоб нельзя было играть по сети с одного id на нескольких устройствах

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

    //TODO: Что делать, если оба прислали код завершения игры, но он не совпал

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

