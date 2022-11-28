using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TestNetworkManager : NetworkManager
{
    public GameManager GameManager;
    public override void Awake()
    {
        base.Awake();
        GameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }
    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        base.OnServerAddPlayer(conn);
        //GameManager.DrawCard();
        //GameManager.FormatCards();
    }

    public void GameManager_Init()
    {

    }
}
