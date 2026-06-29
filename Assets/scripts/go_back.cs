using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class go_back : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void back_to_lobby()
    {
        GameObject.Find("Player(Clone)").GetComponent<JoystickPlayerExample>().ReturnToLobby();
    }
}
