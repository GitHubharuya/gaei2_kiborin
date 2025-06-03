using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EmergencyState : MonoBehaviour
{
    [Header("‹Ù‹}ó‘Ô‚ÌÅ’á•b”")]
    public float emergencyDuration = 5.0f;

    [Header("‹Ù‹}ó‘Ô‚ÌˆÚ“®‘¬“x")]
    public float emergencySpeed = 1.0f;
    public float emergencyRotationSpeed = 20f;

    [Header("ƒRƒ~ƒbƒgˆ—")]
    public float commitDuration = 0.6f;
    private bool isCommited = false;

    [Header("‹ŠEİ’è")]
    public float viewDistance = 6f; // ‹ŠE‚Ì‹——£
    public float viewAngle = 90f;   // ‹–ìŠpi¶‰E45“xj

    [Header("‚»‚Ì‘¼")]
    public LayerMask obstacleLayer;
    public bool useView = false;
    public Transform cheese;
    private Rigidbody rb;
    public TextMeshProUGUI cheeseText;
    private int cheeseCount = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
