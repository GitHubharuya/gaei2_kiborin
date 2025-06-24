using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatFinish : MonoBehaviour
{
    [SerializeField]
    GameObject catCamera;

    public void beCollected()
    {
        Destroy(gameObject);
        Destroy(catCamera);
    }
}
