using Photon.Pun;
using UnityEngine;


public class Card2dScript : MonoBehaviour
{
    public string cor;
    public string tipo;

    public void OnClick()
    {
        if (GameManager.GS.filaDeJogadores.Peek() != PhotonNetwork.LocalPlayer) {
            return;
        }

        string[] data = { cor, tipo };
        Camera camera = PlayerInfo.PI.avatar.GetComponent<Camera>();

        for (int i = 0; i < PlayerInfo.PI.myHand.transform.childCount; i++) {
            GameObject check = PlayerInfo.PI.myHand.transform.GetChild(i).gameObject;

            if(check == this.gameObject) {
                PlayerInfo.PI.cardsInHand.RemoveAt(i);
                break;
            }
        }

        if(GameManager.GS.PlayCard(data, camera.transform.position, camera.transform.forward, new Vector3(0, 0, 0))) {
            Destroy(this.gameObject);
            
            Debug.Log("num de cartas: " + PlayerInfo.PI.cardCount);

            if (PlayerInfo.PI.cardCount == 0) {
                GameManager.GS.WinGame(PhotonNetwork.LocalPlayer);
            }
        }
    }
}
