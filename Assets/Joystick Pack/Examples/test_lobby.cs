using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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

    private Lobby HostLobby;
    private float HeartBeatTimer;

    private float lobbyUpdateTimer;

    private string playerName;
    public GameObject canvas;


    // Start is called before the first frame update
    private async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("logou: " + AuthenticationService.Instance.PlayerId);
        };

        playerName = "Tony" + UnityEngine.Random.Range(10, 99);
        Debug.Log(playerName);
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
            SceneManager.LoadScene(0);
            NetworkManager.Singleton.Shutdown();
        }
    }

    public async void CreateLobby()
    {
        try
        {
            string lobby_name = "lobby1";
            int max_player = 4;
            await relay_manager.Instance.EnsureSignedInAsync();
            string joincode = await relay_manager.Instance.CreateRelay();
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject>
                {
                   {
                        "JoinCode", new DataObject(DataObject.VisibilityOptions.Public, joincode)
                   }
                }
            };
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobby_name, max_player, createLobbyOptions);
            HostLobby = lobby;
            PrintPlayers(HostLobby);
            Debug.Log("lobby criado: " + lobby_name + " " + lobby.MaxPlayers);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    void Update()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            LobbyHeartBeat();
        }
        LobbyUpdates();
    }

    

    private async void LobbyHeartBeat()
    {
        if (HostLobby != null)
        {
            HeartBeatTimer -= Time.deltaTime;
            if (HeartBeatTimer < 0f)
            {
                float HeartBeatTimerMax = 15;
                HeartBeatTimer = HeartBeatTimerMax;

                await LobbyService.Instance.SendHeartbeatPingAsync(HostLobby.Id);
            }
        }
    }

    public async void LobbyList()
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                Count = 25,
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT),
                    //new QueryFilter(QueryFilter.FieldOptions.S1, "escola1", QueryFilter.OpOptions.EQ)
                },
                Order = new List<QueryOrder>
                {
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
                }
            };

            QueryResponse response = await Lobbies.Instance.QueryLobbiesAsync(queryLobbiesOptions);
            Debug.Log("lobies achados: " + response.Results.Count);
            foreach (Lobby lobby in response.Results)
            {
                Debug.Log(lobby.Name + " " + lobby.MaxPlayers + " (" + lobby.Data["JoinCode"].Value + ")");
                PrintPlayers(lobby);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void JoinLobby()
{
    try
    {
        // 1. Buscar lobbies disponíveis
        QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();

        // 2. Tentar entrar no primeiro lobby da lista
        JoinLobbyByIdOptions joinLobbyByIdOptions = new JoinLobbyByIdOptions
        {
            Player = GetPlayer()
        };

        Lobby lobby = await Lobbies.Instance.JoinLobbyByIdAsync(
            queryResponse.Results[0].Id,
            joinLobbyByIdOptions
        );

        // 3. Pegar o joinCode que o host armazenou no Lobby
        string joincode = lobby.Data["JoinCode"].Value;

        // 4. Entrar no Relay com esse joinCode
        var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joincode);

        // 5. Configurar o transporte do NetworkManager para usar o Relay
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));

        // 6. Agora sim, iniciar o cliente
        NetworkManager.Singleton.StartClient();

        // 7. Guardar referência do lobby localmente
        HostLobby = lobby;
        PrintPlayers(lobby);
    }
    catch (LobbyServiceException e)
    {
        Debug.LogError(e);
    }
}


    public async void QuickJoinLobby()
    {
        try
        {

            Lobby lobby = await LobbyService.Instance.QuickJoinLobbyAsync();
            PrintPlayers(lobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private void PrintPlayers(Lobby lobby)
    {
        Debug.Log("jogadores no lobby \"" + lobby.Id + "\": " + lobby.Players.Count + " (" + lobby.Data["JoinCode"].Value + ")");
        foreach (Player player in lobby.Players)
        {
            Debug.Log(player.Id + " " + player.Data["PlayerName"].Value);
        }
    }

    private Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
                    {
                        {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member,playerName)}
                    }
        };
    }

    private async void LobbyUpdates()
    {
        if (HostLobby != null)
        {
            lobbyUpdateTimer -= Time.deltaTime;
            if (lobbyUpdateTimer < 0f)
            {
                float lobbyUpdateTimerMax = 1.1f;
                lobbyUpdateTimer = lobbyUpdateTimerMax;

                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(HostLobby.Id);
                HostLobby = lobby;
            }
        }
    }

    public async void leaveLobby()
    {
        try
        {
            if (NetworkManager.Singleton.IsServer)
            {
                HostLobby = null;
                foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
                {
                    Debug.Log("id do cliente:" + client.ClientId);
                    NetworkManager.Singleton.DisconnectClient(client.ClientId);
                }
                NetworkManager.Singleton.Shutdown();
                SceneManager.LoadScene(0);
            }

            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsClient)
            {
                string lobbyId = HostLobby.Id; // salva antes
                await LobbyService.Instance.RemovePlayerAsync(lobbyId, AuthenticationService.Instance.PlayerId);

                HostLobby = null; // agora sim, pode limpar

                NetworkManager.Singleton.Shutdown();
                //Destroy(canvas);
                //Destroy(NetworkManager.Singleton.gameObject);
                SceneManager.LoadScene(0);
            }
                
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }

        /*if (NetworkManager.Singleton != null)
            {
                AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(0);
            asyncLoad.completed += (op) =>
                NetworkManager.Singleton.Shutdown();
            {
            };
            } necessita testar*/

    }

    public async void KickPlayer()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(HostLobby.Id, HostLobby.Players[1].Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }

    }

    public async void migrateHost()
    {
        try
        {
            HostLobby = await Lobbies.Instance.UpdateLobbyAsync(HostLobby.Id, new UpdateLobbyOptions
            {
                HostId = HostLobby.Players[1].Id
            });
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void deleteLobby()
    {
        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(HostLobby.Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    // Update is called once per frame
}
