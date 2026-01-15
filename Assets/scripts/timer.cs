using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class timer : MonoBehaviour
{
    public TextMeshProUGUI time_text;
    public int time = 25;

    public bool stop = false;

    public int score = 0;
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
                if(time <= 5)
                {
                    time_text.color = Color.red;
                }
                if(time <= 0)
                {
                    time_text.color = Color.white;
                    DestroyAllChildren(GameObject.Find("estados").GetComponent<Transform>(), "RS");
                    Invoke(nameof(wait_for_reset), 3f);
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


        time = 25;

        countdownRoutine = StartCoroutine(countdown());
        Transform estados = GameObject.Find("estados").transform;
        reset_obj(estados);
    }     
}
