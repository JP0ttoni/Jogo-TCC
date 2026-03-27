using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.Networking;
using System.Text;
using UnityEngine;

public class SupabaseManager : MonoBehaviour
{
    public static SupabaseManager Instance;

    [Header("Supabase Settings")]
    public string supabaseUrl;
    public string anonKey;    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
    }
    
    public void InsertPlayer(string username)
    {
        StartCoroutine(InsertPlayerCoroutine(username));
    }
    IEnumerator InsertPlayerCoroutine(string username)
    {
        string url = supabaseUrl + "/rest/v1/players";
        string json = "{\"username\":\""+ username + "\", \"highscore\":0}";

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content=Type", "application/json");
        request.SetRequestHeader("apikey", anonKey);
        request.SetRequestHeader("Authorization", "Bearer" + anonKey);
        request.SetRequestHeader("Prefer", "return=minimal");

        yield return request.SendWebRequest();

        if(request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("jogador inserido com sucesso");
        }
        else
        {
            Debug.LogError("Error: " + request.error);
            Debug.LogError(request.downloadHandler.text);
        }
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
