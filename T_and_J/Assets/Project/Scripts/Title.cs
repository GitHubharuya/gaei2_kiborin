using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Title : MonoBehaviour
{
    private bool isPressed = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void pressStart(int gameMode)
    {
        if (!isPressed)
        {
            isPressed = true;
            GameManager.instance.gameMode = gameMode;
            Debug.Log("start!");
            //シーン遷移
            SceneManager.LoadScene("SampleScene");
        }
    }
}
