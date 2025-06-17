using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InputGenerator : MonoBehaviour
{

    //オブジェクトと結びつける
    public TMP_InputField inputField;

    void Start()
    {
        //Componentを扱えるようにする
        inputField = GetComponent<TMP_InputField>();

    }

    public void Input()
    {
        GameManager.instance.mapSize = int.Parse(inputField.text);

    }

}