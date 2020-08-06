using Photon.Pun;
using System.IO;
using UnityEngine;

public class PhotonPlayer : MonoBehaviour
{
    private PhotonView PV;
    public GameObject myAvatar;
    
    void Start()
    {
        PV = GetComponent<PhotonView>();

        if (PV.IsMine)
        {
            Transform spawn = GameManager.GS.NextSpawn();

            myAvatar = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerAvatar"),
                spawn.position, spawn.rotation, 0);

            Camera camera = myAvatar.GetComponent<Camera>();

            camera.enabled = true;
            camera.gameObject.transform.LookAt(new Vector3(0,0,0));
            Vector3 rota = camera.gameObject.transform.rotation.eulerAngles;
            rota.x = 15f;

            camera.gameObject.transform.rotation = Quaternion.Euler(rota);

            PlayerInfo.PI.avatar = myAvatar;
        }
    }
}
