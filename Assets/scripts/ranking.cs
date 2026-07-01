using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

public class ranking : MonoBehaviour
{
    string url_base =
        "https://oxodeorehirrwdzcvewx.supabase.co/rest/v1/";

    string apiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Im94b2Rlb3JlaGlycndkemN2ZXd4Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3NzQ1NjI1ODEsImV4cCI6MjA5MDEzODU4MX0.qXaHKJD356N71RDh-tygUE79Za-v6zaHOe7NTn2nj30";

    public List<string> ranking_array = new List<string>();

    private JArray request_array;

    public string txt;
    
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(get_ranking());
    }

    public void refresh_ranking()
    {
        txt = "";
        StartCoroutine(get_ranking());
    }

    IEnumerator get_ranking()
    {
        var url = url_base + "profiles?role=eq.student&select=full_name,high_score&order=high_score.desc&limit=10";
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("apikey", apiKey);
        yield return request.SendWebRequest();
        string request_json = request.downloadHandler.text;
        request_array = JArray.Parse(request_json);
        foreach (var item in request_array)
        {
            txt += item["full_name"].ToString() + " - " + item["high_score"].ToString() + "pts\n";
            Debug.Log(txt);
        }
        gameObject.GetComponent<TextMeshPro>().text = txt;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
