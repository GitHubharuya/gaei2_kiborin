using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InputField : MonoBehaviour
{
    //オブジェクトと結びつける
    TMP_InputField inputField;

    void Start()
    {
        //Componentを扱えるようにする
        inputField = GetComponent<TMP_InputField>();

    }

    public void Input(string st)
    {
        if (st == "MapSize")
        {
            GameManager.instance.mapSize = int.Parse(inputField.text);
        }
        if (st == "Seed")
        {
            GameManager.instance.seed = int.Parse(inputField.text);
        }
    }

}