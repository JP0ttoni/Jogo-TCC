using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.Networking;
using System.Text;
using UnityEngine;

public class SupabaseManager : MonoBehaviour
{
    public static SupabaseManager Instance;

    string url = "https://oxodeorehirrwdzcvewx.supabase.co/rest/v1/players";
    string apiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Im94b2Rlb3JlaGlycndkemN2ZXd4Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3NzQ1NjI1ODEsImV4cCI6MjA5MDEzODU4MX0.qXaHKJD356N71RDh-tygUE79Za-v6zaHOe7NTn2nj30"; 
    void Awake()
    {
        Instance = this;
    }

    private void Start() 
    {
        //InsertPlayer("tonizin", 200);
        GetPlayers();    
    }

    public void GetPlayers()
    {
        StartCoroutine(GetPlayersRequest());
    }

    IEnumerator GetPlayersRequest()
    {
        UnityWebRequest request = UnityWebRequest.Get(url + "?select=*");

        request.SetRequestHeader("apikey", apiKey);
        request.SetRequestHeader("Authorization", "Bearer" + apiKey);

        yield return request.SendWebRequest();

        if(request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("response: " + request.downloadHandler.text);
        }
        else
        {
            Debug.LogError("Error: " + request.error);
        }
    }
    
    public void InsertPlayer(string username, int score)
    {
        Debug.Log("chegou 1");
        StartCoroutine(InsertPlayerCoroutine(username, score));
    }
    IEnumerator InsertPlayerCoroutine(string username, int score)
    {
        Debug.Log("chegou 2");
        string json = "{\"username\":\""+ username + "\", \"highscore\":" + score + "}";

        UnityWebRequest request = new UnityWebRequest(url, "POST");

        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("apikey", apiKey);
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);
        request.SetRequestHeader("Prefer", "return=minimal");

        yield return request.SendWebRequest();

        Debug.Log("Insert Result: " + request.result);
        Debug.Log("Insert Code: " + request.responseCode);
        

        if(request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Insert Error: " + request.downloadHandler.text);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
