using System.Collections;
using System.Collections.Generic;
using TMPro;
using TMPro.Examples;
using UnityEngine;

public class floating_text : MonoBehaviour
{
    [Header("Configurações")] // Arraste o seu Player aqui no Inspetor
    private Quaternion rotacaoInicial;
    
    public string txt = ""; // Distância/altura do texto em relação ao player

    void Start()
    {
        rotacaoInicial = transform.rotation;
        txt = GameObject.Find("NetworkManager").GetComponent<test_lobby>().playerName;
        gameObject.GetComponent<TextMeshPro>().text = txt;
    }

    void LateUpdate()
    {
        transform.rotation = rotacaoInicial;
    }
}
