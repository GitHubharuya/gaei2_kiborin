using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EmergencyState : MonoBehaviour
{
    [Header("�ً}��Ԃ̍Œ�b��")]
    public float emergencyDuration = 5.0f;

    [Header("�ً}��Ԃ̈ړ����x")]
    public float emergencySpeed = 1.0f;
    public float emergencyRotationSpeed = 20f;

    [Header("�R�~�b�g������")]
    public float commitDuration = 0.6f;
    private bool isCommited = false;

    [Header("���E�ݒ�")]
    public float viewDistance = 6f; // ���E�̋���
    public float viewAngle = 90f;   // ����p�i���E45�x�j

    [Header("���̑�")]
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
