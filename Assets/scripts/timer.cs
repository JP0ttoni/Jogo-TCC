using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class timer : MonoBehaviour
{
    public TextMeshProUGUI time_text;
    public int time = 25;

    public bool stop = false;

    public int score = 0;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(countdown());
    }

    // Update is called once per frame
    void Update()
    {
        time_text.text = time.ToString();
        if(time <= 0)
        {
            stop = true;
            GameObject[] operacoes = GameObject.FindGameObjectsWithTag("operation");
            foreach (GameObject obj in operacoes)
            {
                Destroy(obj);
            }

            Debug.Log("pontuação: " + score);
        }
    }

    IEnumerator countdown()
    {
        if(time > 0)
        {    
            yield return new WaitForSeconds(1);
            time--;
            StartCoroutine(countdown());
        }
    }     
}
