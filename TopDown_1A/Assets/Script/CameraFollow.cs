using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("추적할 대상")]
    public Transform target;

    [Header("거리 설정")]
    public Vector3 offset = new Vector3(0f, 5f, -7f);

    [Header("부드러운 이동 속도")]
    public float smoothSpeed = 0.125f;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 disiredPosition = target.position + offset;

        Vector3 smoothedPosition = Vector3.Lerp(transform.position, disiredPosition, smoothSpeed);

        transform.position = smoothedPosition;

        //transform.LookAt(target);
    }
}
