using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using static UnityEngine.UI.Image;

public class ObstacleAvoidance : MonoBehaviour
{
    public float detectionRange = 0.2f; //��Q�����o����
    public float avoidDistance = 0.7f; //��Q�����̂��߂̍��E���o����

    public float moveSpeed = 0.5f;
    public float rotationSpeed = 10f; //�������񑬓x

    public LayerMask obstacleLayer;

    private int cheeseCount = 0; // �� �`�[�Y�̐���J�E���g
    public TextMeshProUGUI cheeseText;      // �� UI�ւ̎Q�ƁiInspector�Őݒ�j

    public Transform cheese;
    public bool useView = false;
    private Rigidbody rb; //Rigidbody�R���|�[�l���g��i�[����ϐ�

    public float commitDuration = 0.6f; // ��𓮍�̎�������
    private bool isCommitted = false;

    [Header("視界設定")]
    public float viewDistance = 6f; // 視界の距離
    public float viewAngle = 90f;   // 視野角（左右45度）

    private void Start()
    {
        UpdateCheeseUI();
        rb = GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    void Update()
    {

    }

    private void FixedUpdate()
    {
        if (!isCommitted)
        {
            //Debug.Log(isCommitted);
            bool flag = AvoidObstaclesAndMove();
            if (!flag)
            {
                if (cheese == null)
                {
                    if (useView)
                    {
                        FindNearestCheese_View(); //視界を使ってチーズを探す
                        Quaternion mouseRotation = Quaternion.LookRotation(-transform.forward);
                        transform.rotation = Quaternion.Slerp(transform.rotation, mouseRotation, rotationSpeed * Time.deltaTime);
                        rb.MovePosition(rb.position + -transform.forward * moveSpeed * Time.fixedDeltaTime);
                    }
                    else
                    {
                        FindNearestCheese(); //視界を使わずにチーズを探す 
                    }
                }

                if (cheese != null)
                {
                    Vector3 direction = cheese.position - transform.position;
                    direction.Normalize();
                    //transform.position += direction * speed * Time.deltaTime;
                    Quaternion mouseRotaion = Quaternion.LookRotation(direction); //�l�Y�~�̌�����v���C���[�����Ɍ�����
                    transform.rotation = Quaternion.Slerp(transform.rotation, mouseRotaion, rotationSpeed * Time.deltaTime); //�l�Y�~�̌�������炩�ɕς���
                    rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime); //Rigidbody��g���Ĉړ�
                }
            }
        }
    }



    public bool AvoidObstaclesAndMove()
    {
        bool facingObstacle = false;
        Vector3 origin = transform.position; //���݂̃l�Y�~�ʒu��擾
        Vector3 forwardDir = transform.forward; //�l�Y�~���_���猩�đO�̕����x�N�g��
        Vector3 upwardDirection = (forwardDir + Vector3.up).normalized; //�l�Y�~���_���猩�ď�̕����x�N�g��
        Vector3 dir1 = Quaternion.Euler(0, 30, 0) * transform.forward;
        Vector3 dir2 = Quaternion.Euler(0, -30, 0) * transform.forward;
        Vector3 dir3 = Quaternion.Euler(0, 60, 0) * transform.forward;
        Vector3 dir4 = Quaternion.Euler(0, -60, 0) * transform.forward;
        Vector3 dir5 = Quaternion.Euler(0, 120, 0) * transform.forward;
        Vector3 dir6 = Quaternion.Euler(0, -120, 0) * transform.forward;
        Vector3 dir7 = Quaternion.Euler(0, 150, 0) * transform.forward;
        Vector3 dir8 = Quaternion.Euler(0, -150, 0) * transform.forward;
        Vector3[] directions = new Vector3[] { -transform.right, transform.right, dir1, dir2, dir3, dir4, dir5, dir6, dir7, dir8 };
        // �ǂ��ɓ����邱�Ƃ��ł���̂��A����Ŕ��f

        bool inFront = false;
        bool above = false;

        Debug.DrawRay(origin, forwardDir * detectionRange, Color.red); //Ray�̉���
        Debug.DrawRay(origin, upwardDirection * detectionRange, Color.red); //Ray�̉���
        Debug.DrawRay(origin, -transform.right * avoidDistance, Color.green);
        Debug.DrawRay(origin, transform.right * avoidDistance, Color.green);
        Debug.DrawRay(origin, dir1 * avoidDistance, Color.green);
        Debug.DrawRay(origin, dir2 * avoidDistance, Color.green);
        Debug.DrawRay(origin, dir3 * avoidDistance, Color.green);
        Debug.DrawRay(origin, dir4 * avoidDistance, Color.green);
        Debug.DrawRay(origin, dir5 * avoidDistance, Color.green);
        Debug.DrawRay(origin, dir6 * avoidDistance, Color.green);
        Debug.DrawRay(origin, dir7 * avoidDistance, Color.green);
        Debug.DrawRay(origin, dir8 * avoidDistance, Color.green);

        if (Physics.Raycast(origin, forwardDir, out RaycastHit hitinfo, detectionRange, obstacleLayer))
        {
            if (hitinfo.collider.CompareTag("Obstacle"))
            {
                //Debug.Log("��Q���ɐڐG");
                inFront = true;
            }

        }
        if (Physics.Raycast(origin, upwardDirection, out RaycastHit hitinfo2, detectionRange, obstacleLayer))
        {
            if (hitinfo2.collider.CompareTag("Obstacle"))
            {
                //Debug.Log("��Q���ɐڐG");
                above = true;
            }
        }

        if (inFront || above)
        {
            facingObstacle = true;
            mouseMovement(origin, forwardDir, directions); //��Q�����O���ɂ���ꍇ�͉�������Ăяo��
        }


        //transform.position += transform.forward * moveSpeed * Time.deltaTime;

        return facingObstacle; //��Q���ɐڐG���Ă��邩�ǂ�����Ԃ�
    }


    // Update is called once per frame
    public void mouseMovement(Vector3 origin, Vector3 forwardDir, Vector3[] directions)
    {
        bool clearLeft = !Physics.Raycast(origin, directions[0], avoidDistance, obstacleLayer); //�l�Y�~�������ɏ�Q�����Ȃ����T��
        bool clearRight = !Physics.Raycast(origin, directions[1], avoidDistance, obstacleLayer); //��Ɠ��l(�Ever)
        bool clearDir1 = !Physics.Raycast(origin, directions[2], avoidDistance, obstacleLayer); //30�x�E�����ɏ�Q�����Ȃ����T��
        bool clearDir2 = !Physics.Raycast(origin, directions[3], avoidDistance, obstacleLayer); //30�x�������ɏ�Q�����Ȃ����T��
        bool clearDir3 = !Physics.Raycast(origin, directions[4], avoidDistance, obstacleLayer); //60�x�E�����ɏ�Q�����Ȃ����T��
        bool clearDir4 = !Physics.Raycast(origin, directions[5], avoidDistance, obstacleLayer); //60�x�������ɏ�Q�����Ȃ����T��
        bool clearDir5 = !Physics.Raycast(origin, directions[6], avoidDistance, obstacleLayer); //120�x�E�����ɏ�Q�����Ȃ����T��
        bool clearDir6 = !Physics.Raycast(origin, directions[7], avoidDistance, obstacleLayer); //120�x�������ɏ�Q�����Ȃ����T��
        bool clearDir7 = !Physics.Raycast(origin, directions[8], avoidDistance, obstacleLayer); //150�x�E�����ɏ�Q�����Ȃ����T��
        bool clearDir8 = !Physics.Raycast(origin, directions[9], avoidDistance, obstacleLayer); //150�x�������ɏ�Q�����Ȃ����T��
        List<Vector3> clearDirections = new List<Vector3>(); //��Q�����Ȃ�������i�[���郊�X�g

        foreach (Vector3 dir in directions)
        {
            if (!Physics.Raycast(origin, dir, avoidDistance, obstacleLayer))
            {
                clearDirections.Add(dir);
            }
        }

        Vector3 desiredDirection = Vector3.zero; //����������̃x�N�g���͂����Ɋi�[���܁[��

        if (clearDirections.Count > 0) {
            Debug.Log("��Q���Ȃ��̕��������������A�����܂�");
            int randomIndex = Random.Range(0, clearDirections.Count); //��Q�����Ȃ������̒����烉���_���őI��
            desiredDirection = clearDirections[randomIndex];
        }
        else
        {
            Debug.Log("��Q���Ȃ��̕�����������Ȃ��A��ނ��܂�");
            desiredDirection = -forwardDir; //��Q�����Ȃ�������������Ȃ���������
        }

        /*if (clearLeft && clearRight) //�E�ƍ�������Q�����Ȃ��Ƃ���
        {
            Debug.Log("��Q���Ȃ��A�����_���ŉ����������");
            desiredDirection = Random.value < 0.5f ? -transform.right : transform.right; //�����_���ō����E�ɐi��
        }
        else if (clearLeft)//����Q���Ȃ�
        {
            Debug.Log("���ɉ��");
            desiredDirection = -transform.right; //���ɐi��
        }
        else if (clearRight)//�E��Q���Ȃ�
        {
            Debug.Log("�E�ɉ��");
            desiredDirection = transform.right; //�E�ɐi��
        }
        else//3�����l��
        {
            Debug.Log("3�����l�݁A���");
            desiredDirection = -forwardDir; //���
        }
        */

        //��]����炩�ɂ��鏈��
        Quaternion targetRotation = Quaternion.LookRotation(desiredDirection, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        //��]���f��͑O�i!!!
        StartCoroutine(CommitMovement(desiredDirection, commitDuration));

    }

    void UpdateCheeseUI()
    {
        if (cheeseText != null)
        {
            cheeseText.text = "Cheese: " + cheeseCount.ToString();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Cheese"))
        {
            Destroy(other.gameObject);
            cheeseCount++;
            UpdateCheeseUI();
            cheese = null;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        Vector3 forward = transform.forward;
        Quaternion leftRayRotation = Quaternion.Euler(0, -viewAngle / 2, 0);
        Quaternion rightRayRotation = Quaternion.Euler(0, viewAngle / 2, 0);

        Vector3 leftRay = leftRayRotation * forward;
        Vector3 rightRay = rightRayRotation * forward;

        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, leftRay * viewDistance);
        Gizmos.DrawRay(transform.position, rightRay * viewDistance);
    }

    void FindNearestCheese()
    {
        GameObject[] cheeses = GameObject.FindGameObjectsWithTag("Cheese");
        float closestDistance = Mathf.Infinity;
        Transform closest = null;

        foreach (GameObject cheese in cheeses)
        {
            float distance = Vector3.Distance(transform.position, cheese.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = cheese.transform;
            }
        }

        cheese = closest;
    }

    void FindNearestCheese_View()
    {
        GameObject[] cheeses = GameObject.FindGameObjectsWithTag("Cheese");
        float closestDistance = Mathf.Infinity;
        Transform closest = null;

        foreach (GameObject c in cheeses)
        {
            Vector3 toCheese = c.transform.position - transform.position;
            float distance = toCheese.magnitude;

            if (distance > viewDistance) continue; // 視界の距離外

            float angle = Vector3.Angle(transform.forward, toCheese);
            if (angle > viewAngle / 2f) continue; // 視野角外

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = c.transform;
            }
        }

        cheese = closest;
    }

    private IEnumerator CommitMovement(Vector3 direction, float commitTime)
    {
        isCommitted = true;
        float startTime = Time.time;

        Quaternion targetRotaion = Quaternion.LookRotation(direction, Vector3.up); //�������Ɍ�����

        while (Time.time < startTime + commitTime)
        {
            rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotaion, rotationSpeed * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }
        isCommitted = false;
    }
}




