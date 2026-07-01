using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using System.Text;

public class timer : MonoBehaviour
{
    public TextMeshProUGUI time_text;
    bool trigger = true;
    public int time = 20;

    public bool stop = false;

    public int score = 0;

    public GameObject death;

    public GameObject death_canvas, won_canvas;

    public TextMeshProUGUI score_txt;

    public GameObject end_game_mat;
    private bool dead,won = false;

    

    string url_base =
        "https://oxodeorehirrwdzcvewx.supabase.co/rest/v1/";

    string apiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Im94b2Rlb3JlaGlycndkemN2ZXd4Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3NzQ1NjI1ODEsImV4cCI6MjA5MDEzODU4MX0.qXaHKJD356N71RDh-tygUE79Za-v6zaHOe7NTn2nj30";
    


    Coroutine countdownRoutine;
    // Start is called before the first frame update
    void Start()
    {
        countdownRoutine = StartCoroutine(countdown());
    }

    // Update is called once per frame
    void Update()
    { 
        switch (SceneManager.GetActiveScene().name)
        {
            
            case "mg_mat":
                Screen.orientation = ScreenOrientation.Portrait;
                time_text.text = time.ToString();
                if(time <= 0)
                {
                    stop = true;
                    GameObject[] operacoes = GameObject.FindGameObjectsWithTag("operation");
                    foreach (GameObject obj in operacoes)
                    {
                        Destroy(obj);
                    }

                    end_game_mat.SetActive(true);
                    score_txt.text = "Sua pontuação é: " + score;

                    Debug.Log("pontuação: " + score);
                }
                break;

            case "mg_geo":
                if(dead || won)
                {
                    if(won)
                    {
                        won_canvas.SetActive(true);
                    }
                    time = 0;    
                }
                time_text.text = time.ToString();
                if(time <= 5 && time != 0)
                {
                    time_text.color = Color.red;
                }
                if(time <= 0 && trigger)
                {
                    death.SetActive(true);
                    trigger = false;
                    time_text.color = Color.black;
                    string objective = GameObject.Find("quest txt").GetComponent<quest>().objective;
                    DestroyAllChildren(GameObject.Find("estados").GetComponent<Transform>(), objective);
                    Invoke(nameof(wait_for_reset), 3f);
                }
                if(time > 0)
                {
                    death.SetActive(false);
                    trigger = true;
                }
                break;
        }
    }

    void DestroyAllChildren(Transform parent, string match_name)
    {
        string compare;
        foreach (Transform child in parent)
        {
            if(match_name.Length > 2)
            {
                compare = child.name;
            }
            else
            {
                compare = child.name[..2];
            }
            if(compare == match_name)
            {
                child.parent.gameObject.SetActive(true);
                child.gameObject.SetActive(true);   
            }
            else
            {
                child.gameObject.SetActive(false);
            }
            DestroyAllChildren(child, match_name);
        }
    }

    void reset_obj(Transform parent)
    {
        
        foreach (Transform child in parent)
        {
            child.gameObject.SetActive(true);
            reset_obj(child);
        }
    }

    IEnumerator countdown()
    {
        while (time > 0)
        {
            yield return new WaitForSeconds(1);
            time--;
        }
    }

    void wait_for_reset()
    {

        if (countdownRoutine != null)
            StopCoroutine(countdownRoutine);


        time = 20;
        if(!dead)
        {
            score ++;
        }
        countdownRoutine = StartCoroutine(countdown());
        Transform estados = GameObject.Find("estados").transform;
        reset_obj(estados);
        var quest = GameObject.Find("quest txt").GetComponent<quest>();
        quest.index += 1;
        //Debug.Log(quest.objective_list.Length + " : " + quest.index);
        if(quest.objective_list.Length == quest.index)
        {
            won = true;
            Debug.Log("ganhou");
        }
    }

    public void show_death()
    {
        death_canvas.SetActive(true);
        dead = true;
    }

    public void score_button()
    {
        StartCoroutine(set_score(GameObject.Find("NetworkManager").GetComponent<test_lobby>().playerName));
    }
    public struct EnviarPontosPorNomeData
    {
        public string nome_completo;
        public float pontos_a_adicionar;
    }

    IEnumerator set_score(string player)
    {
        EnviarPontosPorNomeData dados = new EnviarPontosPorNomeData { 
            nome_completo = player, 
            pontos_a_adicionar = score 
        };
        var url = url_base + "rpc/adicionar_pontuacao_por_nome";
        string jsonDados = JsonUtility.ToJson(dados);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonDados);

        // 2. Configura a requisição POST
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            // Headers obrigatórios
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("apikey", apiKey);
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);

            // 3. Envia e aguarda
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Sucesso! +5 pontos adicionados para o estudante: {player}");
            }
            else
            {
                Debug.LogError("Erro ao adicionar pontos por nome: " + request.error);
                Debug.LogError("Detalhes: " + request.downloadHandler.text);
            }
        }
    }
}
