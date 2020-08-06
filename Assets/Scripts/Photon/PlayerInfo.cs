using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfo : MonoBehaviour
{

    public static PlayerInfo PI;
    public GameObject avatar;
    public GameObject myHand;
    public int cardCount;
    public List<string[]> cardsInHand;

    private void OnEnable() {

        if(PlayerInfo.PI == null) {
            PlayerInfo.PI = this;
        }
        else {
            if(PlayerInfo.PI != this)
            {
                Destroy(PlayerInfo.PI.gameObject);
                PlayerInfo.PI = this;
            }
        }

        cardCount = 0;
        cardsInHand = new List<string[]>();

        DontDestroyOnLoad(PlayerInfo.PI.gameObject);
    }

}
