using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using static UnityEngine.UI.Image;

public class ObstacleAvoidance : MonoBehaviour
{
    public float detectionRange = 0.2f; //障害物検出距離
    public float avoidDistance = 0.7f; //障害物回避のための左右検出距離

    public float moveSpeed = 0.5f;
    public float rotationSpeed = 10f; //回避時旋回速度

    public LayerMask obstacleLayer;

    public Transform player;

    private int cheeseCount = 0; // ← チーズの数をカウント
    public TextMeshProUGUI cheeseText;      // ← UIへの参照（Inspectorで設定）


    private Rigidbody rb; //Rigidbodyコンポーネントを格納する変数

    public float commitDuration = 0.6f; // 回避動作の持続時間
    private bool isCommitted = false;
    
    private void Start()
    {
        UpdateCheeseUI();
        rb = GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        if (!isCommitted)
        {
            //Debug.Log(isCommitted);
            bool flag = AvoidObstaclesAndMove();
            if (!flag)
            {
                if (player == null)
                {
                    FindNearestCheese();
                }

                if (player != null)
                {
                    Vector3 direction = player.position - transform.position;
                    direction.Normalize();
                    //transform.position += direction * speed * Time.deltaTime;
                    Quaternion mouseRotaion = Quaternion.LookRotation(direction); //ネズミの向きをプレイヤー方向に向ける
                    transform.rotation = Quaternion.Slerp(transform.rotation, mouseRotaion, rotationSpeed * Time.deltaTime); //ネズミの向きを滑らかに変える
                    rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime); //Rigidbodyを使って移動
                }
            }
        }
    }



    public bool AvoidObstaclesAndMove()
    {
        bool facingObstacle = false;
        Vector3 origin = transform.position; //現在のネズミ位置を取得
        Vector3 forwardDir = transform.forward; //ネズミ視点から見て前の方向ベクトル
        Vector3 upwardDirection = (forwardDir + Vector3.up).normalized; //ネズミ視点から見て上の方向ベクトル
        Vector3 dir1 = Quaternion.Euler(0,30,0) * transform.forward; 
        Vector3 dir2 = Quaternion.Euler(0,-30,0) * transform.forward; 
        Vector3 dir3 = Quaternion.Euler(0, 60, 0) * transform.forward;
        Vector3 dir4 = Quaternion.Euler(0, -60, 0) * transform.forward; 
        Vector3 dir5 = Quaternion.Euler(0, 120, 0) * transform.forward;
        Vector3 dir6 = Quaternion.Euler(0, -120, 0) * transform.forward; 
        Vector3 dir7 = Quaternion.Euler(0, 150, 0) * transform.forward;
        Vector3 dir8 = Quaternion.Euler(0, -150, 0) * transform.forward;
        Vector3[] directions = new Vector3[] { -transform.right, transform.right, dir1, dir2, dir3, dir4, dir5, dir6, dir7, dir8 };
        // どこに逃げることができるのか、これで判断

        bool inFront = false;
        bool above = false;

        Debug.DrawRay(origin, forwardDir * detectionRange, Color.red); //Rayの可視化
        Debug.DrawRay(origin, upwardDirection * detectionRange, Color.red); //Rayの可視化
        Debug.DrawRay(origin, -transform.right * avoidDistance, Color.green);
        Debug.DrawRay(origin, transform.right * avoidDistance, Color.green);
        Debug.DrawRay(origin, dir1 * avoidDistance, Color.green);
        Debug.DrawRay(origin, dir2 * avoidDistance, Color.green);
        Debug.DrawRay(origin, dir3 * avoidDistance, Color.green);
        Debug.DrawRay(origin, dir4 * avoidDistance, Color.green);
        Debug.DrawRay(origin, dir5 * avoidDistance, Color.green);
        Debug.DrawRay(origin, dir6 * avoidDistance, Color.green);
        Debug.DrawRay(origin, dir7 * avoidDistance, Color.green);
        Debug.DrawRay(origin, dir8 * avoidDistance, Color.green);

        if (Physics.Raycast(origin, forwardDir, out RaycastHit hitinfo, detectionRange, obstacleLayer))
        {
            if (hitinfo.collider.CompareTag("Obstacle"))
            {
                //Debug.Log("障害物に接触");
                inFront = true;
            }         
            
        }
        if(Physics.Raycast(origin, upwardDirection, out RaycastHit hitinfo2, detectionRange, obstacleLayer))
        {
            if (hitinfo2.collider.CompareTag("Obstacle"))
            {
                //Debug.Log("障害物に接触");
                above = true;
            }
        }

        if (inFront || above)
        {
            facingObstacle = true;
            mouseMovement(origin, forwardDir,directions); //障害物が前方にある場合は回避処理を呼び出す
        }
        

        //transform.position += transform.forward * moveSpeed * Time.deltaTime;

        return facingObstacle; //障害物に接触しているかどうかを返す
    }


    // Update is called once per frame
    public void mouseMovement(Vector3 origin, Vector3 forwardDir, Vector3[] directions)
    {
        bool clearLeft = !Physics.Raycast(origin, directions[0], avoidDistance, obstacleLayer); //ネズミ左方向に障害物がないか探索
        bool clearRight = !Physics.Raycast(origin, directions[1], avoidDistance, obstacleLayer); //上と同様(右ver)
        bool clearDir1 = !Physics.Raycast(origin, directions[2], avoidDistance, obstacleLayer); //30度右方向に障害物がないか探索
        bool clearDir2 = !Physics.Raycast(origin, directions[3], avoidDistance, obstacleLayer); //30度左方向に障害物がないか探索
        bool clearDir3 = !Physics.Raycast(origin, directions[4], avoidDistance, obstacleLayer); //60度右方向に障害物がないか探索
        bool clearDir4 = !Physics.Raycast(origin, directions[5], avoidDistance, obstacleLayer); //60度左方向に障害物がないか探索
        bool clearDir5 = !Physics.Raycast(origin, directions[6], avoidDistance, obstacleLayer); //120度右方向に障害物がないか探索
        bool clearDir6 = !Physics.Raycast(origin, directions[7], avoidDistance, obstacleLayer); //120度左方向に障害物がないか探索
        bool clearDir7 = !Physics.Raycast(origin, directions[8], avoidDistance, obstacleLayer); //150度右方向に障害物がないか探索
        bool clearDir8 = !Physics.Raycast(origin, directions[9], avoidDistance, obstacleLayer); //150度左方向に障害物がないか探索
        List<Vector3> clearDirections = new List<Vector3>(); //障害物がない方向を格納するリスト

        foreach(Vector3 dir in directions)
        {
            if(!Physics.Raycast(origin, dir, avoidDistance, obstacleLayer))
            {
                clearDirections.Add(dir);
            }
        }

        Vector3 desiredDirection = Vector3.zero; //回避する方向のベクトルはここに格納しまーす
        
        if(clearDirections.Count > 0) {
            Debug.Log("障害物なしの方向が見つかった、回避します");
            int randomIndex = Random.Range(0, clearDirections.Count); //障害物がない方向の中からランダムで選ぶ
            desiredDirection = clearDirections[randomIndex];
        }
        else
        {
            Debug.Log("障害物なしの方向が見つからない、後退します");
            desiredDirection = -forwardDir; //障害物がない方向が見つからなかったら後退
        }

        /*if (clearLeft && clearRight) //右と左両方障害物がないときは
        {
            Debug.Log("障害物なし、ランダムで回避方向を決定");
            desiredDirection = Random.value < 0.5f ? -transform.right : transform.right; //ランダムで左か右に進む
        }
        else if (clearLeft)//左障害物ない
        {
            Debug.Log("左に回避");
            desiredDirection = -transform.right; //左に進む
        }
        else if (clearRight)//右障害物ない
        {
            Debug.Log("右に回避");
            desiredDirection = transform.right; //右に進む
        }
        else//3方向詰み
        {
            Debug.Log("3方向詰み、後退");
            desiredDirection = -forwardDir; //後退
        }
        */

        //回転を滑らかにする処理
        Quaternion targetRotation = Quaternion.LookRotation(desiredDirection, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        //回転判断後は前進!!!
        StartCoroutine(CommitMovement(desiredDirection, commitDuration));

    }

    void UpdateCheeseUI()
    {
        if (cheeseText != null)
        {
            cheeseText.text = "Cheese: " + cheeseCount.ToString();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Cheese"))
        {
            Destroy(other.gameObject);
            cheeseCount++;
            UpdateCheeseUI();
            player = null;
        }
    }

    void FindNearestCheese()
    {
        GameObject[] cheeses = GameObject.FindGameObjectsWithTag("Cheese");
        float closestDistance = Mathf.Infinity;
        Transform closest = null;

        foreach (GameObject cheese in cheeses)
        {
            float distance = Vector3.Distance(transform.position, cheese.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = cheese.transform;
            }
        }

        player = closest;
    }

    private IEnumerator CommitMovement(Vector3 direction, float commitTime)
    {
        isCommitted = true;
        float startTime = Time.time;

        Quaternion targetRotaion = Quaternion.LookRotation(direction, Vector3.up); //回避方向に向ける

        while (Time.time < startTime + commitTime)
        {
            rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotaion, rotationSpeed * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }
        isCommitted = false;
    }
}




