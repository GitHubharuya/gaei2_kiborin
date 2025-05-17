using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldGenerator : MonoBehaviour
{
    [Header("マップサイズ(奇数)")]
    public int mapSize = 11;
    [Header("シード")]
    public int seed = 0;

    [SerializeField]
    private GameObject groundPrefab;
    [SerializeField]
    private GameObject[] wallPrefabs;
    [SerializeField]
    private GameObject[] furniturePrefabsS;
    [SerializeField]
    private GameObject[] furniturePrefabsL;
    [SerializeField] 
    private GameObject ceilingPrefab;
    [SerializeField]
    private GameObject lightPrefab;
    [SerializeField] 
    private GameObject cheesePrefab;

    private int[] dx = { 1, 0, -1, 0 };
    private int[] dy = { 0, 1, 0, -1 };

    void Start()
    {
        if (mapSize % 2 == 0)
        {
            Debug.Log("サイズが偶数になってるよ");
        }
        else
        {
            GenerateField();
        }
    }

    void GenerateField()
    {
        Random.InitState(seed);

        bool[,] wallMap = new bool[mapSize, mapSize];
        bool[,] visited = new bool[mapSize, mapSize];

        // 全部壁で初期化
        for (int i = 0; i < mapSize; i++)
            for (int j = 0; j < mapSize; j++)
                wallMap[i, j] = true;

        int startX = Random.Range(0, (mapSize - 1) / 2) * 2 + 1;
        int startY = Random.Range(0, (mapSize - 1) / 2) * 2 + 1;

        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        stack.Push(new Vector2Int(startX, startY));
        visited[startX, startY] = true;
        wallMap[startX, startY] = false;

        // 通路生成
        while (stack.Count > 0)
        {
            Vector2Int current = stack.Pop();
            int startDir = Random.Range(0, 4); // Unity乱数

            for (int i = 0; i < 4; i++)
            {
                int dir = (startDir + i) % 4;
                int ni = current.x + dx[dir] * 2;
                int nj = current.y + dy[dir] * 2;

                if (ni < 0 || ni >= mapSize || nj < 0 || nj >= mapSize) continue;
                if (visited[ni, nj]) continue;

                visited[ni, nj] = true;
                wallMap[ni, nj] = false;
                wallMap[current.x + dx[dir], current.y + dy[dir]] = false;
                stack.Push(new Vector2Int(ni, nj));
            }
        }

        // 通路の拡張
        for (int i = 1; i < mapSize - 1; i++)
        {
            for (int j = 1; j < mapSize - 1; j++)
            {
                if (!visited[i, j] && Random.Range(0, 3) < 2)
                {
                    visited[i, j] = true;
                    wallMap[i, j] = false;
                }
            }
        }

        // プレハブを配置

        //障害物の一片の長さl
        float l = 0.5f;

        //まずは壁
        for (int r = 0; r < 4; r++)
        {
            for (int i = 2; i < mapSize + 2; i += 4)
            {
                float _i = 0;
                float _j = 0;
                if (r == 0)
                {
                    _i = (i - 0.5f) * l;
                    _j = 0.5f * l;
                }
                else if (r == 1)
                {
                    _i = 0.5f * l;
                    _j = (i - 0.5f) * l;
                }
                else if (r == 2)
                {
                    _i = (i - 0.5f) * l;
                    _j = (0.5f + mapSize - 2) * l;
                }
                else if (r == 3)
                {
                    _i = (0.5f + mapSize - 2) * l;
                    _j = (i - 0.5f) * l;
                }
                Vector3 pos = new Vector3(_i, 0, _j);
                Instantiate(wallPrefabs[(Random.Range(0, 5)) % 3], pos, Quaternion.Euler(0, 90 * r, 0));
            }
        }

        //壁以外
        bool[,] furnitureCheck = new bool[mapSize, mapSize];

        // 全部壁で初期化
        for (int i = 0; i < mapSize; i++)
            for (int j = 0; j < mapSize; j++)
                furnitureCheck[i, j] = false;

        for (int i = 1; i < mapSize - 1; i++)
        {
            for (int j = 1; j < mapSize - 1; j++)
            {
                //障害物のサイズに合わせて座標を調整
                float _i = i * l;
                float _j = j * l;

                //床・天井を張る
                Vector3 pos = new Vector3(_i, 0, _j);
                Instantiate(groundPrefab, pos, Quaternion.identity, transform);
                
                Vector3 ceilingPos = new Vector3(_i, 3f, _j);
                Quaternion ceilingQt = ceilingPrefab.transform.rotation;
                Instantiate(ceilingPrefab, ceilingPos, ceilingQt, transform);

                if (wallMap[i, j] == true && !furnitureCheck[i, j])
                {
                    Vector3 wallPos = new Vector3(_i, 0, _j);
                    //隣まで続いてる？
                    if (i != mapSize - 1 && wallMap[i + 1, j] && !furnitureCheck[i + 1, j])
                    {
                        wallPos.x += l / 2;
                        furnitureCheck[i + 1, j] = true;
                        Instantiate(furniturePrefabsL[Random.Range(0, 2)], wallPos, Quaternion.Euler(0, 180 * Random.Range(0, 2), 0), transform);
                    }
                    else if (j != mapSize - 1 && wallMap[i, j + 1] && !furnitureCheck[i, j + 1])
                    {
                        wallPos.z += l / 2;
                        furnitureCheck[i, j + 1] = true;
                        Instantiate(furniturePrefabsL[Random.Range(0, 2)], wallPos, Quaternion.Euler(0, 90 + 180 * Random.Range(0, 2), 0), transform);
                    }
                    else
                    {
                        Instantiate(furniturePrefabsS[Random.Range(0, 2)], wallPos, Quaternion.Euler(0, 90 * Random.Range(0, 4), 0), transform);
                    }
                }
                else
                {
                    if(Random.Range(0, 3) == 0)
                    {
                        Vector3 cheesePos = new Vector3(_i, l * 0.2f, _j);
                        Instantiate(cheesePrefab, cheesePos, Quaternion.identity, transform);
                    }
                }

                //furnitureCheck更新
                furnitureCheck[i, j] = true;
            }
        }

        //ライト
        Vector3 lightPos = new Vector3(l * (mapSize / 2), 3, l * (mapSize / 2));
        Instantiate(lightPrefab, lightPos, Quaternion.identity, transform);
    }
}