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

public class pick_game : MonoBehaviour
{
    public string[] wrong;
    public string[] right;

    public Transform[] spawn;
    public float wait_time = 4.2f;
    public GameObject right_button;
    public GameObject wrong_button, exit;
    public int i = 0;

    public TextMeshProUGUI score_text, question;

    public int score = 0;
    public bool play = true;
    public int rigth_count = 0;
    Sprite bottle_sprt;
    Sprite glass_sprt;
    Sprite window_sprt;
    Sprite mirror_sprt;
    Sprite chair_sprt;
    Sprite door_sprt;
    Sprite guitar_sprt;
    Sprite wood_sprt;
    int group_num;

    string url_base =
        "https://oxodeorehirrwdzcvewx.supabase.co/rest/v1/";
    string apiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Im94b2Rlb3JlaGlycndkemN2ZXd4Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3NzQ1NjI1ODEsImV4cCI6MjA5MDEzODU4MX0.qXaHKJD356N71RDh-tygUE79Za-v6zaHOe7NTn2nj30";
    // Start is called before the first frame update
    void Start()
    {
        set_image();
    }

    // Update is called once per frame
    void Update()
    {
        if (score < 0)
        {
            score = 0;
        }
        if(rigth_count >= 24)
        {
            Debug.Log("clica");
            end_game();
        }

        score_text.text = score.ToString();
    }

    IEnumerator launch()
    {
        if(play)
        {
            if(wait_time > 1.7f)
            {
                wait_time -= 0.3f;
            }
            int j;
            int repeat = Random.Range(1,5);
            Debug.Log(repeat);
            for(j = 0; j < repeat; j++)
            {   
                var choose = Random.Range(0, 2) == 0 ? wrong_button : right_button;
                GameObject newButton = Instantiate(choose,spawn[i]);
                var text_button = newButton.GetComponentInChildren<TMP_Text>();
                if(choose == right_button)
                {
                    var rand_num = Random.Range(0,right.Length);
                    text_button.text = "";//right[rand_num];
                    if(right[rand_num] == "VIDRO")
                    {                        
                        newButton.GetComponent<Button>().image.sprite = bottle_sprt;
                        //newButton.GetComponent<Image>().enabled = false;
                    }

                    if(right[rand_num] == "COPO")
                    {
                        newButton.GetComponent<Button>().image.sprite = glass_sprt;
                        //newButton.GetComponent<Image>().enabled = false;
                    }

                    if(right[rand_num] == "JANELA")
                    {
                        newButton.GetComponent<Button>().image.sprite = window_sprt;
                        //newButton.GetComponent<Image>().enabled = false;
                    }

                    if(right[rand_num] == "ESPELHO")
                    {
                        newButton.GetComponent<Button>().image.sprite = mirror_sprt;
                        //newButton.GetComponent<Image>().enabled = false;
                    }
                    rigth_count++;
                }
                else
                {
                    var wrong_num = Random.Range(0,wrong.Length);
                    text_button.text = "";//wrong[wrong_num];

                    if(wrong[wrong_num] == "CADEIRA")
                    {
                        newButton.GetComponent<Button>().image.sprite = chair_sprt;
                    }

                    if(wrong[wrong_num] == "PORTA")
                    {
                        newButton.GetComponent<Button>().image.sprite = door_sprt;
                    }

                    if(wrong[wrong_num] == "VIOLÃO")
                    {
                        newButton.GetComponent<Button>().image.sprite = guitar_sprt;
                    }

                    if(wrong[wrong_num] == "TABUA")
                    {
                        newButton.GetComponent<Button>().image.sprite = wood_sprt;
                    }
                }

                i++;
                if(i >= spawn.Length)
                {
                    i = 0;
                }
            }
            yield return new WaitForSeconds(wait_time);
            StartCoroutine(launch());
        }
    }

    void set_image()
    {
        var net_obj = GameObject.Find("NetworkManager").GetComponent<load_images>();
        bottle_sprt = net_obj.bottle_sprt;
        glass_sprt = net_obj.glass_sprt;
        window_sprt = net_obj.window_sprt;
        mirror_sprt = net_obj.mirror_sprt;
        chair_sprt = net_obj.chair_sprt;
        door_sprt = net_obj.door_sprt;
        guitar_sprt = net_obj.guitar_sprt;
        wood_sprt = net_obj.wood_sprt;
        question.text = net_obj.question;
        StartCoroutine(launch());
    }

    void end_game()
    {
        play = false;
        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("Clone") && !obj.name.Contains("Player"))
            {
                return;
            }
        }
        if(score == rigth_count)
        {
            question.text = "você acertou todos os objetos!!";
            exit.SetActive(true);
        }
        else
        {
            if(score < 0)
            {
                score = 0;
            }
            question.text = "faltou: " + (rigth_count-score).ToString() + " pontos";
            exit.SetActive(true);
        }
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
