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
    [SerializeField]
    GameObject gameUI;
    [SerializeField]
    GameObject finishUI;

    private TextMeshProUGUI timerText;
    private TextMeshProUGUI cheeseText;
    private TextMeshProUGUI finishUIText;

    // Start is called before the first frame update
    void Start()
    {
        timerText = timer.GetComponent<TextMeshProUGUI>();
        cheeseText = cheese.GetComponent<TextMeshProUGUI>();
        finishUIText = finishUI.GetComponent<TextMeshProUGUI>();
        finishUI.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        //タイマー
        timerText.text = "Time : " + (Mathf.Floor(GameManager.instance.timer * 100) / 100).ToString();
        //チーズ
        cheeseText.text = "Cheese : " + GameManager.instance.cheeseCount.ToString();

        if (GameManager.instance.isFinished)
        {
            gameUI.SetActive(false);
            if (GameManager.instance.gameMode == 0)
            {
                finishUIText.text = "The Mouse was CAUGHT";
            }
            else if (GameManager.instance.gameMode == 1)
            {
                finishUIText.text = "You CAUGHT The Mouse!!";
            }
            else if (GameManager.instance.gameMode == 2)
            {
                finishUIText.text = "You were CAUGHT by Cat...";
            }
            finishUI.SetActive(true);
        }
    }
}
