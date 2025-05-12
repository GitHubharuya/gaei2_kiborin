using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mouse : MonoBehaviour
{
    [SerializeField]
    GameObject cat;

    [Header("逃走の速さ")]
    private float runSpeed_ = 0.5f;

    public bool isInWall = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 dir = (cat.transform.position - this.transform.position).normalized;
        //指定した量進む
        float vx = dir.x * runSpeed_;
        float vz = dir.z * runSpeed_;
        this.transform.Translate(-vx / 50, 0, -vz / 50);
    }

    public void changeBoolTrue()
    {
        isInWall = true;
        Debug.Log("入ったよ");
    }
    public void changeBoolFalse()
    {
        isInWall = false;
        Debug.Log("出たよ");
    }
}
