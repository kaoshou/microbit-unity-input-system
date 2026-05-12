using System.Globalization;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace tw.yuhan.MicrobitInputSystem
{
    /// <summary>
    /// 建立並更新 Unity Input System 的 micro:bit 自訂輸入裝置。
    /// 所有資料來源（BLE、USB Binary、USB CSV）最後都應呼叫 SubmitRawValues 或 SubmitLine。
    /// </summary>
    [DefaultExecutionOrder(-32000)]
    public class MicrobitInputRuntime : MonoBehaviour
    {
        [Header("micro:bit 原始加速度範圍")]
        [Tooltip("micro:bit acceleration 約為 -1024 到 1024。正規化後 Unity 端約為 -1 到 1。")]
        public float accelerationRange = 1024f;

        [Header("輸入修正")]
        public float deadZone = 0.08f;
        public bool invertX = false;
        public bool invertY = false;
        public bool invertZ = false;

        [Header("Debug")]
        public bool showDebugLog = false;

        private MicrobitInputDevice device;

        private void OnEnable()
        {
            InputSystem.RegisterLayout<MicrobitInputDevice>();
            if (device == null)
            {
                device = InputSystem.AddDevice<MicrobitInputDevice>();
                InputSystem.SetDeviceUsage(device, "Microbit");
                device.MakeCurrent();
            }
        }

        private void OnDisable()
        {
            if (device != null && device.added)
            {
                InputSystem.RemoveDevice(device);
                device = null;
            }
        }

        /// <summary>
        /// 給 CSV 文字模式使用，格式：x,y,z,a,b。
        /// </summary>
        public void SubmitLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return;

            string[] parts = line.Trim().Split(',');
            if (parts.Length != 5) return;

            if (!TryParseShort(parts[0], out short rawX)) return;
            if (!TryParseShort(parts[1], out short rawY)) return;
            if (!TryParseShort(parts[2], out short rawZ)) return;
            if (!TryParseInt(parts[3], out int a)) return;
            if (!TryParseInt(parts[4], out int b)) return;

            SubmitRawValues(rawX, rawY, rawZ, a, b);
        }

        /// <summary>
        /// 直接提交 micro:bit 原始 x/y/z 與 A/B 按鈕狀態。
        /// USB Binary 與 BLE 內建服務建議使用此方法。
        /// </summary>
        public void SubmitRawValues(short rawX, short rawY, short rawZ, int buttonA, int buttonB)
        {
            if (device == null) return;

            float x = Mathf.Clamp(rawX / accelerationRange, -1f, 1f);
            float y = Mathf.Clamp(rawY / accelerationRange, -1f, 1f);
            float z = Mathf.Clamp(rawZ / accelerationRange, -1f, 1f);

            if (invertX) x = -x;
            if (invertY) y = -y;
            if (invertZ) z = -z;

            if (Mathf.Abs(x) < deadZone) x = 0f;
            if (Mathf.Abs(y) < deadZone) y = 0f;
            if (Mathf.Abs(z) < deadZone) z = 0f;

            byte buttons = 0;
            if (buttonA != 0) buttons |= 1 << 0;
            if (buttonB != 0) buttons |= 1 << 1;

            var state = new MicrobitInputState
            {
                acceleration = new Vector3(x, y, z),
                buttons = buttons
            };

            // 直接改變 Input System state，比 QueueStateEvent 更即時，適合感測器連續資料。
            InputState.Change(device, state);

            if (showDebugLog)
            {
                Debug.Log($"micro:bit raw=({rawX},{rawY},{rawZ}) norm=({x:F2},{y:F2},{z:F2}) A={buttonA} B={buttonB}");
            }
        }

        public void SubmitNeutral()
        {
            SubmitRawValues(0, 0, 0, 0, 0);
        }

        private static bool TryParseShort(string text, out short value)
        {
            return short.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }

        private static bool TryParseInt(string text, out int value)
        {
            return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }
    }
}
