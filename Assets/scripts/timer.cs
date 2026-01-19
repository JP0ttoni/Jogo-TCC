using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class timer : MonoBehaviour
{
    public TextMeshProUGUI time_text;
    bool trigger = true;
    public int time = 20;

    public bool stop = false;

    public int score = 0;

    public GameObject death;


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
                break;

            case "mg_geo":
                time_text.text = time.ToString();
                if(time <= 5 && time != 0)
                {
                    time_text.color = Color.red;
                }
                if(time <= 0 && trigger)
                {
                    death.SetActive(true);
                    trigger = false;
                    time_text.color = Color.white;
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

        countdownRoutine = StartCoroutine(countdown());
        Transform estados = GameObject.Find("estados").transform;
        reset_obj(estados);
        GameObject.Find("quest txt").GetComponent<quest>().index += 1;
    }     
}
