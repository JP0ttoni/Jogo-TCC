using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Relay;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;

public class test_lobby : MonoBehaviour
{
    [SerializeField] private GameObject loading_canvas;
    private Lobby HostLobby;
    private float HeartBeatTimer;
    private float lobbyUpdateTimer;

    private string playerName;
    public GameObject canvas;

    private async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Logado como: " + AuthenticationService.Instance.PlayerId);
        };

        playerName = "Tony" + UnityEngine.Random.Range(10, 99);
        Debug.Log("Nome do player: " + playerName);

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private void Awake()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log("Cliente desconectado, retornando ao menu...");
            CleanNetworkAndReturnToMenu();
        }
    }

    private async Task EnsureNotInLobbyAsync()
    {
        try
        {
            if (HostLobby != null)
            {
                Debug.Log("Saindo do lobby anterior...");
                await Lobbies.Instance.RemovePlayerAsync(HostLobby.Id, AuthenticationService.Instance.PlayerId);
                HostLobby = null;
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogWarning("Nenhum lobby anterior ou erro ignorável: " + e);
        }
    }

    public async void CreateLobby()
    {
        loading_canvas.SetActive(true);
        try
        {
            await EnsureNotInLobbyAsync();
            await relay_manager.Instance.EnsureSignedInAsync();

            string joincode = await relay_manager.Instance.CreateRelay();

            var createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject>
                {
                    { "JoinCode", new DataObject(DataObject.VisibilityOptions.Public, joincode) }
                }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync("lobby1", 4, createLobbyOptions);
            HostLobby = lobby;

            PrintPlayers(HostLobby);
            Debug.Log($"Lobby criado com sucesso: {lobby.Name}");
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Erro ao criar lobby: " + e);
        }
        finally
        {
            loading_canvas.SetActive(false);
        }
    }

    public async void JoinLobby()
    {
        loading_canvas.SetActive(true);
        try
        {
            await EnsureNotInLobbyAsync();
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();

            if (queryResponse.Results.Count == 0)
            {
                Debug.LogWarning("Nenhum lobby encontrado!");
                loading_canvas.SetActive(false);
                return;
            }

            var joinLobbyByIdOptions = new JoinLobbyByIdOptions
            {
                Player = GetPlayer()
            };

            Lobby lobby = await Lobbies.Instance.JoinLobbyByIdAsync(queryResponse.Results[0].Id, joinLobbyByIdOptions);
            string joincode = lobby.Data["JoinCode"].Value;

            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joincode);
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));

            NetworkManager.Singleton.StartClient();
            HostLobby = lobby;

            PrintPlayers(lobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Erro ao entrar no lobby: " + e);
        }
        finally
        {
            loading_canvas.SetActive(false);
        }
    }

    private void Update()
    {
        if (NetworkManager.Singleton.IsServer)
            LobbyHeartBeat();

        LobbyUpdates();
    }

    private async void LobbyHeartBeat()
    {
        if (HostLobby == null) return;

        HeartBeatTimer -= Time.deltaTime;
        if (HeartBeatTimer <= 0f)
        {
            HeartBeatTimer = 15f;
            await LobbyService.Instance.SendHeartbeatPingAsync(HostLobby.Id);
        }
    }

    private async void LobbyUpdates()
    {
        if (HostLobby == null) return;

        lobbyUpdateTimer -= Time.deltaTime;
        if (lobbyUpdateTimer <= 0f)
        {
            lobbyUpdateTimer = 1.1f;
            HostLobby = await LobbyService.Instance.GetLobbyAsync(HostLobby.Id);
        }
    }

    public async void LeaveLobby()
    {
        try
        {
            if (HostLobby != null)
            {
                Debug.Log("Removendo jogador do lobby...");
                await Lobbies.Instance.RemovePlayerAsync(HostLobby.Id, AuthenticationService.Instance.PlayerId);
                HostLobby = null;
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogWarning("Erro ao sair do lobby: " + e);
        }

        CleanNetworkAndReturnToMenu();
    }

    private async void CleanNetworkAndReturnToMenu()
    {
        try
    {
        if (NetworkManager.Singleton != null)
        {
            // Se ainda estiver escutando, desligue de forma segura
            if (NetworkManager.Singleton.IsListening)
            {
                try
                {
                    NetworkManager.Singleton.Shutdown();
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning("Erro ao dar Shutdown no NetworkManager: " + e);
                }
            }

            // Dá um pequeno delay para callbacks terminarem
            await Task.Delay(150);

            // Pegamos o GameObject que contém o NetworkManager e destruímos
            var nmGO = NetworkManager.Singleton.gameObject;
            // Força destruição imediata para evitar que ele persista entre cenas
            if (nmGO != null)
            {
                DestroyImmediate(nmGO);
            }

            // Outro pequeno delay pra garantir cleanup do Unity
            await Task.Delay(50);
        }
    }
    catch (System.Exception e)
    {
        Debug.LogWarning("Erro ao limpar NetworkManager: " + e);
    }

    // Agora voltar para o menu (criação de novo NetworkManager na cena de menu deve ocorrer normalmente)
    SceneManager.LoadScene(0);
    }

    private Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) }
            }
        };
    }

    private void PrintPlayers(Lobby lobby)
    {
        Debug.Log($"Jogadores no lobby {lobby.Name}: {lobby.Players.Count}");
        foreach (var player in lobby.Players)
        {
            if (player.Data.ContainsKey("PlayerName"))
                Debug.Log($"→ {player.Id} : {player.Data["PlayerName"].Value}");
        }
    }
}
