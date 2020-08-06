using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PhotonRoom : MonoBehaviourPunCallbacks, IInRoomCallbacks
{
    public static PhotonRoom room;
    private PhotonView PV;

    public bool isGameLoaded;
    public int currentScene;

    private Player[] photonPlayers;

    public int playersInRoom;
    public int myNumberInRoom;

    public int playersInGame;

    public Text feedbackText;
    public GameObject startGameButton;
    public GameObject leaveRoomButton;

    public InputField NickName;

    private void Awake()
    {
        if(PhotonRoom.room == null)
        {
            PhotonRoom.room = this;
        } 
        else
        {
            if(PhotonRoom.room != this)
            {
                Destroy(PhotonRoom.room.gameObject);
                PhotonRoom.room = this;
            }
        }

        startGameButton.SetActive(false);
        DontDestroyOnLoad(this.gameObject);
    }


    public override void OnEnable()
    {
        base.OnEnable();
        PhotonNetwork.AddCallbackTarget(this);
        SceneManager.sceneLoaded += OnSceneFinishedLoading;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        PhotonNetwork.RemoveCallbackTarget(this);
        SceneManager.sceneLoaded -= OnSceneFinishedLoading;
    }

    void Start()
    {
        PV = GetComponent<PhotonView>();
    }

    void Update() {
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        leaveRoomButton.SetActive(true);

        photonPlayers = PhotonNetwork.PlayerList;
        playersInRoom = photonPlayers.Length;
        myNumberInRoom = playersInRoom;
        PhotonNetwork.NickName = NickName.text;

        Debug.Log("Players in room: " + playersInRoom);
        feedbackText.text += "\nEntrou na sala: " + PhotonNetwork.CurrentRoom.Name;
        feedbackText.text += "\n" + playersInRoom + " jogadores na sala";

        if (playersInRoom >= MultiplayerSettings.settings.minPlayers)
        {
            if (PhotonNetwork.IsMasterClient) startGameButton.SetActive(true);
        }

        if(playersInRoom == MultiplayerSettings.settings.maxPlayers)
        {
            if (!PhotonNetwork.IsMasterClient) return;
            PhotonNetwork.CurrentRoom.IsOpen = false;
        }
    }
    
    
    public override void OnConnectedToMaster() {
        Debug.Log("connected to master");
        Debug.Log(isGameLoaded);
        if (isGameLoaded) {
            Destroy(GameObject.Find("PlayerInfo"));
            Destroy(GameObject.Find("RoomController"));
            Debug.Log("just left room");
            SceneManager.LoadScene(MultiplayerSettings.settings.menuScene);
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer){
        base.OnPlayerLeftRoom(otherPlayer);

        playersInRoom--;
        if (!isGameLoaded) {
            feedbackText.text += "\n" + playersInRoom + " jogadores na sala";
        }
        
        if (playersInRoom < MultiplayerSettings.settings.minPlayers)
        {
            if (PhotonNetwork.IsMasterClient) startGameButton.SetActive(false);
        }

        if(playersInRoom < MultiplayerSettings.settings.maxPlayers)
        {
            if (!PhotonNetwork.IsMasterClient) return;
            PhotonNetwork.CurrentRoom.IsOpen = true;
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        
        photonPlayers = PhotonNetwork.PlayerList;
        
        playersInRoom++;

        feedbackText.text += "\n" + playersInRoom + " jogadores na sala";

        if(playersInRoom >= MultiplayerSettings.settings.minPlayers)
        {
            if(PhotonNetwork.IsMasterClient) startGameButton.SetActive(true);
        }
        if(playersInRoom == MultiplayerSettings.settings.maxPlayers)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }

            PhotonNetwork.CurrentRoom.IsOpen = false;
        }
    }

    public void StartGame()
    {
        isGameLoaded = true;
        if (!PhotonNetwork.IsMasterClient) return;

        PhotonNetwork.CurrentRoom.IsOpen = false;

        PhotonNetwork.LoadLevel(MultiplayerSettings.settings.multiPlayerScene);
    }


    void OnSceneFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        currentScene = scene.buildIndex;
        if(currentScene == MultiplayerSettings.settings.multiPlayerScene)
        {
            isGameLoaded = true;

            PV.RPC("RPC_LoadedGameScene", RpcTarget.MasterClient);
        }
    }

    [PunRPC]
    private void RPC_LoadedGameScene()
    {
        playersInGame++;
        if(playersInGame == PhotonNetwork.PlayerList.Length)
        {
            PV.RPC("RPC_CreatePlayer", RpcTarget.All);
        }
    }

    [PunRPC]
    private void RPC_CreatePlayer()
    {
        GameObject player = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PhotonNetworkPlayer"), transform.position, Quaternion.identity, 0);

        
    }
}
