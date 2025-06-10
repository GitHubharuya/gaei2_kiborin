using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using static UnityEngine.UI.Image;
using System;
using System.Net;

public class ObstacleAvoidance : MonoBehaviour
{

    [SerializeField]
    GameObject cat;

    [Header("障害物回避設定")]
    public float detectionRange = 0.2f;
    public float avoidDistance = 0.7f;

    [Header("速度・回転速度設定")]
    public float moveSpeed = 0.5f;
    public float rotationSpeed = 10f;


    [Header("コミット時処理")]
    public float commitDuration = 0.6f;
    private bool isCommitted = false;

    [Header("視界設定")]
    public float viewDistance = 6f; // 視界の距離
    public float findCatDistance = 1f; // 猫を見つける距離
    public float viewAngle = 90f;   // 視野角（左右45度）

    [Header("緊急状態の最低秒数")]
    public float emergencyDuration = 2.0f;
    private bool isEmergency = false; // 緊急状態かどうかのフラグ
    private bool ismoveMouseinEmergency = false; 
    private bool emergencyCoroutineRunning = false; // 緊急状態のコルーチンが実行中かどうかのフラグ


    [Header("緊急状態の移動速度")]
    public float emergencySpeed = 0.5f;
    public float emergencyRotationSpeed = 20f;

    [Header("その他")]
    public LayerMask obstacleLayer;
    private int cheeseCount = 0;
    public TextMeshProUGUI cheeseText;
    public Transform cheese;
    public bool useView = false;
    private Rigidbody rb;

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
        if ( isCommitted || isEmergency )
        {
            
        }
        else if (findCat())
        {
            isEmergency = true; // 猫を見つけたら緊急状態にする


            if (!emergencyCoroutineRunning) // 緊急状態のコルーチンが実行中でない場合
            {
                StartCoroutine(EmergencyEscapeState()); // 緊急状態のコルーチンを開始
            }
        }
        else if (!isCommitted && !isEmergency)
        {
            //Debug.Log(isCommitted);
            bool flag = AvoidObstaclesAndMove();
            if (!flag)
            {
                if (cheese == null)
                {
                    if (useView)
                    {
                        FindNearestCheese_View(); //視界を使ってチーズを探す
                        //Quaternion mouseRotation = Quaternion.LookRotation(-transform.forward);
                        //transform.rotation = Quaternion.Slerp(transform.rotation, mouseRotation, rotationSpeed * Time.deltaTime);
                        //rb.MovePosition(rb.position + -transform.forward * moveSpeed * Time.fixedDeltaTime);
                        //CommitMovement(-transform.forward, commitDuration); 
                    }
                    else
                    {
                        FindNearestCheese(); //視界を使わずにチーズを探す 
                    }
                }

                if (cheese != null)
                {
                    moveSpeed = 0.5f; // チーズに向かうときは通常の速度で移動
                    rotationSpeed = 10f; // チーズに向かうときは通常の回転速度
                    Vector3 direction = cheese.position - transform.position;
                    direction.Normalize();
                    direction.y = 0; // Y軸の成分をゼロにして水平移動にする
                    //transform.position += direction * speed * Time.deltaTime;
                    Quaternion mouseRotaion = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, mouseRotaion, rotationSpeed * Time.deltaTime);
                    rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);
                }
            }
        }
    }



    public bool AvoidObstaclesAndMove()
    {
        bool facingObstacle = false;
        Vector3 origin = transform.position;
        Vector3 forwardDir = transform.forward;
        Vector3 upwardDirection = (forwardDir + Vector3.up).normalized;
        Vector3 forwardDir2 = Quaternion.Euler(0, 10, 0) * forwardDir; 
        Vector3 forwardDir3 = Quaternion.Euler(0, -10, 0) * forwardDir;
        Vector3 dir1 = Quaternion.Euler(0, 30, 0) * transform.forward;
        Vector3 dir2 = Quaternion.Euler(0, -30, 0) * transform.forward;
        Vector3 dir3 = Quaternion.Euler(0, 60, 0) * transform.forward;
        Vector3 dir4 = Quaternion.Euler(0, -60, 0) * transform.forward;
        Vector3 dir5 = Quaternion.Euler(0, 120, 0) * transform.forward;
        Vector3 dir6 = Quaternion.Euler(0, -120, 0) * transform.forward;
        Vector3 dir7 = Quaternion.Euler(0, 150, 0) * transform.forward;
        Vector3 dir8 = Quaternion.Euler(0, -150, 0) * transform.forward;
        Vector3[] directions = new Vector3[] { dir1, dir2, dir3, dir4, -transform.right, transform.right, dir5, dir6, dir7, dir8 };


        bool inFront = false;
        bool above = false;
        bool fDir2 = false;
        bool fDir3 = false;

        Debug.DrawRay(origin, forwardDir * detectionRange, Color.red);
        Debug.DrawRay(origin, upwardDirection * detectionRange, Color.red);
        Debug.DrawRay(origin, forwardDir2 * detectionRange, Color.red);
        Debug.DrawRay(origin, forwardDir3 * detectionRange, Color.red);
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

                inFront = true;
            }

        }
        if (Physics.Raycast(origin, upwardDirection, out RaycastHit hitinfo2, detectionRange, obstacleLayer))
        {
            if (hitinfo2.collider.CompareTag("Obstacle"))
            {

                above = true;
            }
        }
        if (Physics.Raycast(origin, forwardDir2, out RaycastHit hitinfo3, detectionRange, obstacleLayer))
        {
            if (hitinfo3.collider.CompareTag("Obstacle"))
            {
                fDir2 = true;
            }
        }
        if (Physics.Raycast(origin, forwardDir3, out RaycastHit hitinfo4, detectionRange, obstacleLayer))
        {
            if (hitinfo4.collider.CompareTag("Obstacle"))
            {
                fDir3 = true;
            }
        }

        if (inFront || above || fDir2 || fDir3)
        {
            facingObstacle = true;
            mouseMovement(origin, forwardDir, directions);
        }


        //transform.position += transform.forward * moveSpeed * Time.deltaTime;

        return facingObstacle;
    }


    // Update is called once per frame
    public void mouseMovement(Vector3 origin, Vector3 forwardDir, Vector3[] directions)
    {
        bool clearDir1 = !Physics.Raycast(origin, directions[0], avoidDistance, obstacleLayer);
        bool clearDir2 = !Physics.Raycast(origin, directions[1], avoidDistance, obstacleLayer);
        bool clearDir3 = !Physics.Raycast(origin, directions[2], avoidDistance, obstacleLayer);
        bool clearDir4 = !Physics.Raycast(origin, directions[3], avoidDistance, obstacleLayer);
        bool clearLeft = !Physics.Raycast(origin, directions[4], avoidDistance, obstacleLayer);
        bool clearRight = !Physics.Raycast(origin, directions[5], avoidDistance, obstacleLayer);
        bool clearDir5 = !Physics.Raycast(origin, directions[6], avoidDistance, obstacleLayer);
        bool clearDir6 = !Physics.Raycast(origin, directions[7], avoidDistance, obstacleLayer);
        bool clearDir7 = !Physics.Raycast(origin, directions[8], avoidDistance, obstacleLayer);
        bool clearDir8 = !Physics.Raycast(origin, directions[9], avoidDistance, obstacleLayer);
        List<Vector3> clearDirections = new List<Vector3>();

        int avoidIndex = 0;

        foreach (Vector3 dir in directions)
        {
            if (!Physics.Raycast(origin, dir, avoidDistance, obstacleLayer))
            {
                clearDirections.Add(dir);
            }
        }

        Vector3 desiredDirection = Vector3.zero;

        if (clearDirections.Count > 0)
        {
            Debug.Log("move to avoid obstacle");
            //int randomIndex = avoidIndex; 
            desiredDirection = clearDirections[avoidIndex];
        }
        else
        {
            Debug.Log("move to backward");
            desiredDirection = -forwardDir;
        }

        /*if (clearLeft && clearRight) 
        {
           
            desiredDirection = Random.value < 0.5f ? -transform.right : transform.right;   
        }
        else if (clearLeft)
        {
            
            desiredDirection = -transform.right;  
        }
        else if (clearRight)
        {
            
            desiredDirection = transform.right; 
        
        else
        {
            
            desiredDirection = -forwardDir; 
        }
        */


        Quaternion targetRotation = Quaternion.LookRotation(desiredDirection, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);


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
            cheese = null;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        Vector3 forward = transform.forward;
        Quaternion leftRayRotation = Quaternion.Euler(0, -viewAngle / 2, 0);
        Quaternion rightRayRotation = Quaternion.Euler(0, viewAngle / 2, 0);

        Vector3 leftRay = leftRayRotation * forward;
        Vector3 rightRay = rightRayRotation * forward;

        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, leftRay * viewDistance);
        Gizmos.DrawRay(transform.position, rightRay * viewDistance);
    }

    void FindNearestCheese_View()
    {
        GameObject[] cheeses = GameObject.FindGameObjectsWithTag("Cheese");
        float closestDistance = Mathf.Infinity;
        Transform closest = null;

        foreach (GameObject c in cheeses)
        {
            Vector3 toCheese = c.transform.position - transform.position;
            float distance = toCheese.magnitude;

            if (distance > viewDistance) continue; // 視界の距離外

            float angle = Vector3.Angle(transform.forward, toCheese);
            if (angle > viewAngle / 2f) continue; // 視野角外

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = c.transform;
            }
        }
        if (closest == null)
        {
            Debug.Log("No cheese found in view.");
            // 視界内にチーズがない場合の挙動については検討の余地あり
            System.Random rand = new System.Random();
            float randNum = rand.Next(10, 180);


            StartCoroutine(CommitMovement(Quaternion.Euler(0, randNum, 0) * -transform.forward, commitDuration)); // 視界内にチーズがない場合は後退
        }
        else
        {
            cheese = closest;
        }
    }

    private IEnumerator CommitMovement(Vector3 direction, float commitTime)
    {
        Debug.Log("Committing...");
        moveSpeed = 0.5f; // コミット中は通常の移動速度
        rotationSpeed = 10f; // コミット中は通常の回転速度
        isCommitted = true;
        float startTime = Time.time;
        direction.y = 0; // 水平移動にするためY成分をゼロにする

        Quaternion targetRotaion = Quaternion.LookRotation(direction, Vector3.up);

        while (Time.time < startTime + commitTime)
        {
            rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotaion, rotationSpeed * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }
        isCommitted = false;

        Debug.Log("Commit complete.");
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

        cheese = closest;
    }

    private bool findCat()
    {
        Vector3 toCat = cat.transform.position - transform.position;
        float distance = toCat.magnitude;
        float angle = Vector3.Angle(transform.forward, toCat);

        if (distance < findCatDistance)
        {

            if (angle < viewAngle / 2f)
            {
                Debug.Log("Cat found in view!");
                return true;
            }
        }
        return false;

    }

    private IEnumerator EmergencyEscapeState()
    {
        emergencySpeed = 0.5f; // 緊急状態の移動速度を設定
        emergencyRotationSpeed = 10f; // 緊急状態の回転速度を設定
        Debug.Log("Emergency escape state started.");
        emergencyCoroutineRunning = true;
        isEmergency = true;
        float startTime = Time.time;
        Vector3 dir = cat.transform.position - this.transform.position;
        dir.y = 0;
        dir.Normalize();

        while (Time.time < startTime + emergencyDuration)
        {
            //detectionRange = 0.4f;
            //avoidDistance = 1.0f;

            bool flag = false;
            if (!isCommitted)
            {
                flag = AvoidObstaclesAndMove();
            }
            if (!flag && !isCommitted) //障害物回避が成功した場合は緊急状態を維持する
            {
                if(!ismoveMouseinEmergency)
                {
                    ismoveMouseinEmergency = true;
                    StartCoroutine(moveMouseinEmergency(0.3f, -dir));
                }
                
                yield return new WaitForFixedUpdate();

            }
            yield return new WaitForFixedUpdate();

        }
        isEmergency = false;
        emergencyCoroutineRunning = false;
        //detectionRange = 0.2f; // 元の設定に戻す
        //avoidDistance = 0.7f; // 元の設定に戻す
        Debug.Log("Emergency escape state ended.");
    }

    private IEnumerator moveMouseinEmergency(float time, Vector3 dir)
    {
        emergencySpeed = 0.5f; // 緊急状態の移動速度を設定
        emergencyRotationSpeed = 10f; // 緊急状態の回転速度を設定
        ismoveMouseinEmergency = true;
        float startTime = Time.time;

        while( Time.time < startTime + time)
        {
            Quaternion mouseRotation = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, mouseRotation, emergencyRotationSpeed * Time.fixedDeltaTime);
            rb.MovePosition(rb.position + dir * emergencySpeed * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }
        ismoveMouseinEmergency = false;

    }

    public void beEaten()
    {
        Destroy(gameObject);
    }

}