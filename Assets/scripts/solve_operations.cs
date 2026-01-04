using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class solve_operations : MonoBehaviour
{
     public TextMeshProUGUI display;

    // Chamado pelos botões
    public void AdicionarNumero(string num)
    {
        if(display.text.Length <= 8)
        {   
            display.text += num;
        }
    }
    public void Limpar()
    {
        display.text = "";
    }

    public void enter()
    {
        GameObject[] operacoes = GameObject.FindGameObjectsWithTag("operation");
        foreach (GameObject obj in operacoes)
        {
            operation op = obj.GetComponent<operation>();

            if (op != null)
            {
                op.compare();
            }
        }

        Limpar();
    }
}
