//　CubeAtion.cs

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using TMPro;
using System.Data;
using Unity.VisualScripting;
using System;

public class Cat : MonoBehaviour
{
    public int x;
    public int y;
    public float cnt;
    // Start is called before the first frame update
    public bool[,] map;
    public bool[,] v;
    List<int> dx = new List<int>() { 1, 0, -1, 0 };
    List<int> dy = new List<int>() { 0, 1, 0, -1 };

    List<int> go = new List<int>() { 1 };

    [Header("追跡の速さ")]
    public float chaseSpeed_ = 0.5f;
    [Header("ねずみ")]
    public Collider mouseCollider;

    public float angle = 45f;
    public bool inHit = new bool();

    void Start()
    {
        int N = GameManager.instance.wallMap.GetLength(0);
        v = new bool[N, N];
        y = 0;
        cnt = 0;
        void DFS(int nowx, int nowy, int muki)
        {
            bool move = false;
            for (int i = 0; i < 4; i++)
            {
                int ni = nowx + dx[i], nj = nowy + dy[i];
                if (ni < 0 || ni >= N || nj < 0 || nj >= N) continue;
                if (v[ni, nj]) continue;
                if (GameManager.instance.wallMap[ni, nj]) continue;
                v[ni, nj] = true;
                go.Add(i);
                DFS(ni, nj, i);
                move = true;
            }
            if (!move) go.Add((muki + 2) % 4);
        }
        DFS(0, 0, -1);
    }

    // Update is called once per frame
    void Update()
    {
        float vx = 1 * chaseSpeed_;
        if (go[y] == 0) this.transform.Translate(vx / 25, 0, 0);
        if (go[y] == 1) this.transform.Translate(0, 0, vx / 25);
        if (go[y] == 2) this.transform.Translate(-vx / 25, 0, 0);
        if (go[y] == 3) this.transform.Translate(0, 0, -vx / 25);
        cnt++;
        if (cnt == 20)
        {
            y++;
            cnt = 0;
        }
        if (go.Count == y) y = 0;
        if (OnTriggerStay(mouseCollider))
        {
            Debug.Log("入ってる");
        }
        else Debug.Log("入ってない");
    }

    private bool OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Mouth")  //視界の範囲内の当たり判定
        {
            //視界の角度内に収まっているか
            Vector3 posDelta = other.transform.position - this.transform.position;
            float target_angle = Vector3.Angle(this.transform.forward, posDelta);

            if (target_angle < angle)
            {
                if (Physics.Raycast(this.transform.position, posDelta, out RaycastHit hit))  //Rayを使用してtargetに当たっているか判定
                {
                    if (hit.collider == other)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }
}