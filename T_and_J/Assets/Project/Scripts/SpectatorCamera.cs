using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpectatorCamera : MonoBehaviour
{
    [SerializeField]
    FieldGenerator fieldGenerator;
    [Header("�J�����̃T�C�Y")]
    float sizeMultiplier = 0.5f;
    [SerializeField]
    GameObject mouseCamera;
    [SerializeField]
    GameObject catCamera;

    private Camera _camera;
    private int cameraAngle = 3;
    Vector3 centerPos;

    // Start is called before the first frame update
    void Start()
    {
        _camera = GetComponent<Camera>();
        AdjustCamera();
    }

    // Update is called once per frame
    void AdjustCamera()
    {
        int mapSize = fieldGenerator.mapSize;

        // �}�b�v�̒������v�Z�iFieldGenerator�Ɠ��� l = 0.5f�j
        float l = 0.5f;
        centerPos = new Vector3(mapSize * l / 2f, 10, mapSize * l / 2f);

        // �J�������}�b�v�̐^��ɔz�u
        changeCamera();

        // Orthographic�T�C�Y���}�b�v�T�C�Y�ɉ����Ē���
        if (_camera.orthographic)
        {
            _camera.orthographicSize = mapSize * l * sizeMultiplier;
        }
        else
        {
            _camera.fieldOfView = mapSize * l * sizeMultiplier * 10;
        }
    }

    public void changeCamera()
    {
        cameraAngle = (cameraAngle + 1) % 4;

        switch (cameraAngle)
        {
            case 0:
                mouseCamera.SetActive(false);
                gameObject.SetActive(true);
                transform.position = centerPos;
                transform.rotation = Quaternion.Euler(90f, 0f, 0f); // �^�ォ�猩���낷
                break;
            case 1:
                transform.position = new Vector3(centerPos.x, 4.6f, -4.56f);
                transform.rotation = Quaternion.Euler(30f, 0f, 0f); // �����猩���낷
                break;
            case 2:
                gameObject.SetActive(false);
                catCamera.SetActive(true);
                break;
            case 3:
                catCamera.SetActive(false);
                mouseCamera.SetActive(true);
                break;
        }
    }
}
