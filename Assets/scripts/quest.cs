using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class quest : MonoBehaviour
{
    public int index = 0;
    public string [] quest_content;
    public string [] objective_list;
    public string objective;
    private bool got_data = false;
    private string url_base = "https://oxodeorehirrwdzcvewx.supabase.co/rest/v1/";
    private string api_key = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Im94b2Rlb3JlaGlycndkemN2ZXd4Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3NzQ1NjI1ODEsImV4cCI6MjA5MDEzODU4MX0.qXaHKJD356N71RDh-tygUE79Za-v6zaHOe7NTn2nj30";

    // Start is called before the first frame update
    void Start()
    {
        quest_content = new string[5];
        quest_content = new string[5];
        StartCoroutine(get_question());
    }

    // Update is called once per frame
    void Update()
    {
        if(got_data)
        {
            if(index > 5)
            {
                index = 5;
            }
            gameObject.GetComponent<TextMeshProUGUI>().text = quest_content[index];
            objective = objective_list[index];
        }
    }

    IEnumerator get_question()
    {
        var url = url_base + "questions?grade_id=eq."+ 1 +"&subject_id=eq.3&group_id=eq."+ 1;
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("apikey", api_key);
        yield return request.SendWebRequest();
        string request_json = request.downloadHandler.text;
        JArray request_array = JArray.Parse(request_json);
        for(int i = 0; i < 5; i++)
        {
            quest_content[i] = request_array[i]["pergunta"].ToString();
            objective_list[i] = request_array[i]["resposta"].ToString();
        }

        got_data = true;
    }
}
