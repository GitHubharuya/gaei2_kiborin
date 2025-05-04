using System.Threading;
using UnityEngine;

public class MouthSearch : MonoBehaviour
{
    public float angle = 45f;

  private void OnTriggerStay(Collider other)
  {
    if  (other.gameObject.tag == "Mouth")  //視界の範囲内の当たり判定
    {
        //視界の角度内に収まっているか
        Vector3 posDelta = other.transform.position - this.transform.position;
        float target_angle = Vector3.Angle(this.transform.forward, posDelta);

        if  (target_angle < angle)
        {
            if(Physics.Raycast(this.transform.position, posDelta, out RaycastHit hit))  //Rayを使用してtargetに当たっているか判定
            {
                if  (hit.collider==other)
                {
                    Debug.Log("range of view");
                }
            }
        }
    }
  }
}