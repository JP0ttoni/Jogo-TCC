using DilmerGames.Core.Singletons;
using Unity.Netcode;
using UnityEngine;

public class PlayersManager : Singleton<PlayersManager>
{
    private NetworkVariable<int> playersInGame = new NetworkVariable<int>();

    public int PlayersInGame => playersInGame.Value;

    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    // ✅ Corrigido: precisa ser public override (não protected)
    public override void OnDestroy()
    {
        base.OnDestroy(); // mantém a limpeza do NetworkBehaviour

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    private void OnClientConnected(ulong id)
    {
        if (!IsServer) return;
        if (!IsSpawned || playersInGame == null) return;

        playersInGame.Value++;
    }

    private void OnClientDisconnected(ulong id)
    {
        if (!IsServer) return;
        if (!IsSpawned || playersInGame == null) return;

        playersInGame.Value = Mathf.Max(0, playersInGame.Value - 1);
    }
}
