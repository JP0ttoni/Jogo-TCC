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
        StartCoroutine(cd_1());
        StartCoroutine(cd_2());
        StartCoroutine(cd_3());
    }


    IEnumerator cd_1()
    {
        yield return new WaitForSeconds(Random.Range(3f, 4f));
        spawn_obj();
    }

    IEnumerator cd_2()
    {
        yield return new WaitForSeconds(Random.Range(5f, 6f));
        spawn_obj();
    }

    IEnumerator cd_3()
    {
        yield return new WaitForSeconds(Random.Range(6f, 7f));
        spawn_obj();
    }

    void spawn_obj()
    {
        int choose = Random.Range(0, operation.Length);

        GameObject clone = Instantiate(operation[choose], spawn[i]);
        if (i == 0)
        {
            StartCoroutine(cd_1());
            
        } else if(i == 1)
        {
            StartCoroutine(cd_2());
        }else if(i == 2)
        {
            StartCoroutine(cd_3());
        }
        i++;
        if(i > 2)
        {
            i = 0;    
        }

        RectTransform rt = clone.GetComponent<RectTransform>();

        rt.anchoredPosition = Vector2.zero; // centro do canvas
        rt.localScale = Vector3.one;
    }
}

