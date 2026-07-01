using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using System.Text;

public class write : MonoBehaviour
{
    public int word = 0;
    public int letter = 0;
    public GameObject father, end;
    private string answer = "vogal";
    public string current_answer = "";
    public Sprite outline, plain;
    public TextMeshProUGUI tip;
    public TextMeshProUGUI answer_txt;

    string url_base =
        "https://oxodeorehirrwdzcvewx.supabase.co/rest/v1/";

    string apiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Im94b2Rlb3JlaGlycndkemN2ZXd4Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3NzQ1NjI1ODEsImV4cCI6MjA5MDEzODU4MX0.qXaHKJD356N71RDh-tygUE79Za-v6zaHOe7NTn2nj30";
    
    
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(getinfo());
        Screen.orientation = ScreenOrientation.Portrait;
        father = GameObject.Find("row" + word);
    }

    IEnumerator getinfo()
    {
        var url = url_base + "questions?grade_id=eq.1&subject_id=eq.4";
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("apikey", apiKey);
        yield return request.SendWebRequest();
        string request_json = request.downloadHandler.text;
        var request_array = JArray.Parse(request_json);
        var rand = Random.Range(0, request_array.Count);
        Debug.Log(rand);
        tip.text = request_array[rand]["pergunta"].ToString();
        answer = request_array[rand]["resposta"].ToString();
        answer_txt.text = answer;
    }

    // Update is called once per frame
    void Update()
    {
    }

    void atualizar()
    {
        father = GameObject.Find("row" + word);
        foreach(Transform child in father.transform)
        {
            if(child.name.Contains("bl"))
            {
                var img = child.GetComponent<Image>();
                if (img == null)
                {
                  Debug.Log(child.name + " não tem imagem");  
                  continue;
                } 
                img.sprite = outline;
                img.color = Color.gray;
            }
            if(child.name == "bl" + letter)
            {
                child.GetComponent<Image>().color = Color.white;
            }
        }
        
    }

    void GetAllChildren(Transform parent)
    {
        foreach (Transform child in parent)
        {
            GetAllChildren(child);
        }
    }

    public void write_letter(string current_letter)
    {
        foreach (Transform child in father.transform)
        {
            if(child.name == "letter" + letter)
            {
                child.GetComponent<TextMeshProUGUI>().text = current_letter;
                current_answer += current_letter;
                letter++;
                atualizar();
                return;
            }
            
        }

    }

    public void delete()
    {
        if (letter > 0)
        {
            letter--;
        }
        foreach (Transform child in father.transform)
        {
            if(child.name == "letter"+letter)
            {
                child.GetComponent<TextMeshProUGUI>().text = " ";
                current_answer = current_answer.Remove(current_answer.Length - 1);
                atualizar();
                return;
            }
        }
    }
    public void enter()
    {
        //Handle_Outline
        //Handle_Plain
        foreach (Transform child in father.transform)
        {
            var img = child.GetComponent<Image>();
            if(child.name.Contains("bl"))
            {
                img.sprite = plain;
                img.color = Color.grey;
            }
        }
        if(current_answer.Length < 5)
        {
            return;
        }
        Debug.Log(current_answer + " / " + answer);
        char[] temp_answer = answer.ToCharArray();
        char[] temp_current_answer = current_answer.ToLower().ToCharArray();
        int i = 0,j = 0;
        bool check = false;
        foreach(char c1 in temp_current_answer)
        {
            GameObject.Find(c1.ToString()).GetComponent<Image>().color = new Color32(84,84,84,90);
        }
        foreach(char c in temp_answer)
        {
            foreach(char c1 in temp_current_answer)
            {
                if(c == c1 && i == j)
                {
                    Debug.Log("certo / " + j);
                    foreach (Transform child in father.transform)
                    {
                        var img = child.GetComponent<Image>();
                        if(child.name == "bl" + j)
                        {
                            GameObject.Find(c1.ToString()).GetComponent<Image>().color = new Color32(58,163,148,255);
                            img.color = new Color32(58, 163, 148, 255); //#3AA394
                        }
                    }
                    temp_answer[i] = '0';
                    temp_current_answer[j] = '1';
                    check = true;
                    break;
                }
                j++;
            }
            if(!check)
            {
                j = 0;
                foreach(char c1 in temp_current_answer)
                {
                    if(c == c1 && i != j)
                    {
                        Debug.Log("diferente / " + j);
                        foreach (Transform child in father.transform)
                        {
                            var img = child.GetComponent<Image>();
                            if(child.name == "bl" + j)
                            {
                                GameObject.Find(c1.ToString()).GetComponent<Image>().color = new Color32(211,173,105,255);
                                img.color = new Color32(211, 173, 105, 255);//#D3AD69
                            }
                        }
                    }
                    j++;
                }
            }
            check = false;
            j = 0;
            i++;
        }
        if(current_answer.ToLower() == answer)
        {
            Debug.Log("acertou");
            end.SetActive(true);
            return;
        }
        current_answer = "";
        if(word >= 5)
        {
            foreach (Transform child in end.transform)
            {
                if(child.name == "message")
                {
                    child.GetComponent<TextMeshProUGUI>().text = "Que pena, você não acertou. A palavra era:";
                }
            }
            end.SetActive(true);
            return;
        }
        word++;
        letter = 0;
        atualizar();
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
            pontos_a_adicionar = 30f 
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
