using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class portrait : MonoBehaviour
{
    public GameObject main_canvas;
    public GameObject portrait_cam;
    public GameObject portrait_canvas;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            main_canvas.SetActive(false);
            portrait_cam.SetActive(true);
            portrait_canvas.SetActive(true);
        }
    }

    public void exit()
    {
        portrait_cam.SetActive(false);
        portrait_canvas.SetActive(false);  
        main_canvas.SetActive(true);
    }
}


