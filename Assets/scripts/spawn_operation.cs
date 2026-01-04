using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;

public class spawn_operation : MonoBehaviour
{
    public GameObject[] operation;
    public Transform[] spawn;

    public int i = 0;

    void Start()
    {
        spawn_obj1();
        spawn_obj2();
        spawn_obj3(); 
    }


    IEnumerator cd_1()
    {
        yield return new WaitForSeconds(Random.Range(4f, 5f));
        spawn_obj1();
    }


    void spawn_obj1()
    {
        if(!GameObject.Find("timer").GetComponent<timer>().stop)
        {    
            int choose = Random.Range(0, operation.Length);

            GameObject clone = Instantiate(operation[choose], spawn[0]);   

            RectTransform rt = clone.GetComponent<RectTransform>();

            rt.anchoredPosition = Vector2.zero; // centro do canvas
            rt.localScale = Vector3.one;

            StartCoroutine(cd_1());
        }
    }
    IEnumerator cd_2()
    {
        yield return new WaitForSeconds(Random.Range(5f, 6f));
        spawn_obj2();
    }

    void spawn_obj2()
    {
        if(!GameObject.Find("timer").GetComponent<timer>().stop)
        {    
            int choose = Random.Range(0, operation.Length);

            GameObject clone = Instantiate(operation[choose], spawn[1]);   

            RectTransform rt = clone.GetComponent<RectTransform>();

            rt.anchoredPosition = Vector2.zero; // centro do canvas
            rt.localScale = Vector3.one;
            StartCoroutine(cd_2());
        }
    }

    IEnumerator cd_3()
    {
        yield return new WaitForSeconds(Random.Range(6f, 7f));
        spawn_obj3();
    }

    void spawn_obj3()
    {
        if(!GameObject.Find("timer").GetComponent<timer>().stop)
        {    
            int choose = Random.Range(0, operation.Length);

            GameObject clone = Instantiate(operation[choose], spawn[2]);   

            RectTransform rt = clone.GetComponent<RectTransform>();

            rt.anchoredPosition = Vector2.zero; // centro do canvas
            rt.localScale = Vector3.one;
            StartCoroutine(cd_3());
        }
    }
}

