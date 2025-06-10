using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameModeSetting : MonoBehaviour
{
    [SerializeField]
    GameObject catPrefab;
    [SerializeField]
    GameObject mousePrefab;
    [SerializeField]
    GameObject catCamera;
    [SerializeField]
    GameObject mouseCamera;

    // Start is called before the first frame update
    void Start()
    {
        //ネズミの初期位置設定


        switch (GameManager.instance.gameMode)
        {
            case 0:
                catPrefab.GetComponent<Cat>().enabled = true;
                mousePrefab.GetComponent<ObstacleAvoidance>().enabled = true;

                break;
            case 1:
                catPrefab.GetComponent<PlayerController>().enabled = true;
                mousePrefab.GetComponent<ObstacleAvoidance>().enabled = true;
                catCamera.SetActive(true);
                break;
            case 2:
                catPrefab.GetComponent<Cat>().enabled = true;
                mousePrefab.GetComponent<PlayerController>().enabled = true;
                mouseCamera.SetActive(true);
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
