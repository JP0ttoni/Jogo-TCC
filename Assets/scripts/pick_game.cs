using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(launch());
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
                    text_button.text = right[Random.Range(0,right.Length)];
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
