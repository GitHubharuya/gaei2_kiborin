using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cat : MonoBehaviour
{
    [SerializeField]
    GameObject mouse;

    [Header("追跡の速さ")]
    private float chaseSpeed_ = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 dir = (mouse.transform.position - this.transform.position).normalized;
        //指定した量進む
        float vx = dir.x * chaseSpeed_;
        float vz = dir.z * chaseSpeed_;
        this.transform.Translate(vx / 50, 0, vz / 50);
    }
}
