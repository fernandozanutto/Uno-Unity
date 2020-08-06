using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiplayerSettings : MonoBehaviour
{

    public static MultiplayerSettings settings;

    public bool delayStart = true;
    public int maxPlayers;
    public int minPlayers;

    public int menuScene;
    public int multiPlayerScene;

    private void Awake()
    {
        if(MultiplayerSettings.settings == null)
        {
            MultiplayerSettings.settings = this;
        } else
        {
            if(MultiplayerSettings.settings != this)
            {
                Destroy(this.gameObject);
            }
        }


        DontDestroyOnLoad(this.gameObject);
    }


}
