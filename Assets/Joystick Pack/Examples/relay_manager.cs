using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
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
using JetBrains.Annotations;

public class relay_manager : MonoBehaviour
{
    public static relay_manager Instance;
    [SerializeField] Button hostButton;
    [SerializeField] Button joinButton;
    [SerializeField] TMP_InputField joinInput;
    [SerializeField] TextMeshProUGUI codeText;

    [SerializeField] TextMeshProUGUI text1;
    public GameObject canvas;

    // evita múltiplas tentativas simultâneas de sign-in
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
    async void Start()
    {
        DontDestroyOnLoad(canvas);
        text1.enabled = false;
        codeText.enabled = false;
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in: " + AuthenticationService.Instance.PlayerId);
        };

        // garante que estamos autenticados antes de habilitar os botões
        await EnsureSignedInAsync();

        hostButton.onClick.AddListener(() => { _ = CreateRelay(); });

        // Botão join → precisa passar parâmetro, então usa lambda
        joinButton.onClick.AddListener(() => joinRelay(joinInput.text));
    }

    public async Task EnsureSignedInAsync()
    {
        // já estamos autenticados
        if (AuthenticationService.Instance.IsSignedIn)
            return;

        // outra rotina já está assinando — espera até que termine (timeout seguro)
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
            Debug.Log("SignInAnonymouslyAsync succeeded: " + AuthenticationService.Instance.PlayerId);
        }
        catch (AuthenticationException ex) when (ex.ErrorCode == AuthenticationErrorCodes.ClientInvalidUserState)
        {
            // Já está em sign-in ou já assinado — podemos ignorar
            Debug.Log("Sign-in in progress or already signed in (ignored).");
        }
        catch (Exception ex)
        {
            Debug.LogError("Sign-in failed: " + ex);
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

            var relayServerData = new RelayServerData(allocation, "dtls"); // ou "udp" se tiver problema no device
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Example Scene");
            asyncLoad.completed += (op) =>
                NetworkManager.Singleton.StartServer();//StartHost()
                text1.enabled = true;
                codeText.enabled = true;
                codeText.text = "code:" + joinCode;
            {
            };

        }
        catch (Exception e)
        {
            Debug.LogError("CreateRelay error: " + e);
        }
        return joinCode;

    }

    public async void joinRelay(string joinCode)
    {
        await EnsureSignedInAsync();

        try
        {
            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            var relayServerData = new RelayServerData(joinAllocation, "dtls"); // ou "udp"
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartClient();
        }
        catch (Exception e)
        {
            Debug.LogError("joinRelay error: " + e);
        }
    }

    void Update()
    {
        if (NetworkManager.Singleton.IsHost)
        {    
            int playerCount = NetworkManager.Singleton.ConnectedClientsList.Count;
            text1.text = $"jogadores: {playerCount}";
        }
    }
}
