using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;
    public bool[,] wallMap;
    public bool isFinished = false;
    public float timer;
    public int cheeseCount = 0;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Start()
    {
        timer = 0;
    }

    private void Update()
    {
        if (!isFinished)
        {
            timer += Time.deltaTime;
        }
    }

    public void getCheese()
    {
        cheeseCount++;
        Debug.Log(cheeseCount);
    }
}