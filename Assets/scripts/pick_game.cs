using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

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
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(launch());
        StartCoroutine(set_image());
    }

    // Update is called once per frame
    void Update()
    {
        if(rigth_count >= 25)
        {
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
                    text_button.text = right[rand_num];
                    if(right[rand_num] == "VIDRO")
                    {                        
                        newButton.GetComponent<Button>().image.sprite = bottle_sprt;
                    }
                    else
                    {
                        newButton.GetComponent<Button>().image.sprite = glass_sprt;
                    }
                    rigth_count++;
                }
                else
                {
                    text_button.text = wrong[Random.Range(0,wrong.Length)];
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

    IEnumerator set_image()
    {
        string url = "";
        
        url = "https://i.pinimg.com/736x/79/ac/28/79ac288e8245ec913d3270848ad155b1.jpg";
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);

        yield return request.SendWebRequest();

        Texture2D texture = DownloadHandlerTexture.GetContent(request);

        bottle_sprt = Sprite.Create(
            texture,
            new Rect(0,0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f)
        );

        url = "https://img.freepik.com/vetores-gratis/estilhacos-realistas-de-vidro-quebrado-transparente_1284-9417.jpg?semt=ais_hybrid&w=740&q=80";
        UnityWebRequest request2 = UnityWebRequestTexture.GetTexture(url);

        yield return request2.SendWebRequest();

        Texture2D texture2 = DownloadHandlerTexture.GetContent(request2);

        glass_sprt = Sprite.Create(
            texture2,
            new Rect(0,0, texture2.width, texture2.height),
            new Vector2(0.5f, 0.5f)
        );
        
    }

    void end_game()
    {
        play = false;
        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("Clone"))
            {
                return;
            }
        }
        if(score == rigth_count)
        {
            question.text = "você acertou todos os objetos!!";
        }
        else
        {
            question.text = "faltou: " + (rigth_count-score).ToString() + " pontos";
        }
        exit.SetActive(true);
    }
}
