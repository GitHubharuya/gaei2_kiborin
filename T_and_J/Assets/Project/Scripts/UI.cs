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

            if (!GameManager.instance.allCollected)
            {
                if (GameManager.instance.gameMode == 0)
                {
                    finishUIText.color = new Color(0.5198904f, 1f, 0.5137255f, 1.0f);
                    finishUIText.text = "The Mouse was CAUGHT";
                }
                else if (GameManager.instance.gameMode == 1)
                {
                    finishUIText.color = new Color(1f, 0.4705006f, 0.2122642f, 1.0f);
                    finishUIText.text = "You CAUGHT The Mouse!!";
                }
                else if (GameManager.instance.gameMode == 2)
                {
                    finishUIText.color = new Color(0.6721697f, 0.514151f, 1.0f, 1.0f);
                    finishUIText.text = "You were CAUGHT by Cat...";
                }
            }

            else
            {
                if (GameManager.instance.gameMode == 0)
                {
                    finishUIText.color = new Color(0.5198904f, 1f, 0.5137255f, 1.0f);
                    finishUIText.text = "All Cheeses were COLLECTED";
                }
                else if (GameManager.instance.gameMode == 1)
                {
                    finishUIText.color = new Color(0.6721697f, 0.514151f, 1.0f, 1.0f);
                    finishUIText.text = "All Cheeses were COLLECTED...";
                }
                else if (GameManager.instance.gameMode == 2)
                {
                    finishUIText.color = new Color(1f, 0.6098194f, 0.5137255f, 1.0f);
                    finishUIText.text = "You COLLECTED all Cheeses!!";
                }
            }

            finishUI.SetActive(true);
        }
    }
}
