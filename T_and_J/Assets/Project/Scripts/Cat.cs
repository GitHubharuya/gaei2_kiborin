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

    [Header("視界の距離・角度設定")]
    public float viewDistance;
    public float viewAngle;


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

    // 既存のフィールドの後に追加
    private enum CatState
    {
        Patrolling,     // 巡回中
        Chasing,        // 追跡中
        Searching       // 捜索中
    }

    private CatState currentState = CatState.Patrolling;
    private Vector3 lastSeenMousePosition;
    private Vector3 previousMousePosition;
    private float lostSightTimer = 0f;
    private const float LOST_SIGHT_TIMEOUT = 10f;
    private Vector3 predictedMousePosition;
    private float mouseTrackingInterval = 0.1f;
    private float lastMouseTrackTime = 0f;

    // フィールドに追加
    private float stateChangeDelay = 0.5f;
    private float lastStateChangeTime = 0f;

    private Animator catAnimator;


    void Start()
    {
        viewDistance = 1.25f; // 強制的に設定
        viewAngle = 180;
        Debug.Log($"Start()でviewDistanceを設定: {viewDistance}");
    
        Debug.Log("=== Start() 開始 ===");
        Debug.Log($"タイルサイズ設定: {TILE_SIZE}");
        Debug.Log($"マップオフセット: X={MAP_OFFSET_X}, Z={MAP_OFFSET_Z}");

        // GameManagerが初期化されるまで待つ
        StartCoroutine(WaitForGameManagerAndInitialize());

        catAnimator = GetComponent<Animator>();

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

        // 初回のマウス位置記録
        if (lastSeenMousePosition == Vector3.zero && mouse != null)
        {
            lastSeenMousePosition = mouse.transform.position;
            previousMousePosition = mouse.transform.position;
        }

        // テスト用キー入力処理
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("スペースキーで次の目標に移動");
            SetNextTarget();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Rキーで四角形を再生成");
            if (GameManager.instance.wallMap != null)
            {
                DebugMapBounds();
                DivideMapIntoRectangles();
            }
            else
            {
                CreateTestRectangles();
            }
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            Debug.Log("Dキーでマップ境界デバッグ");
            DebugMapBounds();
        }

        // 状態のデバッグ出力
        Debug.Log($"現在の状態: {currentState}");

        // 状態変更遅延処理を完全に削除
        switch (currentState)
        {
            case CatState.Patrolling:
                catAnimator.SetBool("isFindMouse", false);
                Debug.Log("巡回状態 - 視界チェック中");
                bool canSeeMouse = SeeSight();
                Debug.Log($"視界チェック結果: {canSeeMouse}");

                if (canSeeMouse)
                {
                    Debug.Log("ネズミを発見！追跡開始");
                    currentState = CatState.Chasing;

                    // パス関連をクリア
                    currentPath.Clear();
                    isMovingToTarget = false;

                    // マウス位置を記録
                    lastSeenMousePosition = mouse.transform.position;
                    previousMousePosition = mouse.transform.position;
                }
                else
                {
                    Debug.Log("ネズミが見えない - 巡回継続");
                    MoveAlongPath();
                }
                break;

            case CatState.Chasing:
                Debug.Log("追跡状態");
                if (SeeSight())
                {
                    catAnimator.SetBool("isFindMouse", true);
                    Debug.Log("ネズミを追跡中");
                    // マウス位置の記録
                    if (Time.time - lastMouseTrackTime > mouseTrackingInterval)
                    {
                        previousMousePosition = lastSeenMousePosition;
                        lastSeenMousePosition = mouse.transform.position;
                        lastMouseTrackTime = Time.time;
                    }

                    // 直線移動でネズミを追跡
                    ChaseMouseDirectly();
                    lostSightTimer = 0f;
                }
                else
                {
                    Debug.Log("ネズミを見失った - 捜索状態に移行");
                    currentState = CatState.Searching;
                    lostSightTimer = 0f;

                    // 予測位置を計算
                    CalculatePredictedPosition();

                    // 予測位置への経路を設定
                    SetPath(transform.position, predictedMousePosition);
                }
                break;

            case CatState.Searching:
                Debug.Log("捜索状態");
                lostSightTimer += Time.deltaTime;

                if (SeeSight())
                {
                    Debug.Log("ネズミを再発見！");
                    currentState = CatState.Chasing;
                    currentPath.Clear();
                    isMovingToTarget = false;
                }
                else if (lostSightTimer >= LOST_SIGHT_TIMEOUT)
                {
                    Debug.Log("捜索タイムアウト - 巡回に戻る");
                    currentState = CatState.Patrolling;

                    currentPath.Clear();
                    isMovingToTarget = false;

                    // 最寄りの四角形を探して設定
                    currentRectIndex = FindNearestRectangle(transform.position);
                    Debug.Log($"最寄りの四角形({currentRectIndex})へ移動開始");

                    // 最寄りの四角形への経路を設定
                    Vector3 nearestCenter = rectangles[currentRectIndex].center;
                    SetPath(transform.position, nearestCenter);
                }
                else
                {
                    Debug.Log("予測位置への移動継続");
                    MoveAlongPath();
                }
                break;
        }
    }

    //視界内にネズミがいるかを返すメソッド
    private bool SeeSight()
    {
        Debug.Log("=== SeeSight() 開始 ===");
        Debug.Log($"現在のviewDistance設定値: {viewDistance}"); // この行を追加

        if (mouse == null)
        {
            Debug.Log("マウスオブジェクトがnullです");
            return false;
        }
        // 以下既存のコード...

        Vector3 catPosition = transform.position;
        Vector3 mousePosition = mouse.transform.position;

        Debug.Log($"猫の位置: {catPosition}");
        Debug.Log($"ネズミの位置: {mousePosition}");

        // y座標を統一
        catPosition.y = 0f;
        mousePosition.y = 0f;

        Vector3 toMouse = mousePosition - catPosition;
        float dis = toMouse.magnitude;

        Debug.Log($"距離: {dis:F2}, 視界距離: {viewDistance}");

        // 距離の基本チェック
        if (dis >= viewDistance)
        {
            Debug.Log("距離が視界範囲外");
            return false;
        }

        // 角度チェック
        Vector3 catForward = transform.forward;
        Debug.Log($"猫の前方向: {catForward}");
        Debug.Log($"ネズミへの方向: {toMouse.normalized}");

        float ang = Vector3.Angle(catForward, toMouse);
        Debug.Log($"角度: {ang:F2}, 視界角度の半分: {viewAngle / 2:F2}");

        if (ang >= viewAngle / 2.0f)
        {
            Debug.Log("角度が視界範囲外");
            return false;
        }

        // 障害物チェック（レイキャスト）- 猫自身を無視するように修正
        Vector3 rayStart = catPosition + Vector3.up * 0.5f; // 高さを0.5fに変更
        Vector3 rayDirection = toMouse.normalized;

        Debug.Log($"レイキャスト開始: {rayStart} -> 方向: {rayDirection}");

        RaycastHit hit;
        // 猫自身のコライダーを無視するため、LayerMaskを使用するか距離を少し短くする
        if (Physics.Raycast(rayStart, rayDirection, out hit, dis - 0.1f))
        {
            Debug.Log($"レイキャストヒット: {hit.collider.name}");
            if (hit.collider.gameObject != mouse && hit.collider.gameObject != gameObject)
            {
                Debug.Log($"障害物により視界遮蔽: {hit.collider.name}");
                return false;
            }
            else
            {
                Debug.Log("レイキャストがネズミにヒットまたは猫自身");
            }
        }
        else
        {
            Debug.Log("レイキャストは何にもヒットしませんでした");
        }
        /*if (dis < 0.250f)
        {
            Debug.Log("非常に近距離 - 視界内と判定");
            return true;
        }*/

        Debug.Log("ネズミを視界内で発見！");
        Debug.Log("=== SeeSight() 終了 ===");
        return true;
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

            // 状態に応じて次の行動を決定
            if (currentState == CatState.Patrolling)
            {
                // 巡回中の場合のみ次の目標を設定
                Invoke(nameof(SetNextTarget), 0.5f);
            }
            // 捜索中の場合は何もしない（その場で待機）
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
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
            }
            Vector3 newPosition = currentPos + direction * moveDistance;
            newPosition.y = 0f; // y座標を0に固定
            transform.position = newPosition;
        }
    }

    public void MoveAlongPath(Vector3 startPos, Vector3 endPos)
    {
        // 新しいパスを設定
        SetPath(startPos, endPos);
    }

    private void SetPath(Vector3 startPos, Vector3 endPos)
    {
        // y座標を0に設定
        startPos.y = 0f;
        endPos.y = 0f;

        Debug.Log($"経路探索開始: {startPos} -> {endPos}");

        // GameManagerが利用可能な場合はA*を使用、そうでなければ直線移動
        List<Vector3> path = null;
        if (GameManager.instance != null && GameManager.instance.wallMap != null)
        {
            path = FindPathAStar(startPos, endPos);
        }

        if (path != null && path.Count > 0)
        {
            currentPath = path;
            currentPathIndex = 0;
            isMovingToTarget = true;

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

        // パスを設定
        SetPath(startPos, endPos);

        // インデックスを更新
        currentRectIndex = nextIndex;
    }

    private void ChaseMouseDirectly()
    {
        if (mouse == null)
        {
            Debug.LogWarning("ChaseMouseDirectly: マウスがnull");
            return;
        }

        Vector3 mousePosition = mouse.transform.position;
        Vector3 currentPosition = transform.position;

        // y座標を0に統一
        mousePosition.y = 0f;
        currentPosition.y = 0f;

        Vector3 toMouse = mousePosition - currentPosition;

        // 距離をチェック（normalizeする前に）
        if (toMouse.magnitude < 0.1f)
        {
            Debug.Log("ChaseMouseDirectly: マウスに非常に近い位置にいます");
            return;
        }

        Vector3 direction = toMouse.normalized;

        Debug.Log($"ChaseMouseDirectly: 現在位置={currentPosition}, マウス位置={mousePosition}, 方向={direction}");

        // 回転（角度差が大きい場合のみ回転）
        float angleDifference = Vector3.Angle(transform.forward, direction);
        if (angleDifference > 5f) // 5度以上の差がある場合のみ回転
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 3f);
        }

        // 移動
        float moveDistance = moveSpeed * Time.deltaTime * 0.1f;
        Vector3 newPosition = currentPosition + direction * moveDistance;
        newPosition.y = 0f;

        transform.position = newPosition;

        Debug.Log($"ChaseMouseDirectly: 新しい位置={newPosition}");
    }

    private void CalculatePredictedPosition()
    {
        if (previousMousePosition != Vector3.zero && lastSeenMousePosition != Vector3.zero)
        {
            // 前回位置から最後に見た位置への移動ベクトル
            Vector3 mouseVelocity = (lastSeenMousePosition - previousMousePosition) / mouseTrackingInterval;

            // 1秒後の予測位置を計算
            predictedMousePosition = lastSeenMousePosition + mouseVelocity * 1f;
        }
        else
        {
            // 予測できない場合は最後に見た位置を使用
            predictedMousePosition = lastSeenMousePosition;
        }

        // y座標を0に設定
        predictedMousePosition.y = 0f;

        // マップ境界内にクランプ（オプション）
        if (GameManager.instance != null && GameManager.instance.wallMap != null)
        {
            int mapWidth = GameManager.instance.wallMap.GetLength(0);
            int mapHeight = GameManager.instance.wallMap.GetLength(1);

            float maxX = MAP_OFFSET_X + (mapWidth - 1) * TILE_SIZE;
            float maxZ = MAP_OFFSET_Z + (mapHeight - 1) * TILE_SIZE;

            predictedMousePosition.x = Mathf.Clamp(predictedMousePosition.x, MAP_OFFSET_X, maxX);
            predictedMousePosition.z = Mathf.Clamp(predictedMousePosition.z, MAP_OFFSET_Z, maxZ);
        }

        Debug.Log($"予測位置計算: 前回={previousMousePosition}, 最後に見た={lastSeenMousePosition}, 予測={predictedMousePosition}");
    }

    private List<Vector3> FindPathAStar(Vector3 start, Vector3 end)
    {
        if (GameManager.instance.wallMap == null) return null;

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

        bool[,] newMap = new bool[mapWidth, mapHeight];
        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {

                        int newX = i + dx;
                        int newY = j + dy;

                        // 境界チェック
                        if (newX < 0 || newX >= mapWidth || newY < 0 || newY >= mapHeight)
                            continue;

                        if (GameManager.instance.wallMap[newX, newY])
                        {
                            newMap[i, j] = true;
                        }
                    }

                }
            }
        }

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
                    if (newMap[newX, newY])
                        continue;

                    // 閉じたセットにあるかチェック
                    string nodeKey = $"{newX},{newY}";
                    if (closedSet.Contains(nodeKey))
                        continue;

                    // 斜め移動の場合の追加チェック
                    if (dx != 0 && dy != 0)
                    {
                        // 斜め移動時は隣接する2つのマスも空である必要がある
                        if (newMap[currentNode.x + dx, currentNode.y] ||
                            newMap[currentNode.x, currentNode.y + dy])
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

        if (GameManager.instance.wallMap == null)
        {
            Debug.LogError("GameManager.instance.wallMap が null です！");
            return;
        }

        int mapWidth = GameManager.instance.wallMap.GetLength(0);
        int mapHeight = GameManager.instance.wallMap.GetLength(1);
        Debug.Log($"マップサイズ: {mapWidth} x {mapHeight}");

        bool[,] visited = new bool[mapWidth, mapHeight];

        bool[,] newMap = new bool[mapWidth, mapHeight];
        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {

                        int newX = i + dx;
                        int newY = j + dy;

                        // 境界チェック
                        if (newX < 0 || newX >= mapWidth || newY < 0 || newY >= mapHeight)
                            continue;

                        if (GameManager.instance.wallMap[newX, newY])
                        {
                            newMap[i, j] = true;
                        }
                    }

                }
            }
        }

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                if (!visited[x, y] && !newMap[x, y])
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

    private int FindNearestRectangle(Vector3 position)
    {
        if (rectangles.Count == 0) return 0;

        int nearestIndex = 0;
        float nearestDistance = Vector3.Distance(position, rectangles[0].center);

        for (int i = 1; i < rectangles.Count; i++)
        {
            float distance = Vector3.Distance(position, rectangles[i].center);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestIndex = i;
            }
        }

        return nearestIndex;
    }

    // マップの端の座標を確認するデバッグ関数
    private void DebugMapBounds()
    {
        if (GameManager.instance.wallMap == null) return;

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

        bool[,] newMap = new bool[mapWidth, mapHeight];
        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {

                        int newX = i + dx;
                        int newY = j + dy;

                        // 境界チェック
                        if (newX < 0 || newX >= mapWidth || newY < 0 || newY >= mapHeight)
                            continue;

                        if (GameManager.instance.wallMap[newX, newY])
                        {
                            newMap[i, j] = true;
                        }
                    }

                }
            }
        }

        // 最初の非壁タイルの位置を見つけて表示
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                if (!newMap[x, y])
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

        bool[,] newMap = new bool[mapWidth, mapHeight];
        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {

                        int newX = i + dx;
                        int newY = j + dy;

                        // 境界チェック
                        if (newX < 0 || newX >= mapWidth || newY < 0 || newY >= mapHeight)
                            continue;

                        if (GameManager.instance.wallMap[newX, newY])
                        {
                            newMap[i, j] = true;
                        }
                    }

                }
            }
        }

        // 右方向への最大幅を計算
        for (int x = startX; x < mapWidth; x++)
        {
            if (newMap[x, startY] || visited[x, startY])
                break;
            maxWidth++;
        }

        // 下方向への最大高さを計算
        for (int y = startY; y < mapHeight; y++)
        {
            bool canExtend = true;
            for (int x = startX; x < startX + maxWidth; x++)
            {
                if (newMap[x, y] || visited[x, y])
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
                    if (newMap[x, y] || visited[x, y])
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

         // 視界の可視化
        if (mouse != null)
        {
            Vector3 toMouse = mouse.transform.position - transform.position;
            float dis = toMouse.magnitude;
            
            if (dis < viewDistance)
            {
             // レイキャストの可視化
                 Gizmos.color = SeeSight() ? Color.green : Color.red;
                Gizmos.DrawLine(transform.position + Vector3.up * 0.1f, mouse.transform.position);
            }
        }

        // 視界範囲の可視化
        Gizmos.color = Color.yellow;
        Vector3 forward = transform.forward;
        Vector3 right = Quaternion.AngleAxis(viewAngle / 2, Vector3.up) * forward;
        Vector3 left = Quaternion.AngleAxis(-viewAngle / 2, Vector3.up) * forward;

        Gizmos.DrawRay(transform.position, right * viewDistance);
        Gizmos.DrawRay(transform.position, left * viewDistance);
    }
}