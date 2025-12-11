using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class david_pergunta : MonoBehaviour
{
    public bool is_talking = false;
    public GameObject[] questions_object;
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
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        points_txt.text = "questões certas: " + points;
        if (is_talking)
        {
            questions_object[question].SetActive(true);
            cam.enabled = true;
            
        }
        else
        {
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

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            is_talking = true;
            canvas.SetActive(false);
        }
    }

    public void next_question()
    {
        questions_object[question].SetActive(false);
        question++;
    }

    public void right_answer()
    {
        points++;
    }

    public void final_question()
    {
        questions_object[question].SetActive(false);

        if(question == 2)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, 30.51f);
            david.transform.position = new Vector3(david.transform.position.x, david.transform.position.y, 30.51f);
            right.transform.position = new Vector3(right.transform.position.x, right.transform.position.y, 43.57f);
            left.transform.position = new Vector3(left.transform.position.x, left.transform.position.y, 43.57f);
        } 

        if(question == 5)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, 52.26f);
            david.transform.position = new Vector3(david.transform.position.x, david.transform.position.y, 52.26f);
            right.transform.position = new Vector3(right.transform.position.x, right.transform.position.y, 65.51f);
            left.transform.position = new Vector3(left.transform.position.x, left.transform.position.y, 65.51f);
        }
        question++;
        is_talking = false;
        canvas.SetActive(true);
    }
    

}

//q2 30.51 - 43.57

//q3 52.26 - 65.51
