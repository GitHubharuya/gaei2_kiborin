using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Title : MonoBehaviour
{
    private bool isPressed = false;

    [SerializeField]
    GameObject buttons;
    [SerializeField]
    GameObject inputFields;

    // Start is called before the first frame update
    void Start()
    {
        buttons.SetActive(true);
        inputFields.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void pressStart()
    {
        if (!isPressed)
        {
            isPressed = true;
            Debug.Log("start!");
            //シーン遷移
            SceneManager.LoadScene("SampleScene");
        }
    }

    public void pressMode(int gameMode)
    {
        GameManager.instance.gameMode = gameMode;
        buttons.SetActive(false);
        inputFields.SetActive(true);
    }

    public void pressGoBack()
    {
        buttons.SetActive(true);
        inputFields.SetActive(false);
    }
}
