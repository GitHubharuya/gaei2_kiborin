using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ObstacleAvoidance : MonoBehaviour
{
    public float detectionRange = 1.0f; //��Q�����o����
    public float avoidDistance = 2f; //��Q�����̂��߂̍��E���o����

    public float moveSpeed = 0.5f;
    public float rotationSpeed = 5f; //�������񑬓x

    public LayerMask obstacleLayer;

    public Transform player;

    private int cheeseCount = 0; // �� �`�[�Y�̐���J�E���g
    public TextMeshProUGUI cheeseText;      // �� UI�ւ̎Q�ƁiInspector�Őݒ�j

    private void Start()
    {
        
    }

    void Update()
    {
        
            AvoidObstaclesAndMove();
    }

 

    void AvoidObstaclesAndMove()
    {
        Vector3 origin = transform.position; //���݂̃l�Y�~�ʒu��擾
        Vector3 forwardDir = transform.forward; //�l�Y�~���_���猩�đO�̕����x�N�g��
        Vector3 upwardDirection = (forwardDir + Vector3.up).normalized; //�l�Y�~���_���猩�ď�̕����x�N�g��

        bool inFront = false;
        bool above = false;
        Debug.DrawRay(origin, forwardDir * detectionRange, Color.red); //Ray�̉���
        Debug.DrawRay(origin, upwardDirection * detectionRange, Color.blue); //Ray�̉���

        if (Physics.Raycast(origin, forwardDir, out RaycastHit hitinfo, detectionRange, obstacleLayer))
        {
            if (hitinfo.collider.CompareTag("Obstacle"))
            {
                Debug.Log("��Q���ɐڐG");
                inFront = true;
            }         
            
        }
        if(Physics.Raycast(origin, upwardDirection, out RaycastHit hitinfo2, detectionRange, obstacleLayer))
        {
            if (hitinfo2.collider.CompareTag("Obstacle"))
            {
                Debug.Log("��Q���ɐڐG");
                above = true;
            }
        }

        if (inFront || above)
        {
            bool clearLeft = !Physics.Raycast(origin, -transform.right, avoidDistance, obstacleLayer); //�l�Y�~�������ɏ�Q�����Ȃ����T��
            bool clearRight = !Physics.Raycast(origin, transform.right, avoidDistance, obstacleLayer); //��Ɠ��l(�Ever)

            Vector3 desiredDirection = Vector3.zero; //����������̃x�N�g���͂����Ɋi�[���܁[��
            if (clearLeft && clearRight) //�E�ƍ�������Q�����Ȃ��Ƃ���
            {
                desiredDirection = Random.value < 0.5f ? -transform.right : transform.right; //�����_���ō����E�ɐi��
            }
            else if (clearLeft)//����Q���Ȃ�
            {
                desiredDirection = -transform.right; //���ɐi��
            }
            else if (clearRight)//�E��Q���Ȃ�
            {
                desiredDirection = transform.right; //�E�ɐi��
            }
            else//3�����l��
            {
                desiredDirection = -forwardDir; //���
            }
            //��]����炩�ɂ��鏈��
            Quaternion targetRotation = Quaternion.LookRotation(desiredDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        //��]���f��͑O�i!!!
        transform.position += transform.forward * moveSpeed * Time.deltaTime;
    }


    // Update is called once per frame
    
}
