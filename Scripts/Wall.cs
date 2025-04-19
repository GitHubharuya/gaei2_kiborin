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
        //nameは各自設定したアクタ通りに設定してね
        if (other.name == "Sphere")
        {
            //ネズミに障害物と衝突した合図を送る
            mouse.ChangeBoolTrue();
        }
    }

    public void OnTriggerExit(Collider other)
    {
        //nameは各自設定したアクタ通りに設定してね
        if (other.name == "Sphere")
        {
            //ネズミに障害物から離れた合図を送る
            mouse.ChangeBoolFalse();
        }
    }
}
