using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;
    public bool[,] wallMap;
    public bool isFinished = false;
    public float timer;
    //*timerはFieldGeneratorで初期化！！！*
    public int cheeseCount = 0;

    [Header("モード(0,1,2)")]
    public int gameMode;

    public int mapSize;
    public int seed;

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

    private void Start()
    {
        
    }
}