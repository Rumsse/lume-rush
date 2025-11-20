using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public Transform planet;

    public float distance = 8f;
    public float heightOffset = 2f;

    public float sensitivityX = 120f;
    public float sensitivityY = 80f;

    private float yaw;
    private float pitch = 20f;

    void LateUpdate()
    {
        if (!target || !planet) return;

        yaw += Input.GetAxis("Mouse X") * sensitivityX * Time.deltaTime;
        pitch -= Input.GetAxis("Mouse Y") * sensitivityY * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, -10f, 80f);

        Vector3 gravityUp = (target.position - planet.position).normalized;

        Quaternion camRot = Quaternion.AngleAxis(yaw, gravityUp) *
                            Quaternion.AngleAxis(pitch, target.right);

        Vector3 camPos = target.position - camRot * Vector3.forward * distance + gravityUp * heightOffset;

        transform.position = camPos;
        transform.LookAt(target.position, gravityUp);
    }
}
