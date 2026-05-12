using UnityEngine;
using tw.yuhan.MicrobitInputSystem;

public class MicrobitInputDebugText : MonoBehaviour
{
    private void OnGUI()
    {
        var microbit = MicrobitInputDevice.current;
        if (microbit == null)
        {
            GUILayout.Label("micro:bit Input Device: not ready");
            return;
        }

        Vector3 accel = microbit.acceleration.ReadValue();
        GUILayout.Label($"acceleration: {accel.x:F2}, {accel.y:F2}, {accel.z:F2}");
        GUILayout.Label($"A: {microbit.buttonA.isPressed}, B: {microbit.buttonB.isPressed}");
    }
}
