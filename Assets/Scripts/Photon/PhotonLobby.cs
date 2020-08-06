using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhotonLobby : MonoBehaviourPunCallbacks { 
    
    public static PhotonLobby lobby;
    public GameObject joinRoom;
    public GameObject leaveRoom;
    public Text feedbackText;
    public InputField inputRoomName;
    public GameObject startGameButton;
    private ClientState currentState;

    private void Awake()
    {
        lobby = this;
    }

    void Start() {
        
        joinRoom.SetActive(false);
        leaveRoom.SetActive(false);
        Debug.Log(PhotonNetwork.NetworkClientState);
        currentState = PhotonNetwork.NetworkClientState;
        Debug.Log("connected: " + PhotonNetwork.IsConnected);
        if (!PhotonNetwork.IsConnected) {
            Debug.Log("conectando....");
            PhotonNetwork.ConnectUsingSettings();
        } else {
            joinRoom.SetActive(true);
        }
    }

    private void Update() {
        if(PhotonNetwork.NetworkClientState != currentState) {
            currentState = PhotonNetwork.NetworkClientState;
            Debug.Log("Novo estado:" + currentState);
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("connected to photon master server " + PhotonNetwork.CloudRegion);
        joinRoom.SetActive(true);
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public void OnJoinRoomButtonClick()
    {
        joinRoom.SetActive(false);
        string roomName = inputRoomName.text;
        PhotonNetwork.JoinRoom(roomName);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        feedbackText.text = "Erro ao entrar na sala ";
        feedbackText.text += "\n" + message;
        joinRoom.SetActive(true);
    }

    public void CreateRoom()
    {
        int randomRoomName = Random.Range(0, 10000);

        RoomOptions roomOps = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = (byte) MultiplayerSettings.settings.maxPlayers };

        PhotonNetwork.CreateRoom(randomRoomName.ToString(), roomOps);
    }

    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        feedbackText.text = "Sala criada: " + PhotonNetwork.CurrentRoom.Name;
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        feedbackText.text = "Falha em criar sala";
        CreateRoom();
    }

    public void OnLeaveButtonClicked()
    {
        joinRoom.SetActive(true);
        leaveRoom.SetActive(false);
        startGameButton.SetActive(false);
        
        PhotonNetwork.LeaveRoom();

        feedbackText.text = "";
    }
}
