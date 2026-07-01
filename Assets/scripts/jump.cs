using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    GameObject player;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player(Clone)");   
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void jumping()
    {
        player.GetComponent<JoystickPlayerExample>().Jump();
    }
}
