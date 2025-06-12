using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Cat : MonoBehaviour
{
    [SerializeField]
    GameObject mouse;

    [Header("移動の速さ（単位：Unity単位/秒）")]
    [SerializeField]
    private float moveSpeed = 1.0f;

    // タイルサイズを0.125に設定
    private const float TILE_SIZE = 0.125f;

    // マップのオフセット（端の座標）
    private const float MAP_OFFSET_X = 0.125f;
    private const float MAP_OFFSET_Z = 0.125f;

    // 四角形の情報を保持するクラス
    [System.Serializable]
    public class Rectangle
    {
        public int x, y, width, height;
        public Vector3 center;

        public Rectangle(int x, int y, int width, int height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            // Unity座標系での中心点を計算（マップ座標系を考慮）
            this.center = new Vector3(
                MAP_OFFSET_X + (x + width / 2.0f) * TILE_SIZE,
                0f, // y座標を0に設定
                MAP_OFFSET_Z + (y + height / 2.0f) * TILE_SIZE
            );
        }
    }

    // A*アルゴリズム用のノードクラス
    public class AStarNode
    {
        public int x, y;
        public float gCost, hCost;
        public float fCost => gCost + hCost;
        public AStarNode parent;

        public AStarNode(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    private List<Rectangle> rectangles = new List<Rectangle>();
    private int currentRectIndex = 0;
    private bool isMovingToTarget = false;
    private List<Vector3> currentPath = new List<Vector3>();
    private int currentPathIndex = 0;
    private Vector3 startPosition;
    private float totalDistance = 0f;
    private float currentDistance = 0f;

    void Start()
    {
        Debug.Log("=== Start() 開始 ===");
        Debug.Log($"タイルサイズ設定: {TILE_SIZE}");
        Debug.Log($"マップオフセット: X={MAP_OFFSET_X}, Z={MAP_OFFSET_Z}");

        // GameManagerが初期化されるまで待つ
        StartCoroutine(WaitForGameManagerAndInitialize());

        Debug.Log("=== Start() 終了 ===");
    }

    private IEnumerator WaitForGameManagerAndInitialize()
    {
        // GameManagerとwallMapが利用可能になるまで待つ
        while (GameManager.instance == null || GameManager.instance.wallMap == null)
        {
            Debug.Log("GameManagerまたはwallMapの初期化を待っています...");
            yield return new WaitForSeconds(0.1f);
        }

        Debug.Log("GameManager初期化完了、マップ情報を確認します");

        // マップの端の座標を確認するデバッグコード
        DebugMapBounds();

        // マップを四角形に分割
        DivideMapIntoRectangles();

        Debug.Log($"四角形分割完了: {rectangles.Count}個");

        // 最初の目標位置を設定
        if (rectangles.Count > 0)
        {
            // 最初の位置を現在位置に設定（y座標は0）
            currentRectIndex = 0;
            Vector3 initialPos = rectangles[0].center;
            initialPos.y = 0f;
            transform.position = initialPos;
            Debug.Log($"初期位置設定: {initialPos}");

            // 次の目標を設定
            SetNextTarget();
        }
        else
        {
            Debug.LogWarning("四角形が見つかりませんでした！");
            // フォールバック：簡単なテスト用の四角形を作成
            CreateTestRectangles();
        }
    }

    private void CreateTestRectangles()
    {
        Debug.Log("テスト用四角形を作成します");
        rectangles.Clear();

        // テスト用の四角形をいくつか作成
        rectangles.Add(new Rectangle(5, 5, 3, 3));
        rectangles.Add(new Rectangle(10, 8, 4, 2));
        rectangles.Add(new Rectangle(15, 12, 2, 4));
        rectangles.Add(new Rectangle(8, 15, 5, 3));

        Debug.Log($"テスト用四角形を{rectangles.Count}個作成しました");

        // 最初の位置を設定
        if (rectangles.Count > 0)
        {
            currentRectIndex = 0;
            Vector3 initialPos = rectangles[0].center;
            initialPos.y = 0f;
            transform.position = initialPos;
            Debug.Log($"テスト用初期位置設定: {initialPos}");

            // 次の目標を設定
            SetNextTarget();
        }
    }

    private void Update()
    {
        if (rectangles.Count == 0)
        {
            Debug.LogWarning("四角形が0個のため、Update処理をスキップします");
            return;
        }

        // パスに沿って移動
        MoveAlongPath();

        // テスト用：スペースキーで次の目標に移動
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("スペースキーで次の目標に移動");
            SetNextTarget();
        }

        // テスト用：Rキーで四角形を再生成
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Rキーで四角形を再生成");
            if (GameManager.instance?.wallMap != null)
            {
                DebugMapBounds(); // マップ境界を再確認
                DivideMapIntoRectangles();
            }
            else
            {
                CreateTestRectangles();
            }
        }

        // テスト用：Dキーでマップ境界デバッグ情報を表示
        if (Input.GetKeyDown(KeyCode.D))
        {
            Debug.Log("Dキーでマップ境界デバッグ");
            DebugMapBounds();
        }
    }

    private void MoveAlongPath()
    {
        if (!isMovingToTarget || currentPath.Count == 0) return;

        if (currentPathIndex >= currentPath.Count)
        {
            // パス完了
            isMovingToTarget = false;
            currentDistance = 0f;
            Debug.Log("目標到達！");

            // 少し待ってから次の目標を設定
            Invoke(nameof(SetNextTarget), 0f);
            return;
        }

        Vector3 targetPos = currentPath[currentPathIndex];
        Vector3 currentPos = transform.position;

        // 現在の目標点までの距離
        float distanceToTarget = Vector3.Distance(currentPos, targetPos);

        // 移動距離を計算（一定速度）
        float moveDistance = moveSpeed * Time.deltaTime;

        if (distanceToTarget <= moveDistance)
        {
            // 目標点に到達
            transform.position = targetPos;
            currentPathIndex++;
            Debug.Log($"パス点{currentPathIndex - 1}に到達: {targetPos}");
        }
        else
        {
            // 目標点に向かって移動
            Vector3 direction = (targetPos - currentPos).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f); // 曲がる早さを調節
            }
            transform.position = currentPos + direction * moveDistance;
        }
    }

    private void SetNextTarget()
    {
        if (rectangles.Count <= 1)
        {
            Debug.LogWarning("四角形が1個以下のため、移動できません");
            return;
        }

        // 次の四角形を選択
        int nextIndex = (currentRectIndex + 1) % rectangles.Count;

        Vector3 startPos = rectangles[currentRectIndex].center;
        Vector3 endPos = rectangles[nextIndex].center;

        // y座標を0に設定
        startPos.y = 0f;
        endPos.y = 0f;

        Debug.Log($"経路探索開始: {startPos} -> {endPos}");

        // GameManagerが利用可能な場合はA*を使用、そうでなければ直線移動
        List<Vector3> path = null;
        if (GameManager.instance?.wallMap != null)
        {
            path = FindPathAStar(startPos, endPos);
        }

        if (path != null && path.Count > 0)
        {
            currentPath = path;
            currentPathIndex = 0;
            isMovingToTarget = true;
            currentRectIndex = nextIndex;

            Debug.Log($"パス生成完了: {path.Count}個の点");
            for (int i = 0; i < path.Count; i++)
            {
                Debug.Log($"  パス点{i}: {path[i]}");
            }
        }
        else
        {
            Debug.LogWarning("A*パスが見つからないか利用不可、直線移動を使用します");
            // 直線移動にフォールバック
            currentPath = new List<Vector3> { endPos };
            currentPathIndex = 0;
            isMovingToTarget = true;
            currentRectIndex = nextIndex;
        }
    }

    private List<Vector3> FindPathAStar(Vector3 start, Vector3 end)
    {
        if (GameManager.instance?.wallMap == null) return null;

        int mapWidth = GameManager.instance.wallMap.GetLength(0);
        int mapHeight = GameManager.instance.wallMap.GetLength(1);

        // ワールド座標をマップ座標に変換（オフセットを考慮）
        int startX = Mathf.RoundToInt((start.x - MAP_OFFSET_X) / TILE_SIZE);
        int startZ = Mathf.RoundToInt((start.z - MAP_OFFSET_Z) / TILE_SIZE);
        int endX = Mathf.RoundToInt((end.x - MAP_OFFSET_X) / TILE_SIZE);
        int endZ = Mathf.RoundToInt((end.z - MAP_OFFSET_Z) / TILE_SIZE);

        // 境界チェック
        startX = Mathf.Clamp(startX, 0, mapWidth - 1);
        startZ = Mathf.Clamp(startZ, 0, mapHeight - 1);
        endX = Mathf.Clamp(endX, 0, mapWidth - 1);
        endZ = Mathf.Clamp(endZ, 0, mapHeight - 1);

        Debug.Log($"マップ座標: ({startX}, {startZ}) -> ({endX}, {endZ})");

        List<AStarNode> openSet = new List<AStarNode>();
        HashSet<string> closedSet = new HashSet<string>();

        AStarNode startNode = new AStarNode(startX, startZ);
        AStarNode endNode = new AStarNode(endX, endZ);

        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            // 最小fCostのノードを選択
            AStarNode currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost)
                    currentNode = openSet[i];
            }

            openSet.Remove(currentNode);
            closedSet.Add($"{currentNode.x},{currentNode.y}");

            // ゴールに到達
            if (currentNode.x == endNode.x && currentNode.y == endNode.y)
            {
                return ReconstructPath(currentNode);
            }

            // 隣接ノードを探索（8方向）
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;

                    int newX = currentNode.x + dx;
                    int newY = currentNode.y + dy;

                    // 境界チェック
                    if (newX < 0 || newX >= mapWidth || newY < 0 || newY >= mapHeight)
                        continue;

                    // 障害物チェック
                    if (GameManager.instance.wallMap[newX, newY])
                        continue;

                    // 閉じたセットにあるかチェック
                    string nodeKey = $"{newX},{newY}";
                    if (closedSet.Contains(nodeKey))
                        continue;

                    // 斜め移動の場合の追加チェック
                    if (dx != 0 && dy != 0)
                    {
                        // 斜め移動時は隣接する2つのマスも空である必要がある
                        if (GameManager.instance.wallMap[currentNode.x + dx, currentNode.y] ||
                            GameManager.instance.wallMap[currentNode.x, currentNode.y + dy])
                            continue;
                    }

                    AStarNode neighbor = new AStarNode(newX, newY);

                    // gCost計算（斜め移動は√2倍）
                    float moveCost = (dx != 0 && dy != 0) ? 1.414f : 1f;
                    float tentativeGCost = currentNode.gCost + moveCost;

                    // 既にオープンセットにあるかチェック
                    AStarNode existingNode = openSet.Find(n => n.x == newX && n.y == newY);
                    if (existingNode != null)
                    {
                        if (tentativeGCost < existingNode.gCost)
                        {
                            existingNode.gCost = tentativeGCost;
                            existingNode.parent = currentNode;
                        }
                    }
                    else
                    {
                        neighbor.gCost = tentativeGCost;
                        neighbor.hCost = Mathf.Abs(newX - endX) + Mathf.Abs(newY - endZ);
                        neighbor.parent = currentNode;
                        openSet.Add(neighbor);
                    }
                }
            }
        }

        Debug.LogWarning("A*パス探索失敗");
        return null;
    }

    private List<Vector3> ReconstructPath(AStarNode endNode)
    {
        List<Vector3> path = new List<Vector3>();
        AStarNode currentNode = endNode;

        while (currentNode != null)
        {
            Vector3 worldPos = new Vector3(
                MAP_OFFSET_X + currentNode.x * TILE_SIZE,
                0f,
                MAP_OFFSET_Z + currentNode.y * TILE_SIZE
            );
            path.Add(worldPos);
            currentNode = currentNode.parent;
        }

        path.Reverse();
        return path;
    }

    public void startDFS()
    {
        DivideMapIntoRectangles();
    }

    private void DivideMapIntoRectangles()
    {
        rectangles.Clear();

        if (GameManager.instance?.wallMap == null)
        {
            Debug.LogError("GameManager.instance.wallMap が null です！");
            return;
        }

        int mapWidth = GameManager.instance.wallMap.GetLength(0);
        int mapHeight = GameManager.instance.wallMap.GetLength(1);
        Debug.Log($"マップサイズ: {mapWidth} x {mapHeight}");

        bool[,] visited = new bool[mapWidth, mapHeight];

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                if (!visited[x, y] && !GameManager.instance.wallMap[x, y])
                {
                    Rectangle rect = FindLargestRectangle(x, y, visited);
                    if (rect != null && rect.width > 0 && rect.height > 0)
                    {
                        rectangles.Add(rect);
                        Debug.Log($"四角形追加: 位置({rect.x}, {rect.y}) サイズ({rect.width}x{rect.height}) 中心{rect.center}");
                    }
                }
            }
        }

        Debug.Log($"分割された四角形数: {rectangles.Count}");
    }

    // マップの端の座標を確認するデバッグ関数
    private void DebugMapBounds()
    {
        if (GameManager.instance?.wallMap == null) return;

        int mapWidth = GameManager.instance.wallMap.GetLength(0);
        int mapHeight = GameManager.instance.wallMap.GetLength(1);

        Debug.Log("=== マップ境界デバッグ情報 ===");
        Debug.Log($"マップ配列サイズ: {mapWidth} x {mapHeight}");

        // マップの4つ角の座標を計算して表示
        Vector3 corner00 = new Vector3(MAP_OFFSET_X + 0 * TILE_SIZE, 0f, MAP_OFFSET_Z + 0 * TILE_SIZE);
        Vector3 cornerMax = new Vector3(MAP_OFFSET_X + (mapWidth - 1) * TILE_SIZE, 0f, MAP_OFFSET_Z + (mapHeight - 1) * TILE_SIZE);

        Debug.Log($"マップ左下角 (0,0): {corner00}");
        Debug.Log($"マップ右上角 ({mapWidth - 1},{mapHeight - 1}): {cornerMax}");

        // 現在のオフセット設定を表示
        Debug.Log($"現在のオフセット設定: X={MAP_OFFSET_X}, Z={MAP_OFFSET_Z}");
        Debug.Log($"タイルサイズ: {TILE_SIZE}");

        // 最初の非壁タイルの位置を見つけて表示
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                if (!GameManager.instance.wallMap[x, y])
                {
                    Vector3 firstOpenTile = new Vector3(
                        MAP_OFFSET_X + x * TILE_SIZE,
                        0f,
                        MAP_OFFSET_Z + y * TILE_SIZE
                    );
                    Debug.Log($"最初の通行可能タイル位置 ({x},{y}): {firstOpenTile}");
                    goto FoundFirst;
                }
            }
        }
    FoundFirst:

        Debug.Log("=== デバッグ情報終了 ===");
    }

    private Rectangle FindLargestRectangle(int startX, int startY, bool[,] visited)
    {
        int mapWidth = GameManager.instance.wallMap.GetLength(0);
        int mapHeight = GameManager.instance.wallMap.GetLength(1);

        int maxWidth = 0;
        int maxHeight = 0;

        // 右方向への最大幅を計算
        for (int x = startX; x < mapWidth; x++)
        {
            if (GameManager.instance.wallMap[x, startY] || visited[x, startY])
                break;
            maxWidth++;
        }

        // 下方向への最大高さを計算
        for (int y = startY; y < mapHeight; y++)
        {
            bool canExtend = true;
            for (int x = startX; x < startX + maxWidth; x++)
            {
                if (GameManager.instance.wallMap[x, y] || visited[x, y])
                {
                    canExtend = false;
                    break;
                }
            }
            if (!canExtend) break;
            maxHeight++;
        }

        // 最大の四角形を見つける
        int bestWidth = 0, bestHeight = 0, bestArea = 0;

        for (int h = 1; h <= maxHeight; h++)
        {
            int w = maxWidth;
            for (int y = startY; y < startY + h; y++)
            {
                int currentWidth = 0;
                for (int x = startX; x < mapWidth; x++)
                {
                    if (GameManager.instance.wallMap[x, y] || visited[x, y])
                        break;
                    currentWidth++;
                }
                w = Mathf.Min(w, currentWidth);
            }

            int area = w * h;
            if (area > bestArea)
            {
                bestArea = area;
                bestWidth = w;
                bestHeight = h;
            }
        }

        // 訪問済みマークを付ける
        for (int y = startY; y < startY + bestHeight; y++)
        {
            for (int x = startX; x < startX + bestWidth; x++)
            {
                visited[x, y] = true;
            }
        }

        return new Rectangle(startX, startY, bestWidth, bestHeight);
    }

    // デバッグ用：四角形とパスを可視化
    void OnDrawGizmos()
    {
        if (rectangles == null) return;

        // 四角形を描画
        Gizmos.color = Color.green;
        foreach (Rectangle rect in rectangles)
        {
            Vector3 center = rect.center;
            center.y = 0f; // y座標を0に設定
            Vector3 size = new Vector3(rect.width * TILE_SIZE, 0.1f, rect.height * TILE_SIZE);
            Gizmos.DrawWireCube(center, size);

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(center, 0.025f);
            Gizmos.color = Color.green;
        }

        // 現在のパスを描画
        if (currentPath != null && currentPath.Count > 0)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < currentPath.Count - 1; i++)
            {
                Gizmos.DrawLine(currentPath[i], currentPath[i + 1]);
            }

            // パス上の点を描画
            Gizmos.color = Color.cyan;
            foreach (Vector3 point in currentPath)
            {
                Gizmos.DrawSphere(point, 0.02f);
            }
        }
    }
}