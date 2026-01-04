using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class operation : MonoBehaviour
{
    public TextMeshProUGUI digit1;
    public TextMeshProUGUI digit2;
    public TextMeshProUGUI display;
    public int result;
    // Start is called before the first frame update
    void Start()
    {
        display = GameObject.Find("display").GetComponent<TextMeshProUGUI>();
        int num1 = Random.Range(0,10);
        int num2;
        digit1.text = num1.ToString();
        if(gameObject.name.Contains("2"))
        {
            num2 = Random.Range(0,num1 + 1);
            result = num1 - num2;
        }
        else
        {
            num2 = Random.Range(0,10);
            result = num1 + num2;
        }
        digit2.text = num2.ToString();

        Debug.Log(result);
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(display.text);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.tag == "destroy")
        {
            Destroy(gameObject);
        }
    }

    public void compare()
    {
        Debug.Log("entrou compare");
        Debug.Log(display.text);
        Debug.Log(result);
        if(int.Parse(display.text) == result)
        {
            Debug.Log("acertou");
            GameObject.Find("timer").GetComponent<timer>().score++;
            Destroy(gameObject);
        }
    }
}
