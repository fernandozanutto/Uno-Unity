using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card3dScript : MonoBehaviour
{
    public string cor;
    public string tipo;

    void Start()
    {
        string[] dados = (string[]) GetComponent<PhotonView>().InstantiationData;
        cor = dados[0];
        tipo = dados[1];

        MeshRenderer mesh = GetComponent<MeshRenderer>();

        string materialName = "Materials/" + cor + "_" + tipo;
        mesh.material = Resources.Load<Material>(materialName);
    }

    public override string ToString()
    {
        return cor + " " + tipo;
    }
}
