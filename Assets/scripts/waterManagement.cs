using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class waterManagement : MonoBehaviour
{
    public static waterManagement instance;

    public float amplitude = 1f;
    public float length = 2f;
    public float speed = 1f;
    public float offset = 0f;

    private void Awake() 
    {
        if(instance == null)
        {
            instance = this;
        }else if(instance != this)
        {
            Debug.Log("instance already exists");
            Destroy(this);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        offset += Time.deltaTime * speed;
    }

    public float getwaveheight(float x)
    {
        return  amplitude * Mathf.Sin(x / length + offset);
    }
}
