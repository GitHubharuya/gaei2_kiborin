using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ObstacleAvoidance : MonoBehaviour
{
    public float detectionRange = 1.0f; //障害物検出距離
    public float avoidDistance = 2f; //障害物回避のための左右検出距離

    public float moveSpeed = 0.5f;
    public float rotationSpeed = 5f; //回避時旋回速度

    public LayerMask obstacleLayer;

    public Transform player;

    private int cheeseCount = 0; // ← チーズの数をカウント
    public TextMeshProUGUI cheeseText;      // ← UIへの参照（Inspectorで設定）

    private void Start()
    {
        
    }

    void Update()
    {
        
            AvoidObstaclesAndMove();
    }

 

    void AvoidObstaclesAndMove()
    {
        Vector3 origin = transform.position; //現在のネズミ位置を取得
        Vector3 forwardDir = transform.forward; //ネズミ視点から見て前の方向ベクトル
        Vector3 upwardDirection = (forwardDir + Vector3.up).normalized; //ネズミ視点から見て上の方向ベクトル

        bool inFront = false;
        bool above = false;
        Debug.DrawRay(origin, forwardDir * detectionRange, Color.red); //Rayの可視化
        Debug.DrawRay(origin, upwardDirection * detectionRange, Color.blue); //Rayの可視化

        if (Physics.Raycast(origin, forwardDir, out RaycastHit hitinfo, detectionRange, obstacleLayer))
        {
            if (hitinfo.collider.CompareTag("Obstacle"))
            {
                Debug.Log("障害物に接触");
                inFront = true;
            }         
            
        }
        if(Physics.Raycast(origin, upwardDirection, out RaycastHit hitinfo2, detectionRange, obstacleLayer))
        {
            if (hitinfo2.collider.CompareTag("Obstacle"))
            {
                Debug.Log("障害物に接触");
                above = true;
            }
        }

        if (inFront || above)
        {
            bool clearLeft = !Physics.Raycast(origin, -transform.right, avoidDistance, obstacleLayer); //ネズミ左方向に障害物がないか探索
            bool clearRight = !Physics.Raycast(origin, transform.right, avoidDistance, obstacleLayer); //上と同様(右ver)

            Vector3 desiredDirection = Vector3.zero; //回避する方向のベクトルはここに格納しまーす
            if (clearLeft && clearRight) //右と左両方障害物がないときは
            {
                desiredDirection = Random.value < 0.5f ? -transform.right : transform.right; //ランダムで左か右に進む
            }
            else if (clearLeft)//左障害物ない
            {
                desiredDirection = -transform.right; //左に進む
            }
            else if (clearRight)//右障害物ない
            {
                desiredDirection = transform.right; //右に進む
            }
            else//3方向詰み
            {
                desiredDirection = -forwardDir; //後退
            }
            //回転を滑らかにする処理
            Quaternion targetRotation = Quaternion.LookRotation(desiredDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        //回転判断後は前進!!!
        transform.position += transform.forward * moveSpeed * Time.deltaTime;
    }


    // Update is called once per frame
    
}
