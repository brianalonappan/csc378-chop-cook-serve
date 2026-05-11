using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    void LateUpdate()
    {
        if (target != null)
        {
            Vector3 targetPosition = new Vector3(
                target.position.x,
                target.position.y,
                transform.position.z
            );

            transform.position = targetPosition;
        }
    }
}