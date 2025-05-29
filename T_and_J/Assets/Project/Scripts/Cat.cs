using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Cat : MonoBehaviour
{

    [SerializeField]
    GameObject mouse;
    [Header("追跡の速さ")]
    private float chaseSpeed_ = 0.125f;

    int y = 0;
    int cnt = 0;
    public List<int> go = new List<int>();

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        float vx = 3 * chaseSpeed_;
        if (go[y] == 0) this.transform.Translate(vx / 15, 0, 0);
        if (go[y] == 1) this.transform.Translate(0, 0, vx / 15);
        if (go[y] == 2) this.transform.Translate(-vx / 15, 0, 0);
        if (go[y] == 3) this.transform.Translate(0, 0, -vx / 15);
        cnt++;
        if (cnt == 5)
        {
            y++;
            cnt = 0;
        }
        if (go.Count == y) y = 0;
    }

    public void startDFS()
    {
        bool[,] map;
        List<int> dx = new List<int>() { 1, 0, -1, 0 };
        List<int> dy = new List<int>() { 0, 1, 0, -1 };
        bool[,] v = new bool[GameManager.instance.wallMap.GetLength(0), GameManager.instance.wallMap.GetLength(0)];
        void DFS(int nowx, int nowy, int muki)
        {
            bool move = false;
            for (int i = 0; i < 4; i++)
            {
                int ni = nowx + dx[i], nj = nowy + dy[i];
                if (ni < 0 || ni >= GameManager.instance.wallMap.GetLength(0) || nj < 0 || nj >= GameManager.instance.wallMap.GetLength(0)) continue;
                if (v[ni, nj]) continue;
                if (GameManager.instance.wallMap[ni, nj]) continue;
                v[ni, nj] = true;
                go.Add(i);
                DFS(ni, nj, i);
                move = true;
            }
            if (!move) go.Add((muki + 2) % 4);
        }
        DFS(1, 1, -1);
        Debug.Log(GameManager.instance.wallMap.GetLength(0));
        /*for(int i = 0; i < GameManager.instance.wallMap.GetLength(0); i++)
        {
            for (int j = 0; j < GameManager.instance.wallMap.GetLength(0); j++)
            {
                Debug.Log(GameManager.instance.wallMap[i, j]);
            }
        }*/
    }
}