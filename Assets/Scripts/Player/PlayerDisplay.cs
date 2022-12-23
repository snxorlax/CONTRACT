using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class PlayerDisplay : MonoBehaviour
{
    public PlayerInfo playerInfo;
    public TextMeshProUGUI lifeTotal;
    private void OnEnable()
    {
        // SetPlayerProperties();
    }
    public void SetPlayerProperties()
    {
        lifeTotal = transform.Find("LifeTotal").Find("Text").Find("LifeTotal").gameObject.GetComponent<TextMeshProUGUI>();

        lifeTotal.text = playerInfo.lifeTotal.ToString();
    }
}
