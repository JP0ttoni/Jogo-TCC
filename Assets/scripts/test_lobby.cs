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
using TMPro;
using UnityEngine.SceneManagement;
using Unity.Services.Relay;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using UnityEngine.Networking;
using System.Text;

public class test_lobby : MonoBehaviour
{
    [SerializeField] private GameObject loading_canvas;
    private Lobby HostLobby;
    private string supabaseUrl = "https://oxodeorehirrwdzcvewx.supabase.co";
    private string supabaseAnonKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Im94b2Rlb3JlaGlycndkemN2ZXd4Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3NzQ1NjI1ODEsImV4cCI6MjA5MDEzODU4MX0.qXaHKJD356N71RDh-tygUE79Za-v6zaHOe7NTn2nj30";
    private float HeartBeatTimer;
    private float lobbyUpdateTimer;

    public TMP_InputField lobbyCodeInput;
    public TMP_InputField db_user;
    public TMP_InputField db_password;

    public GameObject lobby_obj;

    public GameObject fail_login;

    public GameObject login;

    private string playerName;
    private string db_id;
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

        [System.Serializable]
        public class LoginData
        {
            public string email;
            public string password;
        }
    public void IniciarLogin()
    {
        string email = db_user.text + "@teste.com";
        string senha = db_password.text;
        StartCoroutine(FazerLoginCoroutine(email, senha));
    }

    private IEnumerator FazerLoginCoroutine(string email, string senha)
    {
        // 1. Prepara a URL de autenticação do Supabase
        string url = $"{supabaseUrl}/auth/v1/token?grant_type=password";

        // 2. Cria o objeto com os dados e transforma em JSON
        LoginData dados = new LoginData { email = email, password = senha };
        string jsonDados = JsonUtility.ToJson(dados);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonDados);

        // 3. Configura a requisição web
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            
            // O Supabase exige esses headers para saber quem está chamando a API
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("apikey", supabaseAnonKey);

            // 4. Envia a requisição e aguarda a resposta
            yield return request.SendWebRequest();

            // 5. Trata o resultado
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Login efetuado com sucesso!");
                // A resposta contém o JWT (Access Token), dados do usuário, etc.
                Debug.Log("Resposta do servidor: " + request.downloadHandler.text);
                lobby_obj.SetActive(true);
                login.SetActive(false);
            }
            else
            {
                Debug.LogError("Erro no login: " + request.error);
                Debug.LogError("Detalhes do erro: " + request.downloadHandler.text);
                db_password.text = "";
                db_user.text = "";
                fail_login.SetActive(true);
            }
        }
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

    public async void JoinLobbyByCode()
{
    loading_canvas.SetActive(true);

    try
    {
        await EnsureNotInLobbyAsync();

        var joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
        {
            Player = GetPlayer()
        };

        Lobby lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(
            lobbyCodeInput.text,
            joinLobbyByCodeOptions
        );

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
                { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) },
                {"db_id", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, db_id)}
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
