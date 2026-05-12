using tw.yuhan.MicrobitInputSystem;
using UnityEngine;

public class MicrobitInputObjTiltExample : MonoBehaviour
{
    Vector3 smoothAccel;

    void Update()
    {
        Vector3 accel = MicrobitInputDevice.current.acceleration.ReadValue();

        // 平滑濾波
        smoothAccel = Vector3.Lerp(
            smoothAccel,
            accel,
            Time.deltaTime * 8f
        );

        transform.rotation = Quaternion.Euler(
            smoothAccel.y * 90f,
            0f,
            -smoothAccel.x * 90f
        );
    }
}