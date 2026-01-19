using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class quest : MonoBehaviour
{
    public int index = 0;
    public string [] quest_content;
    public string [] objective_list;
    public string objective;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.GetComponent<TextMeshProUGUI>().text = quest_content[index];
        objective = objective_list[index];
        //Debug.Log(objective);
    }
}
