using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;
using UnityEngine.UI;

public class write : MonoBehaviour
{
    public int word = 0;
    public int letter = 0;
    public GameObject father, end;
    private string answer = "vogal";
    public string current_answer = "";
    public Sprite outline, plain;
    
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        father = GameObject.Find("row" + word);
        foreach(Transform child in father.transform)
        {
            if(child.name.Contains("bl"))
            {
                var img = child.GetComponent<Image>();
                img.sprite = outline;
                img.color = Color.gray;
            }
            if(child.name == "bl" + letter)
            {
                child.GetComponent<Image>().color = Color.white;
            }
        }
    }

    void GetAllChildren(Transform parent)
    {
        foreach (Transform child in parent)
        {
            GetAllChildren(child);
        }
    }

    public void write_letter(string current_letter)
    {
        foreach (Transform child in father.transform)
        {
            if(child.name == "letter" + letter)
            {
                child.GetComponent<TextMeshProUGUI>().text = current_letter;
                current_answer += current_letter;
                letter++;
                return;
            }
            
        }
    }

    public void delete()
    {
        if (letter > 0)
        {
            letter--;
        }
        foreach (Transform child in father.transform)
        {
            if(child.name == "letter"+letter)
            {
                child.GetComponent<TextMeshProUGUI>().text = " ";
                current_answer = current_answer.Remove(current_answer.Length - 1);
                return;
            }
        }
    }
    public void enter()
    {
        //Handle_Outline
        //Handle_Plain
        foreach (Transform child in father.transform)
        {
            var img = child.GetComponent<Image>();
            if(child.name.Contains("bl"))
            {
                img.sprite = plain;
                img.color = Color.grey;
            }
        }
        if(current_answer.Length < 5)
        {
            return;
        }
        Debug.Log(current_answer + " / " + answer);
        char[] temp_answer = answer.ToCharArray();
        char[] temp_current_answer = current_answer.ToLower().ToCharArray();
        int i = 0,j = 0;
        bool check = false;
        foreach(char c1 in temp_current_answer)
        {
            GameObject.Find(c1.ToString()).GetComponent<Image>().color = new Color32(84,84,84,90);
        }
        foreach(char c in temp_answer)
        {
            foreach(char c1 in temp_current_answer)
            {
                if(c == c1 && i == j)
                {
                    Debug.Log("certo / " + j);
                    foreach (Transform child in father.transform)
                    {
                        var img = child.GetComponent<Image>();
                        if(child.name == "bl" + j)
                        {
                            GameObject.Find(c1.ToString()).GetComponent<Image>().color = new Color32(58,163,148,255);
                            img.color = new Color32(58, 163, 148, 255); //#3AA394
                        }
                    }
                    temp_answer[i] = '0';
                    temp_current_answer[j] = '1';
                    check = true;
                    break;
                }
                j++;
            }
            if(!check)
            {
                j = 0;
                foreach(char c1 in temp_current_answer)
                {
                    if(c == c1 && i != j)
                    {
                        Debug.Log("diferente / " + j);
                        foreach (Transform child in father.transform)
                        {
                            var img = child.GetComponent<Image>();
                            if(child.name == "bl" + j)
                            {
                                GameObject.Find(c1.ToString()).GetComponent<Image>().color = new Color32(211,173,105,255);
                                img.color = new Color32(211, 173, 105, 255);//#D3AD69
                            }
                        }
                    }
                    j++;
                }
            }
            check = false;
            j = 0;
            i++;
        }
        if(current_answer.ToLower() == answer)
        {
            Debug.Log("acertou");
            end.SetActive(true);
            return;
        }
        current_answer = "";
        if(word >= 5)
        {
            foreach (Transform child in end.transform)
            {
                if(child.name == "message")
                {
                    child.GetComponent<TextMeshProUGUI>().text = "Que pena, você não acertou. A palavra era:";
                }
            }
            end.SetActive(true);
            return;
        }
        word++;
        letter = 0;
    }
}
