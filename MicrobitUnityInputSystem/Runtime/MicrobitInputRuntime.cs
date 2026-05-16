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

        [Header("進階偵測 (Advanced)")]
        [Tooltip("搖晃感應門檻值 (動態加速度能量，建議 0.1 ~ 0.5)")]
        public float shakeThreshold = 0.15f;
        [Tooltip("揮動感應門檻值 (峰值 G 力，建議 0.8 ~ 3.0)")]
        public float swingThreshold = 1.0f;
        [Tooltip("傾斜判定門檻值 (重力分量，建議 0.3 ~ 0.6)")]
        public float tiltThreshold = 0.4f;
        [Tooltip("手勢觸發後持續時間 (秒)")]
        public float gestureDuration = 0.2f;

        private MicrobitInputDevice device;
        private Vector3 gravityFilter;
        private float shakeEnergy;
        private bool hasLastAcceleration = false;
        private float lastProcessTime = -1f;
        private float lastShakeTime = -1f;
        private float lastSwingTime = -1f;

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

            // 移除 Clamp，保留真實的 G 力大小以利於 Swing 偵測與搖晃能量計算
            float x = rawX / accelerationRange;
            float y = rawY / accelerationRange;
            float z = rawZ / accelerationRange;

            if (invertX) x = -x;
            if (invertY) y = -y;
            if (invertZ) z = -z;

            if (Mathf.Abs(x) < deadZone) x = 0f;
            if (Mathf.Abs(y) < deadZone) y = 0f;
            if (Mathf.Abs(z) < deadZone) z = 0f;

            Vector3 currentAccel = new Vector3(x, y, z);
            
            // 手勢偵測
            byte gestureFlags = DetectGestures(currentAccel);

            byte buttons = 0;
            if (buttonA != 0) buttons |= 1 << 0;
            if (buttonB != 0) buttons |= 1 << 1;

            var state = new MicrobitInputState
            {
                acceleration = currentAccel,
                buttons = buttons,
                gestures = gestureFlags
            };

            // 直接改變 Input System state，比 QueueStateEvent 更即時，適合感測器連續資料。
            InputState.Change(device, state);



            if (showDebugLog)
            {
                Debug.Log($"micro:bit raw=({rawX},{rawY},{rawZ}) norm=({x:F2},{y:F2},{z:F2}) A={buttonA} B={buttonB} Gestures={gestureFlags:X2}");
            }
        }

        private byte DetectGestures(Vector3 currentAccel)
        {
            float currentTime = Time.unscaledTime;
            if (!hasLastAcceleration)
            {
                gravityFilter = currentAccel;
                lastProcessTime = currentTime;
                hasLastAcceleration = true;
                return 0;
            }

            float dt = currentTime - lastProcessTime;
            lastProcessTime = currentTime;
            
            // 處理同幀內的多次呼叫 (Burst)，確保濾波器的時間一致性
            float safeDt = Mathf.Max(dt, 0.0001f);

            byte flags = 0;

            // 1. 分離重力與動態加速度
            // 使用時間常數約 0.2 秒的低通濾波 (5Hz)
            float gravityAlpha = 1.0f - Mathf.Exp(-5.0f * safeDt);
            gravityFilter = Vector3.Lerp(gravityFilter, currentAccel, gravityAlpha);
            
            // 動態加速度 (扣除重力)
            Vector3 linearAccel = currentAccel - gravityFilter;

            // 2. Shake (搖晃) 偵測
            // 使用時間常數約 0.1 秒的濾波累積能量
            float energyAlpha = 1.0f - Mathf.Exp(-10.0f * safeDt);
            shakeEnergy = Mathf.Lerp(shakeEnergy, linearAccel.sqrMagnitude, energyAlpha);
            
            if (shakeEnergy > shakeThreshold)
            {
                lastShakeTime = currentTime;
            }

            // 3. Swing (揮動) 偵測
            // 判斷瞬間的動態加速度峰值是否超過高 G 力門檻值
            if (linearAccel.magnitude > swingThreshold)
            {
                lastSwingTime = Time.unscaledTime;
            }

            // 維持動態手勢的觸發時間
            if (Time.unscaledTime - lastShakeTime < gestureDuration)
            {
                flags |= (1 << 0); // bit 0: shake
            }
            if (Time.unscaledTime - lastSwingTime < gestureDuration)
            {
                flags |= (1 << 1); // bit 1: swing
            }

            // 4. 靜態姿態 (Tilt / Face) 偵測
            // 使用濾波後的重力向量，確保不受瞬間晃動影響，並且保證互斥 (僅取最大分量)
            float absX = Mathf.Abs(gravityFilter.x);
            float absY = Mathf.Abs(gravityFilter.y);
            float absZ = Mathf.Abs(gravityFilter.z);

            if (absX > absY && absX > absZ && absX > tiltThreshold)
            {
                if (gravityFilter.x < 0) flags |= (1 << 2); // tiltLeft
                else flags |= (1 << 3); // tiltRight
            }
            else if (absY > absX && absY > absZ && absY > tiltThreshold)
            {
                if (gravityFilter.y < 0) flags |= (1 << 4); // tiltUp
                else flags |= (1 << 5); // tiltDown
            }
            else if (absZ > absX && absZ > absY && absZ > tiltThreshold)
            {
                if (gravityFilter.z < 0) flags |= (1 << 6); // faceUp
                else flags |= (1 << 7); // faceDown
            }

            return flags;
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
