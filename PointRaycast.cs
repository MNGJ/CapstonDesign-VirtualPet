using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointRaycast : MonoBehaviour
{
    public OVRHand rightHand;
    public OVRSkeleton rightSkeleton;

    void Update()
    {
        if (PlayerInput.Instance.Get_HandPose() == "Pointing")
        {
            ShootRayFromFinger();
        }
    }

    private void ShootRayFromFinger()
    {
        Transform indexFingerTip = rightSkeleton.Bones[(int)OVRSkeleton.BoneId.Hand_IndexTip].Transform;
        Vector3 rayOrigin = indexFingerTip.position;
        Vector3 rayDirection = indexFingerTip.forward;

        Ray ray = new Ray(rayOrigin, rayDirection);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo))
        {
            Debug.Log($"Hit object: {hitInfo.collider.gameObject.name}");
            // hitInfo를 사용하여 원하는 작업을 수행합니다.
        }
    }
}
