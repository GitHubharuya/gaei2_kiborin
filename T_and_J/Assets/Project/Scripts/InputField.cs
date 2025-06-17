using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InputField : MonoBehaviour
{
    //�I�u�W�F�N�g�ƌ��т���
    TMP_InputField inputField;

    void Start()
    {
        //Component��������悤�ɂ���
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