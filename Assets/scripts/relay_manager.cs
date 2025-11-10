using System;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class relay_manager : MonoBehaviour
{
    public static relay_manager Instance;

    [SerializeField] Button hostButton;
    [SerializeField] Button joinButton;
    [SerializeField] TMP_InputField joinInput;
    [SerializeField] TextMeshProUGUI codeText;
    [SerializeField] TextMeshProUGUI text1;
    public GameObject canvas;

    private static bool s_signInInProgress = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private async void Start()
    {
        text1.enabled = false;
        codeText.enabled = false;

        await UnityServices.InitializeAsync();
        await EnsureSignedInAsync();

        hostButton.onClick.AddListener(() => { _ = CreateRelay(); });
        joinButton.onClick.AddListener(() => joinRelay(joinInput.text));
    }

    public async Task EnsureSignedInAsync()
    {
        if (AuthenticationService.Instance.IsSignedIn)
            return;

        if (s_signInInProgress)
        {
            int waited = 0;
            const int timeoutMs = 5000;
            const int step = 100;
            while (!AuthenticationService.Instance.IsSignedIn && waited < timeoutMs)
            {
                await Task.Delay(step);
                waited += step;
            }
            return;
        }

        s_signInInProgress = true;
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Assinado anonimamente: " + AuthenticationService.Instance.PlayerId);
        }
        catch (Exception ex)
        {
            Debug.LogWarning("Falha no sign-in: " + ex);
        }
        finally
        {
            s_signInInProgress = false;
        }
    }

    public async Task<string> CreateRelay()
    {
        await EnsureSignedInAsync();
        string joinCode = "";

        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);
            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            var relayServerData = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartServer();
            NetworkManager.Singleton.SceneManager.LoadScene("lobby_start", LoadSceneMode.Single);

            codeText.enabled = true;
            text1.enabled = true;
            codeText.text = $"Code: {joinCode}";

            Debug.Log($"Relay criado! JoinCode: {joinCode}");
        }
        catch (Exception e)
        {
            Debug.LogError("Erro ao criar Relay: " + e);
        }

        return joinCode;
    }

    public async void joinRelay(string joinCode)
    {
        await EnsureSignedInAsync();

        try
        {
            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            var relayServerData = new RelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartClient();
        }
        catch (Exception e)
        {
            Debug.LogError("Erro ao entrar no Relay: " + e);
        }
    }

    private void Update()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost)
        {
            int playerCount = NetworkManager.Singleton.ConnectedClientsList.Count;
            text1.text = $"Jogadores conectados: {playerCount}";
        }
    }
}
