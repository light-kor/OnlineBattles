using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameResources_2 : MonoBehaviour
{
    public static GameResources_2 GameResources;
    [SerializeField] private Player player1, player2;

    public bool GameOn { get; private set; } = false;

    private void Awake()
    {
        GameResources = this;
    }

    void Start()
    {
        GameOn = true;

    }

    void Update()
    {
        
    }

    public void PauseGame()
    {
        GameOn = false;
        player1.StopTrail();
        player2.StopTrail();
    }
}
