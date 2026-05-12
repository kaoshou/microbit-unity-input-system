using UnityEngine;
using tw.yuhan.MicrobitInputSystem;

public class MicrobitPlayerMoveExample : MonoBehaviour
{
    public float moveSpeed = 6f;
    public bool useX = true;

    private void Update()
    {
        var microbit = MicrobitInputDevice.current;
        if (microbit == null) return;

        Vector3 accel = microbit.acceleration.ReadValue();
        float move = useX ? accel.x : accel.y;
        transform.Translate(Vector3.right * move * moveSpeed * Time.deltaTime);

        if (microbit.buttonA.wasPressedThisFrame)
        {
            Debug.Log("micro:bit A pressed");
        }

        if (microbit.buttonB.wasPressedThisFrame)
        {
            Debug.Log("micro:bit B pressed");
        }
    }
}
