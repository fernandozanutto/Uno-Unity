using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager GS;
    private PhotonView PV;

    public Transform[] spawnPoints;

    private int spawnIndex = 0;

    private int cardIndex = 0;
    public GameObject dropZone;
    public Text debugText;

    private string[][] deck;
    public string[] lastPlayedCard = { "", "" };

    public GameObject card2d;
    public GameObject startGameButton;
    public int force;
    public Image ultimaCartaJogada;

    private bool gameRunning = false;

    public GameObject pickCardButton;
    public GameObject passTurnButton;
    public GameObject pickColorButton;
    
    public Queue<Player> filaDeJogadores;

    private Queue<Action> filaDeFuncoes;

    public TextMeshProUGUI YouWinMessage;
    public GameObject gameOver;
    public GameObject unoButton;

    private void OnEnable() {
        if (GameManager.GS == null) {
            GameManager.GS = this;
        }
    }


    private void Start() {
        PV = GetComponent<PhotonView>();
        filaDeFuncoes = new Queue<Action>();
        filaDeJogadores = new Queue<Player>();

        foreach (Player p in PhotonNetwork.PlayerList) {
            filaDeJogadores.Enqueue(p);
        }

        if (PhotonNetwork.IsMasterClient) {
            string[][] deck = new string[108][];

            string[] cores = { "blue", "yellow", "red", "green" };
            string[] tipo = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "picker", "reverse", "skip" };
            for (int k = 0; k < 2; k++) {
                for (int i = 0; i < cores.Length; i++) {
                    for (int j = 0; j < tipo.Length; j++) {
                        string[] carta = { cores[i], tipo[j] };
                        deck[i * 13 + j + k * 54] = carta;
                    }
                }
            }
            string[] temp = { "wild", "color_changer" };
            deck[52] = temp;
            deck[106] = temp;

            string[] temp2 = { "wild", "pick_four" };
            deck[53] = temp2;
            deck[107] = temp2;

            for (int i = 0; i < 10; i++){
                //shuffle deck
                string[] tempGO;
                for (int j = 0; j < deck.Length; j++)
                {
                    int rnd = UnityEngine.Random.Range(0, deck.Length);
                    tempGO = deck[rnd];
                    deck[rnd] = deck[i];
                    deck[i] = tempGO;
                }
            }

            PV.RPC("RPC_SetDeck", RpcTarget.All, deck);
        } else {
            startGameButton.SetActive(false);
        }

        pickColorButton.SetActive(false);
        pickCardButton.SetActive(false);
        passTurnButton.SetActive(false);
        unoButton.SetActive(false);
    }

    public void StartGame() {
        if (!PhotonNetwork.IsMasterClient) return;
        if (gameRunning) return;

        startGameButton.SetActive(false);

        PV.RPC("RPC_StartGame", RpcTarget.All);
        
        int aux = 0;
        while(deck[aux][0].Equals("wild") || !int.TryParse(deck[aux][1], out _)) {
            aux++;
        }

        string[] card = deck[aux];

        if (aux != cardIndex) {
            deck[aux] = deck[cardIndex];
            deck[cardIndex] = card;
        }

        PV.RPC("RPC_IncrementCardIndex", RpcTarget.All);

        float torqueY = 50 + UnityEngine.Random.Range(-40f, 40f);
        Vector3 torque = new Vector3(0, torqueY, 0);

        PlayCard(card, dropZone.transform.position, new Vector3(0,0,0), torque);

        GiveCards();
    }

    [PunRPC]
    private void RPC_StartGame() {
        gameRunning = true;
    }

    private void GiveCards() {
        if (!PhotonNetwork.IsMasterClient) return;

        Player[] players = PhotonNetwork.PlayerList;

        for (int i = 0; i < players.Length; i++) {
            for (int j = 0; j < 7; j++) {
                PV.RPC("RPC_SetPlayerCard", RpcTarget.All, deck[cardIndex], players[i]);
                PV.RPC("RPC_IncrementCardIndex", RpcTarget.All);
            }
        }
    }

    public void PreProcessTurn() {
        while(filaDeFuncoes.Count > 0) {
            Action action = filaDeFuncoes.Dequeue();
            action();
        }
    }

    public void WinGame(Player p) {
        PV.RPC("RPC_GameOver", RpcTarget.All, p);
    }

    [PunRPC]
    private void RPC_GameOver(Player p) {
        YouWinMessage.gameObject.SetActive(true);
        YouWinMessage.text = p.NickName + " ganhou!";
        gameRunning = false;
        gameOver.SetActive(true);
    }

    public void OnExitButtonClick() {
        PhotonNetwork.LeaveRoom();
    }

    public bool PlayCard(string[] card, Vector3 position, Vector3 direction, Vector3 torque) {
        if (!gameRunning) return false;

        if(lastPlayedCard.Length > 1) {
            Debug.Log("minha carta: " + card[0] + " " + card[1]);
            Debug.Log("ultima carta: " + lastPlayedCard[0] + " " + lastPlayedCard[1]);
        }

        if(direction != new Vector3(0, 0, 0) && PV.IsMine) {
            PlayerInfo.PI.cardCount--;
        }
        
        if (lastPlayedCard.Length == 0 || lastPlayedCard[0] == "wild" || card[0].Equals(lastPlayedCard[0]) || card[1].Equals(lastPlayedCard[1]) || card[0].Equals("wild")) {
            PV.RPC("RPC_LastPlayedCard", RpcTarget.All, card);

            GameObject newCard3d = PhotonNetwork.Instantiate(
                Path.Combine("PhotonPrefabs", "Carta3d"),
                position,
                Quaternion.Euler(90f, 0f, 0f), 0, card);

            newCard3d.GetComponent<Rigidbody>().AddTorque(torque);
            newCard3d.GetComponent<Rigidbody>().AddForce(direction * force);

            if(card[1] == "skip") {
                NextPlayer();
            } 
            else if (card[1] == "picker"){
                NextPlayer();
                PickCard(filaDeJogadores.Peek());
                PickCard(filaDeJogadores.Peek());
            }
            else if(card[1] == "reverse") {
                NextPlayer();
                ReverseOrder();
            } 
            else if(card[1] == "pick_four") {
                WaitPickColor(filaDeJogadores.Peek());

                filaDeFuncoes.Enqueue(NextPlayer);
                filaDeFuncoes.Enqueue(PickCard);
                filaDeFuncoes.Enqueue(PickCard);
                filaDeFuncoes.Enqueue(PickCard);
                filaDeFuncoes.Enqueue(PickCard);
                filaDeFuncoes.Enqueue(NextPlayer);

                return true;
            }
            else if (card[1] == "color_changer") {
                WaitPickColor(filaDeJogadores.Peek());
                filaDeFuncoes.Enqueue(NextPlayer);

                return true;
            }

            NextPlayer();

            return true;
        }
        else {
            return false;
        }
    }

    private void WaitPickColor(Player player) {
        if(player == PhotonNetwork.LocalPlayer) {
            pickColorButton.SetActive(true);
        }
    }

    public void PickColor(string color) {
        lastPlayedCard[0] = color;
        PV.RPC("RPC_LastPlayedCard", RpcTarget.All, lastPlayedCard);
        pickColorButton.SetActive(false);
        PreProcessTurn();
    }

    public void PickCard() {
        if (PV.IsMine) {
            PV.RPC("RPC_SetPlayerCard", RpcTarget.All, deck[cardIndex], filaDeJogadores.Peek());
            PV.RPC("RPC_IncrementCardIndex", RpcTarget.All);
            passTurnButton.SetActive(true);
        }
    }

    public void PickCard(Player p) {
        PV.RPC("RPC_SetPlayerCard", RpcTarget.All, deck[cardIndex], p);
        PV.RPC("RPC_IncrementCardIndex", RpcTarget.All);
    }

    public void ReverseOrder() {
        PV.RPC("RPC_ReverseOrder", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_ReverseOrder() {
        Stack<Player> pilha = new Stack<Player>();
        while (filaDeJogadores.Count > 0) {
            pilha.Push(filaDeJogadores.Dequeue());
        }

        while (pilha.Count > 0) {
            filaDeJogadores.Enqueue(pilha.Pop());
        }
    }

    public void NextPlayer()
    {
        PV.RPC("RPC_NextPlayer", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_NextPlayer() {
        Player p = filaDeJogadores.Dequeue();
        filaDeJogadores.Enqueue(p);

        if(p == PhotonNetwork.LocalPlayer) {
            // if i was the last player
            pickCardButton.SetActive(false);
            passTurnButton.SetActive(false);
            unoButton.SetActive(false);
        }

        if(filaDeJogadores.Peek() == PhotonNetwork.LocalPlayer) {
            // if i am the current player
            pickCardButton.SetActive(true);
            passTurnButton.SetActive(false);
            unoButton.SetActive(false);

            if (PlayerInfo.PI.cardCount == 2) {
                string topColor = lastPlayedCard[0];
                string topType = lastPlayedCard[1];

                foreach (string[] card in PlayerInfo.PI.cardsInHand) {
                    if(topColor == card[0] || topType == card[1]) {
                        unoButton.SetActive(true);
                        break;
                    }
                }
            }
        }
    }

    public void OnUnoClick() {
        PV.RPC("Uno", RpcTarget.All);
    }

    [PunRPC]
    private void Uno() {
        //TODO: aparecer tela de que alguem gritou uno
    }

    public Transform NextSpawn() {
        Transform spawn = spawnPoints[spawnIndex++];
        Debug.Log("spawn: " + spawn);
        PV.RPC("RPC_SpawnPoint", RpcTarget.All, spawnIndex);

        return spawn;
    }

    [PunRPC]
    private void RPC_SpawnPoint(int v)
    {
        spawnIndex = v;
    }

    [PunRPC]
    private void RPC_SetDeck(string[][] cards)
    {
        Debug.Log("setando deck: " + cards.Length);
        deck = cards;
    }
    
    [PunRPC]
    private void RPC_IncrementCardIndex()
    {
        cardIndex++;
    }

    [PunRPC]
    private void RPC_LastPlayedCard(string[] card)
    {
        lastPlayedCard = card;
        ultimaCartaJogada.sprite = Resources.Load<Sprite>("Sprites/" + card[0] + "_" + card[1] + "_large");
    }

    [PunRPC]
    private void RPC_SetPlayerCard(string[] card, Player player)
    {
        if (player == PhotonNetwork.LocalPlayer) {
            PlayerInfo.PI.cardCount++;

            PlayerInfo.PI.cardsInHand.Add(card);

            string spriteName = "Sprites/" + card[0] + "_" + card[1] + "_large";

            GameObject newCard = Instantiate(card2d, GameObject.Find("My Hand").transform);

            newCard.GetComponent<Image>().sprite = Resources.Load<Sprite>(spriteName);
            Card2dScript newCardInfo = newCard.GetComponent<Card2dScript>();
            newCardInfo.cor = card[0];
            newCardInfo.tipo = card[1];
        }
    }
}
