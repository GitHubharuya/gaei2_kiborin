using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cube : MonoBehaviour
{
    [SerializeField]
    GameObject player;

    private float chaseSpeed_ = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 dir = (player.transform.position - this.transform.position).normalized;
        //進む
        float vx = dir.x * chaseSpeed_;
        float vz = dir.z * chaseSpeed_;
        this.transform.Translate(vx / 50, 0, vz / 50);
    }
}
