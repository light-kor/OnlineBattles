using System.Collections.Generic;
using UnityEngine;

public class Game_onPhone_2 : MonoBehaviour
{
    private GameResources_2 GR;

    private void Start()
    {
        GR = GameResources_2.GameResources;
    }

    private void CheckEndOfGame()
    {
        int myPoints = DataHolder.MyScore;
        int enemyPoints = DataHolder.EnemyScore;

        if (myPoints >= GR.WinScore || enemyPoints >= GR.WinScore)
        {
            string notifText = null;

            if (myPoints > enemyPoints)
                notifText = "Синий победил";
            else if (enemyPoints > myPoints)
                notifText = "Красный победил";

            new Notification(notifText, Notification.ButtonTypes.MenuButton);
        }
    }
}