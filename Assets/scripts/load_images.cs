using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Unity.VisualScripting;
using Newtonsoft.Json.Linq;
using System.Text;

public class load_images : MonoBehaviour
{

    public string  question;
    public Sprite bottle_sprt;
    public Sprite glass_sprt;
    public Sprite window_sprt;
    public Sprite mirror_sprt;
    public Sprite chair_sprt;
    public Sprite door_sprt;
    public Sprite guitar_sprt;
    public Sprite wood_sprt;
    int group_num;
        string url_base =
        "https://oxodeorehirrwdzcvewx.supabase.co/rest/v1/";
    string apiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Im94b2Rlb3JlaGlycndkemN2ZXd4Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3NzQ1NjI1ODEsImV4cCI6MjA5MDEzODU4MX0.qXaHKJD356N71RDh-tygUE79Za-v6zaHOe7NTn2nj30";

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(set_image());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator set_image()
    {
        var url = url_base + "subjects?id=eq.2";
        UnityWebRequest image_url = UnityWebRequest.Get(url);
        image_url.SetRequestHeader("apikey", apiKey);
        yield return image_url.SendWebRequest();
        string image_json = image_url.downloadHandler.text;
        JArray image_array = JArray.Parse(image_json);
        int group_range = int.Parse(image_array[0]["question_num"].ToString());
        group_num = Random.Range(1, group_range + 1);

        url = url_base + "questions?group_id=eq." + group_num + "&grade_id=eq.1&subject_id=eq.2";
        image_url = UnityWebRequest.Get(url);
        image_url.SetRequestHeader("apikey", apiKey);

        yield return image_url.SendWebRequest();

        image_json = image_url.downloadHandler.text;

        image_array = JArray.Parse(image_json);
        
        question = image_array[0]["pergunta"].ToString();

        string request_url = image_array[0]["resposta"].ToString();
    
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(request_url);

        yield return request.SendWebRequest();

        Texture2D texture = DownloadHandlerTexture.GetContent(request);

        bottle_sprt = Sprite.Create(
            texture,
            new Rect(0,0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f)
        );

        request_url = image_array[1]["resposta"].ToString();
        UnityWebRequest request2 = UnityWebRequestTexture.GetTexture(request_url);

        yield return request2.SendWebRequest();

        Texture2D texture2 = DownloadHandlerTexture.GetContent(request2);

        glass_sprt = Sprite.Create(
            texture2,
            new Rect(0,0, texture2.width, texture2.height),
            new Vector2(0.5f, 0.5f)
        );

        request_url = image_array[2]["resposta"].ToString();
        UnityWebRequest request3 = UnityWebRequestTexture.GetTexture(request_url);

        yield return request3.SendWebRequest();

        Texture2D texture3 = DownloadHandlerTexture.GetContent(request3);

        window_sprt = Sprite.Create(
            texture3,
            new Rect(0,0, texture3.width, texture3.height),
            new Vector2(0.5f, 0.5f)
        );

        request_url = image_array[3]["resposta"].ToString();
        UnityWebRequest request4 = UnityWebRequestTexture.GetTexture(request_url);

        yield return request4.SendWebRequest();

        Texture2D texture4 = DownloadHandlerTexture.GetContent(request4);

        mirror_sprt = Sprite.Create(
            texture4,
            new Rect(0,0, texture4.width, texture4.height),
            new Vector2(0.5f, 0.5f)
        );

        //errrados

        url = url_base + "question_options?group_id=eq." + 1 + "&question_id=eq.2&grade_id=eq.1";
        image_url = UnityWebRequest.Get(url);
        image_url.SetRequestHeader("apikey", apiKey);
        yield return image_url.SendWebRequest();
        image_json = image_url.downloadHandler.text;
        image_array = JArray.Parse(image_json);
        request_url = image_array[0]["texto"].ToString();
        UnityWebRequest request_op = UnityWebRequestTexture.GetTexture(request_url);

        yield return request_op.SendWebRequest();

        Texture2D texture_op = DownloadHandlerTexture.GetContent(request_op);

        chair_sprt = Sprite.Create(
            texture_op,
            new Rect(0,0, texture_op.width, texture_op.height),
            new Vector2(0.5f, 0.5f)
        );

        request_url = image_array[1]["texto"].ToString();
        UnityWebRequest request2_op = UnityWebRequestTexture.GetTexture(request_url);

        yield return request2_op.SendWebRequest();

        Texture2D texture2_op = DownloadHandlerTexture.GetContent(request2_op);

        door_sprt = Sprite.Create(
            texture2_op,
            new Rect(0,0, texture2_op.width, texture2_op.height),
            new Vector2(0.5f, 0.5f)
        );

        request_url = image_array[2]["texto"].ToString();
        UnityWebRequest request3_op = UnityWebRequestTexture.GetTexture(request_url);

        yield return request3_op.SendWebRequest();

        Texture2D texture3_op = DownloadHandlerTexture.GetContent(request3_op);

        guitar_sprt = Sprite.Create(
            texture3_op,
            new Rect(0,0, texture3_op.width, texture3_op.height),
            new Vector2(0.5f, 0.5f)
        );

        request_url = image_array[3]["texto"].ToString();
        UnityWebRequest request4_op = UnityWebRequestTexture.GetTexture(request_url);

        yield return request4_op.SendWebRequest();

        Texture2D texture4_op = DownloadHandlerTexture.GetContent(request4_op);

        wood_sprt = Sprite.Create(
            texture4_op,
            new Rect(0,0, texture4_op.width, texture4_op.height),
            new Vector2(0.5f, 0.5f)
        );
    }
}
