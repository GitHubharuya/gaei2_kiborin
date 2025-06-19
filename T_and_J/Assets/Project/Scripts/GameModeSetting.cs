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
    [SerializeField]
    GameObject catMouse;

    // Start is called before the first frame update
    void Start()
    {
        switch (GameManager.instance.gameMode)
        {
            case 0:
                catPrefab.GetComponent<Cat>().enabled = true;
                mousePrefab.GetComponent<ObstacleAvoidance>().enabled = true;
                mousePrefab.GetComponent<CapsuleCollider>().enabled = false;
                catPrefab.GetComponent<CapsuleCollider>().enabled = false;
                break;
            case 1:
                catPrefab.GetComponent<PlayerController>().enabled = true;
                mousePrefab.GetComponent<ObstacleAvoidance>().enabled = true;
                catPrefab.GetComponent<CapsuleCollider>().enabled = true;
                mousePrefab.GetComponent<CapsuleCollider>().enabled = false;
                catCamera.SetActive(true);
                break;
            case 2:
                catPrefab.GetComponent<Cat>().enabled = true;
                mousePrefab.GetComponent<PlayerController>().enabled = true;
                mousePrefab.GetComponent<ObstacleAvoidance>().enabled = false;
                catPrefab.GetComponent<CapsuleCollider>().enabled = false;
                mousePrefab.GetComponent<CapsuleCollider>().enabled = true;
                mouseCamera.SetActive(true);
                break;
        }
    }

    // Update is called once per frame
    public void finishGame()
    {
        catMouse.SetActive(true);
        catCamera.SetActive(true);
        GameManager.instance.isFinished = true;
    }
}
