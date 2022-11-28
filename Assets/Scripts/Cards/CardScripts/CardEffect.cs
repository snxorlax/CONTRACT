using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.VFX;

public class CardEffect : ScriptableObject
{
    public GameManager gameManager;
    public PlayerManager player, enemy;
    public GameObject enemyField, playerField, enemyAvatar, self;
    public effect currentEffect;
    public string gameMessage;
    public List<ContractInfo> contractInfo = new List<ContractInfo>();
    [System.Serializable]
    public struct ContractInfo
    {
        public Transform zone;
        public int amount;
    }

    public enum effect{
        Contract, Play, Destroy, TurnStart, Attack, Shroud, ActivatedEffect_1, ActivatedEffect_2, ActivatedEffect_3, Deathwalk
    }


    public void CardSetup()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        player = gameManager.playerManager;
        enemyField = GameObject.Find("EnemyField");
        playerField = GameObject.Find("PlayerField");
    }
    public virtual void Contract(){

        currentEffect = effect.Contract;
        player.currentContract.Add(self);
        playerField = GameObject.Find("PlayerField");
    }
    public virtual void ContractEffect(){
        player.PlayCard(self, false);
    }
    //effect activated when card is played
    public virtual void Play(){
        currentEffect = effect.Play;
        enemyAvatar = GameObject.Find("EnemyAvatarZone").transform.GetChild(0).gameObject;
        foreach (PlayerManager p in gameManager.players)
        {
            if (p != player)
            {
                enemy = p;
            }
        }
    }
    public virtual void Create(){

        enemyAvatar = GameObject.Find("EnemyAvatarZone").transform.GetChild(0).gameObject;
    }
    public virtual void PlayEffect(GameObject target)
    {
        player.gameText.GetComponent<TextMeshProUGUI>().enabled = false;
        player.currentEffect.Clear();
    }
    //effect activated when starts an attack
    public virtual void Attack(){}
    //effect activated when card is destroyed
    public virtual void Destroy(){}
    public virtual void TurnStart(){

        // currentEffect = effect.TurnStart;
    }
    public virtual void TurnStartEffect(GameObject target){
        // player.currentEffect.Clear();
    }
    //effect activated when exits shroud
    public virtual void Shroud(){
        if (self.GetComponent<CardDisplay>().card.shroud)
        {
            self.GetComponent<CardDisplay>().card.shroud = false;
            gameManager.DeactivateShroud(self);
        }
    }
    public virtual void Continuous(){}
    public virtual void Deathwalk()
    {
        player.ExileCard(self);
    }
    public virtual void DeathwalkEffect(GameObject target){}
    public virtual void ActivatedEffect_1(){
        currentEffect = effect.ActivatedEffect_1;
        player.currentEffect.Add(self);
    }
    public virtual void Effect1(GameObject target){
        player.currentEffect.Clear();
    }
    public virtual void ActivatedEffect_2(){}
    public virtual void Effect2(){}
    public virtual void ActivatedEffect_3(){}
    public virtual void Effect3(){}
    
}
