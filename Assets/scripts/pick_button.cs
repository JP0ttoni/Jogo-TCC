using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pick_button : MonoBehaviour
{
    public float minForceX = -3f;
    public float maxForceX = 3f;
    public float minForceY = 10f;
    public float maxForceY = 14f;

    private Rigidbody2D rb;

    void Start()
    {
        StartCoroutine(destroyMe());
        rb = GetComponent<Rigidbody2D>();

        float randomX = Random.Range(minForceX, maxForceX);
        float randomY = Random.Range(minForceY, maxForceY);

        rb.velocity = new Vector2(randomX, randomY);
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    public void DestroyMeRight()
    {
        GameObject.Find("Canvas").GetComponent<pick_game>().score++;
        Destroy(gameObject);
    }

    public void DestroyMeWrong()
    {
        GameObject.Find("Canvas").GetComponent<pick_game>().score--;
        Destroy(gameObject);
    }

    IEnumerator destroyMe()
    {
        yield return new WaitForSeconds(6f);
        Destroy(gameObject);
    }
}
