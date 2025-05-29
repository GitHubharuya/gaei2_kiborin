using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cat : MonoBehaviour
{
    [SerializeField]
    GameObject mouse;

    [Header("追跡の速さ")]
    private float chaseSpeed_ = 1.0f;

    [Header("回転の速さ（数値を大きくするとキビキビ回る）")]
    private float rotationSpeed_ = 5f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 direction = mouse.transform.position - this.transform.position;
        direction.y = 0f; // 水平方向のみを考慮

        // ネズミが存在していて、方向ベクトルがゼロでなければ
        if (direction != Vector3.zero)
        {
            // 目標の回転（ネズミの方向を向く）
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            // 現在の回転から滑らかに補間
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed_);
        }

        // 向いている方向（transform.forward）に移動
        transform.Translate(transform.forward * chaseSpeed_ * Time.deltaTime, Space.World);
    }
}
