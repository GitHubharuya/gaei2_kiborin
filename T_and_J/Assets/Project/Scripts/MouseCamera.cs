using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform target;         // 追いかける対象（ネズミ）
    public Vector3 offset = new Vector3(0, 5, -7); // 高さと後ろからの距離
    public float smoothSpeed = 5f;   // カメラの追従速度

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        transform.position = smoothedPosition;
        transform.LookAt(target); // ネズミを見る
    }
}
