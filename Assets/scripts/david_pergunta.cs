using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

public class david_pergunta : MonoBehaviour
{
    public bool is_talking = false;
    public GameObject questions_object;
    public TextMeshProUGUI question_text;
    public Button right_answer_text;
    public Button[] options;
    public int question = 0;
    public Animator door1;
    public Animator door2;
    public Camera cam;
    public int points = 0;
    public GameObject david;
    public GameObject canvas;
    public GameObject left;
    public GameObject right;
    public TextMeshProUGUI points_txt; 
    public GameObject canvas_question;
    private float[] positions = {95f, 27.25f, -34f, -96f};
    JArray request_array;

    string url_base =
        "https://oxodeorehirrwdzcvewx.supabase.co/rest/v1/";

    string apiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Im94b2Rlb3JlaGlycndkemN2ZXd4Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3NzQ1NjI1ODEsImV4cCI6MjA5MDEzODU4MX0.qXaHKJD356N71RDh-tygUE79Za-v6zaHOe7NTn2nj30";
    // Start is called before the first frame update
    void Start()
    {
        change_pos();
        StartCoroutine(get_question());
        StartCoroutine(change_text());
        
    }

    // Update is called once per frame
    void Update()
    {
        points_txt.text = "questões certas: " + points;
        if (is_talking)
        {
            questions_object.SetActive(true);
            canvas_question.SetActive(true);
            cam.enabled = true;
            
        }
        else
        {
            canvas_question.SetActive(false);
            cam.enabled = false;
        }

        if(question > 8)
        {
            Destroy(gameObject);
        }

        if(question == 3)
        {
            door1.SetBool("open", true);
        }

        if(question == 6)
        {
            door2.SetBool("open", true);
        }
    }

    void change_pos()
    {
        List<float> temp_pos = new List<float>(positions);
        var indice = Random.Range(0, temp_pos.Count);
        right_answer_text.GetComponent<RectTransform>().anchoredPosition = new Vector2(255, temp_pos[indice]);
        temp_pos.RemoveAt(indice);
        for (int i = 0; i < options.Length; i++)
        {
            indice = Random.Range(0, temp_pos.Count);
            options[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(255, temp_pos[indice]);
            temp_pos.RemoveAt(indice);
        }

    }
    IEnumerator get_question()
    {
        var url = url_base + "exam?grade_id=eq.1&subject_id=eq.1";
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("apikey", apiKey);
        yield return request.SendWebRequest();
        string request_json = request.downloadHandler.text;
        request_array = JArray.Parse(request_json);
        Debug.LogError(request_array[0].ToSafeString());
    }
    IEnumerator change_text()
    {
        var req_question = question + 1;
        var url_options = url_base + "exam_options?subject_id=eq.1&question_num=eq." + req_question;
        UnityWebRequest req_options = UnityWebRequest.Get(url_options);
        req_options.SetRequestHeader("apikey", apiKey);

        yield return req_options.SendWebRequest();

        string options_json = req_options.downloadHandler.text;

        JArray options_array = JArray.Parse(options_json);
        
        question_text.text = request_array[question]["pergunta"].ToString();
        right_answer_text.GetComponentInChildren<TextMeshProUGUI>().text = request_array[question]["resposta"].ToString();
        for (int i = 0; i < options.Length; i++)
        {
            options[i].GetComponentInChildren<TextMeshProUGUI>().text = options_array[i]["texto"].ToString();
        }
        change_pos();
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") && other.gameObject.GetComponent<JoystickPlayerExample>().IsOwner)
        {
            is_talking = true;
            canvas.SetActive(false);

        }
    }

    public void next_question()
    {
        question++;
        StartCoroutine(change_text());
        if(question == 3 || question == 6 || question == 9)
        {
            final_question();
        }
    }

    public void right_answer()
    {
        points++;
    }

    public void final_question()
    {
        questions_object.SetActive(false);

        if(question == 3)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, 30.51f);
            david.transform.position = new Vector3(david.transform.position.x, david.transform.position.y, 30.51f);
            right.transform.position = new Vector3(right.transform.position.x, right.transform.position.y, 43.57f);
            left.transform.position = new Vector3(left.transform.position.x, left.transform.position.y, 43.57f);
        } 

        if(question == 6)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, 52.26f);
            david.transform.position = new Vector3(david.transform.position.x, david.transform.position.y, 52.26f);
            right.transform.position = new Vector3(right.transform.position.x, right.transform.position.y, 65.51f);
            left.transform.position = new Vector3(left.transform.position.x, left.transform.position.y, 65.51f);
        }
        is_talking = false;
        canvas.SetActive(true);
    }
    

}

//q2 30.51 - 43.57

//q3 52.26 - 65.51
