using TMPro;
using Unity.Netcode;
using Unity.Properties;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

public class UI_Manager : MonoBehaviour
{
    [SerializeField]
    private Button startServer;

    [SerializeField]
    private Button startHost;

    [SerializeField]
    private Button startClient;

    [SerializeField]
    private TextMeshProUGUI pig;

    private void Update()
    {

        pig.text = $"jogadores: {PlayersManager.Instance.PlayersInGame}";
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        startHost.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.StartHost())
            {
                Debug.Log("começou host");
            }
            else
            {
                Debug.Log("não começou host");
            }
        });

        startServer.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.StartServer())
            {
                Debug.Log("começou server");
            }
            else
            {
                Debug.Log("não começou server");
            }
        });
        
        startClient.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.StartClient())
            {
                Debug.Log("começou client");
            }
            else
            {
                Debug.Log("não começou client");
            }
        });
    }
}
