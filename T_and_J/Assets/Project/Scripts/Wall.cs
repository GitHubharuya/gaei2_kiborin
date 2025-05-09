using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collision : MonoBehaviour
{

    [SerializeField]
    Mouse mouse;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.name == "Mouse")
        {
            //ネズミに障害物と衝突した合図を送る
            mouse.changeBoolTrue();
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.name == "Mouse")
        {
            //ネズミに障害物から離れた合図を送る
            mouse.changeBoolFalse();
        }
    }
}
