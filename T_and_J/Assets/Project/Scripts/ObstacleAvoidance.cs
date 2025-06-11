using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using static UnityEngine.UI.Image;
using System;
using System.Net;
using System.Security.Cryptography;
using UnityEngine.Experimental.GlobalIllumination;

public class ObstacleAvoidance : MonoBehaviour
{

    [SerializeField]
    GameObject cat;

    [SerializeField]
    GameObject[] sofa; // Change the type of 'sofa' to an array of GameObjects.
    [SerializeField]
    GameObject[] chest;

    [Header("障害物回避設定")]
    public float detectionRange = 0.2f;
    public float avoidDistance = 0.7f;

    [Header("速度・回転速度設定")]
    public float moveSpeed = 0.5f;
    public float rotationSpeed = 10f;


    [Header("コミット時処理")]
    public float commitDuration = 0.6f;
    private bool isCommitted = false;
    private bool isNoCheeseCommitted = false;

    [Header("視界設定")]
    public float viewDistance = 6f; // 視界の距離
    public float findCatDistance = 0.7f; // 猫を見つける距離(視野ではなく，ネズミ周りの円の半径を示す)
    public float findObstacle = 1.0f; //隠れる障害物を見つける距離
    public float findCattoEscape = 6f;
    public float viewAngle = 90f;   // 視野角（左右45度）

    [Header("緊急状態の最低秒数")]
    public float emergencyDuration = 2.0f;
    private bool isEmergency = false; // 緊急状態かどうかのフラグ
    private bool isMoveMouseinEmergency = false; 
    private bool emergencyCoroutineRunning = false; // 緊急状態のコルーチンが実行中かどうかのフラグ

    [Header("障害物に隠れるときの設定")]
    private bool foundCat = false; //視界内に猫がいるかどうか
    private bool isMoveMouseinFindingCat = false;
    private bool escapeCatCoroutineRunning = false;
    //private bool backwardCoroutineRunning = false;
    public float escapeTime = 2.0f;


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
        if (isCommitted || isEmergency || isNoCheeseCommitted || foundCat)
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
        else if(HidetoObstacle())
        {
            foundCat = true;
            
            if(!escapeCatCoroutineRunning)
            {
                Vector3 dir = DetectEscapeObstacle();
                StartCoroutine(MovetoObstacleCoroutine(escapeTime, dir));
                
            }
            else
            {

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
                        FindNearestCheese_View();
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

    private void FixedUpdate()
    {
        
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


        

        return facingObstacle;
    }

    public Vector3 ChoiceVector(Vector3 origin, Vector3  forwardDir, Vector3[] directions, int flag)
    {
        origin.y += 0.05f;
        List<Vector3> clearDirections = new List<Vector3>();

        int avoidIndex;

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
            if(flag == 0)
            {
                avoidIndex = 0;
            }
            else
            {
                System.Random r = new System.Random();
                int randNum = r.Next(0, clearDirections.Count);

                avoidIndex = randNum;
            }
            Debug.Log("move to avoid obstacle"); 
            desiredDirection = clearDirections[avoidIndex];
        }
        else
        {
            Debug.Log("move to backward");
            desiredDirection = -forwardDir;
        }

        return desiredDirection;
    }
    
    public void mouseMovement(Vector3 origin, Vector3 forwardDir, Vector3[] directions)
    {

        Vector3 desiredDirection = ChoiceVector(origin, forwardDir, directions, 0);

        Quaternion targetRotation = Quaternion.LookRotation(desiredDirection, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);


        StartCoroutine(CommitMovement(desiredDirection, commitDuration));

    }

    public bool HidetoObstacle()
    {
        Vector3 toCat = cat.transform.position - transform.position;
        float distance = toCat.magnitude;
        float angle = Vector3.Angle(transform.forward, toCat);

        if (distance < findCattoEscape && angle < viewAngle / 2f) //視界内に猫居たらスタート
        {
            Debug.Log("find cat.");
            return true;
        }

        return false;
    }


    private Vector3 DetectEscapeObstacle()
    {
        isMoveMouseinFindingCat = true;
      
        GameObject hiddenObstacle = null;

        float dis = Mathf.Infinity;
        float ang;
        bool flag = false;
        foreach (GameObject s in sofa)
        {
            Vector3 toS = s.transform.position - transform.position;
            float t_dis = toS.magnitude;
            ang = Vector3.Angle(transform.forward, toS);

            if(dis > t_dis && ang > viewAngle/2f)
            {
                dis = t_dis;
                flag = true;
                hiddenObstacle = s;
            }
        }
        if (!flag)
        {
            foreach(GameObject c in chest)
            {
                Vector3 toC = c.transform.position - transform.position;
                float t_dis = toC.magnitude;
                ang = Vector3.Angle(transform.forward, toC);

                if(dis > t_dis && ang > viewAngle / 2f)
                {
                    dis = t_dis;
                    flag = true;
                    hiddenObstacle = c;
                }
            }
        }

        if (flag)
        {
            
            Vector3 v = hiddenObstacle.transform.position - transform.position;
            return v;
        }
        else
        {
            
            Vector3 v = Vector3.zero;
            return v;
        }
    }

    /*private IEnumerator MoveToBackwardCoroutine(Vector3 dir)
    {
        backwardCoroutineRunning = true;
        float startTime = Time.time;
        while(Time.time < startTime + 0.3)
        {
            Quaternion backward = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, backward, rotationSpeed * Time.deltaTime);
            yield return new WaitForFixedUpdate();
        }
        backwardCoroutineRunning = false;
    }*/

    private IEnumerator MovetoObstacleCoroutine(float time, Vector3 dir)
    {
        Debug.Log("Move to Obstacle...");

        escapeCatCoroutineRunning = true;
        float startTime = Time.time;
        dir.y = 0;
        dir.Normalize();

        while (Time.time < startTime + escapeTime)
        {
            rb.MovePosition(rb.position + dir * moveSpeed * Time.fixedDeltaTime);
            Quaternion targetRotation = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);

            Vector3 dir1 = transform.position;
            dir1.y += 0.05f;
            Vector3 dir2 = transform.forward;
            Vector3 dir3 = (dir2 + Vector3.up).normalized;
            Vector3 dir4 = Quaternion.Euler(0, 10, 0) * dir2;
            Vector3 dir5 = Quaternion.Euler(0, -10, 0) * dir2;

            bool jug1 = Physics.Raycast(dir1, dir2, out RaycastHit hitinfo, 0.1f, obstacleLayer);
            bool jug2 = Physics.Raycast(dir1, dir3, out RaycastHit hitinfo2, 0.1f, obstacleLayer);
            bool jug3 = Physics.Raycast(dir1, dir4, out RaycastHit hitinfo3, 0.1f, obstacleLayer);
            bool jug4 = Physics.Raycast(dir1, dir5, out RaycastHit hitinfo4, 0.1f, obstacleLayer);

            if (jug1 || jug2 || jug3 || jug4)
            {
                break;
            }

            yield return new WaitForFixedUpdate();
        }
        escapeCatCoroutineRunning = false;
        foundCat = false;
        // StartCoroutine(HideBehindObstacleCoroutine());

    }

    /*private IEnumerator HideBehindObstacleCoroutine()
    {
        Vector3 hideDirection = 
        float startTime = Time.time;
    }*/
    
    

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
            Vector3 dir1 = Quaternion.Euler(0, 0, 0) * transform.forward;
            Vector3 dir2 = Quaternion.Euler(0, 30, 0) * transform.forward;
            Vector3 dir3 = Quaternion.Euler(0, -30, 0) * transform.forward;
            Vector3 dir4 = Quaternion.Euler(0, 60, 0) * transform.forward;
            Vector3 dir5 = Quaternion.Euler(0, -60, 0) * transform.forward;
            Vector3 dir6 = Quaternion.Euler(0, 90, 0) * transform.forward;
            Vector3 dir7 = Quaternion.Euler(0, -90, 0) * transform.forward;
            Vector3 dir8 = Quaternion.Euler(0, 120, 0) * transform.forward;
            Vector3 dir9 = Quaternion.Euler(0, -120, 0) * transform.forward;
            Vector3 dir10 = Quaternion.Euler(0, 150, 0) * transform.forward;
            Vector3 dir11 = Quaternion.Euler(0, -150, 0) * transform.forward;
            Vector3[] directions = new Vector3[] { dir1, dir2, dir3, dir4, dir5, dir6, dir7, dir8, dir9, dir10, dir11 };

            Vector3 direction = ChoiceVector(transform.position, transform.forward, directions, 1);
            StartCoroutine(NoCheeseMovement(direction, commitDuration));
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
            if (!isCommitted)
            {
                
            }
            rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotaion, rotationSpeed * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }
        isCommitted = false;

        Debug.Log("Commit complete.");
    }

    private IEnumerator NoCheeseMovement(Vector3 direction, float commitTime)
    {
        Debug.Log("No Cheese Movement Committing...");
        moveSpeed = 0.5f;
        rotationSpeed = 10f;
        isNoCheeseCommitted = true;
        float startTime = Time.time;
        direction.y = 0;

        Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);

        while(Time.time < startTime + commitTime)
        {
            rb.MovePosition(rb.position + direction*moveSpeed * Time.fixedDeltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }

        isNoCheeseCommitted = false;

        Debug.Log("No Cheese Movement Complete.");
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

    private bool findCat() //すごい近くに猫がいたとき，緊急退避
    {
        Vector3 toCat = cat.transform.position - transform.position;
        float distance = toCat.magnitude;
        float angle = Vector3.Angle(transform.forward, toCat);

        if (distance < findCatDistance)
        {
           
            Debug.Log("Escape!!");
            return true;
            
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
                if(!isMoveMouseinEmergency)
                {
                    isMoveMouseinEmergency = true;
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
        isMoveMouseinEmergency = true;
        float startTime = Time.time;

        while( Time.time < startTime + time)
        {
            Quaternion mouseRotation = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, mouseRotation, emergencyRotationSpeed * Time.fixedDeltaTime);
            rb.MovePosition(rb.position + dir * emergencySpeed * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }
        isMoveMouseinEmergency = false;

    }

    public void beEaten()
    {
        Destroy(gameObject);
    }

}