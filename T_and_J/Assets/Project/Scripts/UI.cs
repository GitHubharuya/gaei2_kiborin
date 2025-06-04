using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UI : MonoBehaviour
{
    [SerializeField]
    GameObject cheese;
    [SerializeField]
    GameObject timer;

    private TextMeshProUGUI timerText;
    private TextMeshProUGUI cheeseText;

    // Start is called before the first frame update
    void Start()
    {
        timerText = timer.GetComponent<TextMeshProUGUI>();
        cheeseText = cheese.GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        //タイマー
        timerText.text = "Time : " + (Mathf.Floor(GameManager.instance.timer * 100) / 100).ToString();
        //チーズ
        cheeseText.text = "Cheese : " + GameManager.instance.cheeseCount.ToString();
    }
}
