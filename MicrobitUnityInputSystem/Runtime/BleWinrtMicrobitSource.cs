using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace tw.yuhan.MicrobitInputSystem
{
    /// <summary>
    /// BLE 無線模式：使用 BleWinrtDll 讀取 micro:bit 內建 Accelerometer Service 與 Button Service。
    /// 本檔不需要 Scripting Define Symbol。若未安裝 BleWinrtDll，Unity 仍可編譯，Play 時會提示安裝。
    /// </summary>
    [DefaultExecutionOrder(-32000)]
    public class BleWinrtMicrobitSource : MonoBehaviour
    {
        private enum State
        {
            Idle,
            ScanningDevices,
            ScanningServices,
            ScanningAccelerometerCharacteristics,
            ScanningButtonCharacteristics,
            Subscribed,
            Failed
        }

        [Header("Target Runtime")]
        public MicrobitInputRuntime runtime;

        [Header("Connection")]
        public bool connectOnStart = true;
        public string deviceNameFilter = "micro:bit";
        public string specificDeviceId = "";
        public bool autoReconnect = true;
        public float reconnectDelaySeconds = 3f;

        [Header("micro:bit Accelerometer Service")]
        public string accelerometerServiceUuid = "e95d0753-251d-470a-a062-fa1922dfa9a8";
        public string accelerometerDataUuid = "e95dca4b-251d-470a-a062-fa1922dfa9a8";

        [Header("micro:bit Button Service")]
        public string buttonServiceUuid = "e95d9882-251d-470a-a062-fa1922dfa9a8";
        public string buttonAStateUuid = "e95dda90-251d-470a-a062-fa1922dfa9a8";
        public string buttonBStateUuid = "e95dda91-251d-470a-a062-fa1922dfa9a8";

        [Header("Polling Limits")]
        public int maxDevicePollsPerFrame = 20;
        public int maxServicePollsPerFrame = 20;
        public int maxCharacteristicPollsPerFrame = 20;
        public int maxDataPollsPerFrame = 20;

        [Header("Subscribe")]
        [Tooltip("若非同步訂閱不穩，可勾選。勾選時 Play 初期可能短暫卡一下，但比較容易確認訂閱結果。")]
        public bool subscribeBlock = false;

        [Header("Reconnect Watchdog")]
        public bool reconnectIfNoData = true;
        public float reconnectIfNoDataSeconds = 5f;

        [Header("Debug")]
        public bool showDebugLog = false;
        public bool logAllScannedDevices = false;
        public bool logAllServices = false;
        public bool logAllCharacteristics = false;

        private State state = State.Idle;
        private BleWinrtReflectionApi ble;
        private bool bleStarted;

        private string targetDeviceId = "";
        private string targetDeviceName = "";

        private bool foundAccelerometerService;
        private bool foundButtonService;
        private bool subscribedAccelerometer;
        private bool subscribedButtonA;
        private bool subscribedButtonB;

        private float nextReconnectTime;
        private float lastDataTime;

        private short latestRawX;
        private short latestRawY;
        private short latestRawZ;
        private int latestButtonA;
        private int latestButtonB;

        private bool warnedAccelerometerSizeMismatch;
        private bool warnedButtonSizeMismatch;

        private readonly HashSet<string> loggedDeviceIds = new HashSet<string>();
        private readonly HashSet<string> loggedServices = new HashSet<string>();
        private readonly HashSet<string> loggedCharacteristics = new HashSet<string>();

        private void Start()
        {
            if (runtime == null) runtime = FindFirstObjectByType<MicrobitInputRuntime>();
            if (connectOnStart) Connect();
        }

        private void Update()
        {
            TickBle();
        }

        private void OnDisable() => ShutdownBleApi();
        private void OnApplicationQuit() => ShutdownBleApi();

        public void Connect()
        {
            ResetInternalState();

            if (runtime == null) runtime = FindFirstObjectByType<MicrobitInputRuntime>();

            ble = new BleWinrtReflectionApi(showDebugLog);
            if (!ble.IsAvailable)
            {
                Fail("找不到 BleApi。請先安裝 BleWinrtDll：Package Manager → Add package from git URL → https://github.com/adabru/BleWinrtDll.git?path=/BleWinrtDll-UnityPackage");
                return;
            }

            try
            {
                ble.StartDeviceScan();
                bleStarted = true;
                state = State.ScanningDevices;
                if (showDebugLog) Debug.Log("Started BLE scan for micro:bit built-in services.");
            }
            catch (Exception ex)
            {
                Fail("StartDeviceScan failed: " + ex.Message);
            }
        }

        public void Disconnect()
        {
            ShutdownBleApi();
            state = State.Idle;
        }

        private void TickBle()
        {
            switch (state)
            {
                case State.Idle:
                    if (autoReconnect && Time.unscaledTime >= nextReconnectTime) Connect();
                    break;
                case State.ScanningDevices:
                    PollDevices();
                    break;
                case State.ScanningServices:
                    PollServices();
                    break;
                case State.ScanningAccelerometerCharacteristics:
                    PollAccelerometerCharacteristics();
                    break;
                case State.ScanningButtonCharacteristics:
                    PollButtonCharacteristics();
                    break;
                case State.Subscribed:
                    PollData();
                    if (reconnectIfNoData && Time.unscaledTime - lastDataTime > reconnectIfNoDataSeconds)
                    {
                        Debug.LogWarning("BLE 已完成訂閱但一段時間未收到資料，將重新連線。若經常發生，請 Reset micro:bit 或重開 Windows 藍牙。");
                        ShutdownBleApi();
                        nextReconnectTime = Time.unscaledTime + reconnectDelaySeconds;
                        state = State.Failed;
                    }
                    break;
                case State.Failed:
                    if (autoReconnect && Time.unscaledTime >= nextReconnectTime) Connect();
                    break;
            }
        }

        private void PollDevices()
        {
            int count = 0;
            while (count < maxDevicePollsPerFrame && ble.PollDevice(out var device))
            {
                count++;
                string deviceName = device.name ?? "";
                string deviceId = device.id ?? "";

                if (logAllScannedDevices && showDebugLog && !string.IsNullOrEmpty(deviceId) && loggedDeviceIds.Add(deviceId))
                {
                    Debug.Log($"BLE device: {deviceName} / {deviceId}");
                }

                if (IsTargetDevice(deviceName, deviceId))
                {
                    targetDeviceName = deviceName;
                    targetDeviceId = deviceId;
                    if (showDebugLog) Debug.Log($"已找到 micro:bit BLE 裝置：{targetDeviceName} / {targetDeviceId}");
                    StopDeviceScanSafely();
                    StartServiceScan();
                    return;
                }
            }
        }

        private bool IsTargetDevice(string deviceName, string deviceId)
        {
            if (!string.IsNullOrWhiteSpace(specificDeviceId) &&
                string.Equals(deviceId, specificDeviceId, StringComparison.OrdinalIgnoreCase)) return true;

            if (string.IsNullOrWhiteSpace(deviceName)) return false;
            if (!string.IsNullOrWhiteSpace(deviceNameFilter) &&
                deviceName.IndexOf(deviceNameFilter, StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (deviceName.IndexOf("micro:bit", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (deviceName.IndexOf("BBC micro", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            return false;
        }

        private void StartServiceScan()
        {
            try
            {
                loggedServices.Clear();
                foundAccelerometerService = false;
                foundButtonService = false;
                state = State.ScanningServices;
                if (showDebugLog) Debug.Log("開始掃描 micro:bit Services：Accelerometer + Button");
                ble.ScanServices(targetDeviceId);
            }
            catch (Exception ex)
            {
                Fail("ScanServices failed: " + ex.Message);
            }
        }

        private void PollServices()
        {
            int count = 0;
            while (count < maxServicePollsPerFrame && ble.PollService(out var service))
            {
                count++;
                string serviceUuid = NormalizeUuid(service.uuid);
                if (logAllServices && showDebugLog && loggedServices.Add(serviceUuid)) Debug.Log($"BLE service: {serviceUuid}");

                if (UuidEquals(serviceUuid, accelerometerServiceUuid))
                {
                    foundAccelerometerService = true;
                    if (showDebugLog) Debug.Log($"找到 Accelerometer Service：{serviceUuid}");
                }
                if (UuidEquals(serviceUuid, buttonServiceUuid))
                {
                    foundButtonService = true;
                    if (showDebugLog) Debug.Log($"找到 Button Service：{serviceUuid}");
                }
            }

            if (ble.LastPollFinished)
            {
                if (!foundAccelerometerService)
                {
                    Fail($"找不到 Accelerometer Service：{accelerometerServiceUuid}");
                    return;
                }
                if (!foundButtonService && showDebugLog) Debug.LogWarning($"找不到 Button Service：{buttonServiceUuid}。將只使用加速度資料。");
                StartAccelerometerCharacteristicScan();
            }
        }

        private void StartAccelerometerCharacteristicScan()
        {
            try
            {
                loggedCharacteristics.Clear();
                subscribedAccelerometer = false;
                state = State.ScanningAccelerometerCharacteristics;
                if (showDebugLog) Debug.Log($"開始掃描 Accelerometer Characteristics：{accelerometerDataUuid}");
                ble.ScanCharacteristics(targetDeviceId, NormalizeUuid(accelerometerServiceUuid));
            }
            catch (Exception ex)
            {
                Fail("ScanCharacteristics accelerometer failed: " + ex.Message);
            }
        }

        private void PollAccelerometerCharacteristics()
        {
            int count = 0;
            while (count < maxCharacteristicPollsPerFrame && ble.PollCharacteristic(out var characteristic))
            {
                count++;
                string uuid = NormalizeUuid(characteristic.uuid);
                if (logAllCharacteristics && showDebugLog && loggedCharacteristics.Add(uuid))
                {
                    Debug.Log($"Accelerometer characteristic: {uuid} / {characteristic.userDescription}");
                }

                if (UuidEquals(uuid, accelerometerDataUuid))
                {
                    subscribedAccelerometer = Subscribe(NormalizeUuid(accelerometerServiceUuid), uuid, "Accelerometer Data");
                }
            }

            if (ble.LastPollFinished)
            {
                if (!subscribedAccelerometer)
                {
                    Fail($"找不到或未能訂閱 Accelerometer Data：{accelerometerDataUuid}。可嘗試勾選 Subscribe Block、Reset micro:bit、重開 Unity。");
                    return;
                }
                if (foundButtonService) StartButtonCharacteristicScan();
                else EnterSubscribedState();
            }
        }

        private void StartButtonCharacteristicScan()
        {
            try
            {
                loggedCharacteristics.Clear();
                subscribedButtonA = false;
                subscribedButtonB = false;
                state = State.ScanningButtonCharacteristics;
                if (showDebugLog) Debug.Log($"開始掃描 Button Characteristics：A={buttonAStateUuid}, B={buttonBStateUuid}");
                ble.ScanCharacteristics(targetDeviceId, NormalizeUuid(buttonServiceUuid));
            }
            catch (Exception ex)
            {
                Fail("ScanCharacteristics button failed: " + ex.Message);
            }
        }

        private void PollButtonCharacteristics()
        {
            int count = 0;
            while (count < maxCharacteristicPollsPerFrame && ble.PollCharacteristic(out var characteristic))
            {
                count++;
                string uuid = NormalizeUuid(characteristic.uuid);
                if (logAllCharacteristics && showDebugLog && loggedCharacteristics.Add(uuid))
                {
                    Debug.Log($"Button characteristic: {uuid} / {characteristic.userDescription}");
                }

                if (UuidEquals(uuid, buttonAStateUuid))
                {
                    subscribedButtonA = Subscribe(NormalizeUuid(buttonServiceUuid), uuid, "Button A State");
                }
                else if (UuidEquals(uuid, buttonBStateUuid))
                {
                    subscribedButtonB = Subscribe(NormalizeUuid(buttonServiceUuid), uuid, "Button B State");
                }
            }

            if (ble.LastPollFinished)
            {
                if (!subscribedButtonA && showDebugLog) Debug.LogWarning("未訂閱 Button A State，仍可使用加速度。可嘗試勾選 Subscribe Block。");
                if (!subscribedButtonB && showDebugLog) Debug.LogWarning("未訂閱 Button B State，仍可使用加速度。可嘗試勾選 Subscribe Block。");
                EnterSubscribedState();
            }
        }

        private bool Subscribe(string serviceUuid, string characteristicUuid, string label)
        {
            try
            {
                if (showDebugLog) Debug.Log($"開始訂閱 {label}: service={serviceUuid}, characteristic={characteristicUuid}, block={subscribeBlock}");
                bool result = ble.SubscribeCharacteristic(targetDeviceId, serviceUuid, characteristicUuid, subscribeBlock);
                if (showDebugLog) Debug.Log($"Subscribe {label} returned: {result}");
                if (!result)
                {
                    Debug.LogWarning($"Subscribe {label} 未確認成功。若無資料，請勾選 Subscribe Block 或重置藍牙連線。");
                }
                // 非阻塞模式有時回傳 false 但後續仍可能收到資料，因此只在 block 模式嚴格判定。
                return subscribeBlock ? result : true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Subscribe {label} failed: {ex.Message}");
                return false;
            }
        }

        private void EnterSubscribedState()
        {
            state = State.Subscribed;
            lastDataTime = Time.unscaledTime;
            if (showDebugLog) Debug.Log("已完成 micro:bit 內建服務訂閱，開始接收資料。");
        }

        private void PollData()
        {
            int count = 0;
            while (count < maxDataPollsPerFrame && ble.PollData(out var data))
            {
                count++;
                byte[] buffer = data.buf;
                if (buffer == null || buffer.Length == 0) continue;

                string serviceUuid = NormalizeUuid(data.serviceUuid);
                string characteristicUuid = NormalizeUuid(data.characteristicUuid);

                if (UuidEquals(serviceUuid, accelerometerServiceUuid) && UuidEquals(characteristicUuid, accelerometerDataUuid))
                {
                    HandleAccelerometerData(buffer, data.size);
                }
                else if (UuidEquals(serviceUuid, buttonServiceUuid) && UuidEquals(characteristicUuid, buttonAStateUuid))
                {
                    HandleButtonData(buffer, data.size, true);
                }
                else if (UuidEquals(serviceUuid, buttonServiceUuid) && UuidEquals(characteristicUuid, buttonBStateUuid))
                {
                    HandleButtonData(buffer, data.size, false);
                }
            }
        }

        private void HandleAccelerometerData(byte[] buffer, int size)
        {
            const int requiredBytes = 6;
            if (buffer == null || buffer.Length < requiredBytes) return;

            // BleWinrtDll 在部分情況下 data.size 可能小於實際 buffer 長度。
            // micro:bit Accelerometer Data 是固定 6 bytes，因此以 buffer.Length 做有效性判斷。
            if (size < requiredBytes && showDebugLog && !warnedAccelerometerSizeMismatch)
            {
                warnedAccelerometerSizeMismatch = true;
                Debug.LogWarning($"Accelerometer data.size={size} 小於 6，但 buffer.Length={buffer.Length}。將依固定 6 bytes 解析。");
            }

            latestRawX = ReadInt16LittleEndian(buffer, 0);
            latestRawY = ReadInt16LittleEndian(buffer, 2);
            latestRawZ = ReadInt16LittleEndian(buffer, 4);
            lastDataTime = Time.unscaledTime;
            SubmitCurrentState();

            if (showDebugLog) Debug.Log($"BLE Accelerometer: x={latestRawX}, y={latestRawY}, z={latestRawZ}");
        }

        private void HandleButtonData(byte[] buffer, int size, bool isButtonA)
        {
            const int requiredBytes = 1;
            if (buffer == null || buffer.Length < requiredBytes) return;

            if (size < requiredBytes && showDebugLog && !warnedButtonSizeMismatch)
            {
                warnedButtonSizeMismatch = true;
                Debug.LogWarning($"Button data.size={size} 小於 1，但 buffer.Length={buffer.Length}。將依固定 1 byte 解析。");
            }

            int stateValue = buffer[0]; // 0=not pressed, 1=pressed, 2=long pressed
            if (isButtonA) latestButtonA = stateValue != 0 ? 1 : 0;
            else latestButtonB = stateValue != 0 ? 1 : 0;

            lastDataTime = Time.unscaledTime;
            SubmitCurrentState();
            if (showDebugLog) Debug.Log($"BLE Button {(isButtonA ? "A" : "B")}: raw={stateValue}");
        }

        private void SubmitCurrentState()
        {
            if (runtime == null) runtime = FindFirstObjectByType<MicrobitInputRuntime>();
            if (runtime == null) return;
            runtime.SubmitRawValues(latestRawX, latestRawY, latestRawZ, latestButtonA, latestButtonB);
        }

        private static short ReadInt16LittleEndian(byte[] buffer, int offset)
        {
            unchecked { return (short)(buffer[offset] | (buffer[offset + 1] << 8)); }
        }

        private void Fail(string message)
        {
            state = State.Failed;
            nextReconnectTime = Time.unscaledTime + reconnectDelaySeconds;
            Debug.LogWarning("[BleWinrtMicrobitSource] " + message);
            StopDeviceScanSafely();
        }

        private void StopDeviceScanSafely()
        {
            try { ble?.StopDeviceScan(); }
            catch { }
        }

        private void ShutdownBleApi()
        {
            try
            {
                if (bleStarted)
                {
                    try { ble?.StopDeviceScan(); } catch { }
                    ble?.Quit();
                    bleStarted = false;
                }
            }
            catch (Exception ex)
            {
                if (showDebugLog) Debug.LogWarning("BleApi.Quit warning: " + ex.Message);
            }
        }

        private void ResetInternalState()
        {
            state = State.Idle;
            targetDeviceId = "";
            targetDeviceName = "";
            foundAccelerometerService = false;
            foundButtonService = false;
            subscribedAccelerometer = false;
            subscribedButtonA = false;
            subscribedButtonB = false;
            latestRawX = latestRawY = latestRawZ = 0;
            latestButtonA = latestButtonB = 0;
            warnedAccelerometerSizeMismatch = false;
            warnedButtonSizeMismatch = false;
            loggedDeviceIds.Clear();
            loggedServices.Clear();
            loggedCharacteristics.Clear();
            lastDataTime = Time.unscaledTime;
        }

        private static bool UuidEquals(string a, string b)
        {
            return string.Equals(NormalizeUuid(a), NormalizeUuid(b), StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeUuid(string uuid)
        {
            if (string.IsNullOrWhiteSpace(uuid)) return "";
            return uuid.Trim().Trim('{', '}').ToLowerInvariant();
        }

        private class BleWinrtReflectionApi
        {
            public bool IsAvailable => apiType != null;
            public bool LastPollFinished { get; private set; }

            private readonly bool debug;
            private readonly Type apiType;
            private readonly Type deviceUpdateType;
            private readonly Type serviceType;
            private readonly Type characteristicType;
            private readonly Type dataType;

            private readonly MethodInfo startDeviceScan;
            private readonly MethodInfo stopDeviceScan;
            private readonly MethodInfo pollDevice;
            private readonly MethodInfo scanServices;
            private readonly MethodInfo pollService;
            private readonly MethodInfo scanCharacteristics;
            private readonly MethodInfo pollCharacteristic;
            private readonly MethodInfo subscribeCharacteristic;
            private readonly MethodInfo pollData;
            private readonly MethodInfo quit;

            public BleWinrtReflectionApi(bool debug)
            {
                this.debug = debug;
                apiType = FindType("BleApi");
                if (apiType == null) return;

                deviceUpdateType = apiType.GetNestedType("DeviceUpdate");
                serviceType = apiType.GetNestedType("Service");
                characteristicType = apiType.GetNestedType("Characteristic");
                dataType = apiType.GetNestedType("BLEData");

                startDeviceScan = apiType.GetMethod("StartDeviceScan", BindingFlags.Public | BindingFlags.Static);
                stopDeviceScan = apiType.GetMethod("StopDeviceScan", BindingFlags.Public | BindingFlags.Static);
                pollDevice = apiType.GetMethod("PollDevice", BindingFlags.Public | BindingFlags.Static);
                scanServices = apiType.GetMethod("ScanServices", BindingFlags.Public | BindingFlags.Static);
                pollService = apiType.GetMethod("PollService", BindingFlags.Public | BindingFlags.Static);
                scanCharacteristics = apiType.GetMethod("ScanCharacteristics", BindingFlags.Public | BindingFlags.Static);
                pollCharacteristic = apiType.GetMethod("PollCharacteristic", BindingFlags.Public | BindingFlags.Static);
                subscribeCharacteristic = apiType.GetMethod("SubscribeCharacteristic", BindingFlags.Public | BindingFlags.Static);
                pollData = apiType.GetMethod("PollData", BindingFlags.Public | BindingFlags.Static);
                quit = apiType.GetMethod("Quit", BindingFlags.Public | BindingFlags.Static);
            }

            public void StartDeviceScan() => Invoke(startDeviceScan);
            public void StopDeviceScan() => Invoke(stopDeviceScan);
            public void ScanServices(string deviceId) => Invoke(scanServices, deviceId);
            public void ScanCharacteristics(string deviceId, string serviceUuid) => Invoke(scanCharacteristics, deviceId, serviceUuid);
            public bool SubscribeCharacteristic(string deviceId, string serviceUuid, string characteristicUuid, bool block)
            {
                object result = Invoke(subscribeCharacteristic, deviceId, serviceUuid, characteristicUuid, block);
                return result is bool b && b;
            }
            public void Quit() => Invoke(quit);

            public bool PollDevice(out DeviceInfo device)
            {
                device = default;
                LastPollFinished = false;
                object obj = Activator.CreateInstance(deviceUpdateType);
                object[] args = { obj, false };
                object status = pollDevice.Invoke(null, args);
                string statusName = status?.ToString() ?? "";
                if (IsProcessing(statusName)) return false;
                if (IsFinished(statusName)) { LastPollFinished = true; return false; }
                object updated = args[0];
                device = new DeviceInfo
                {
                    name = GetString(updated, "name"),
                    id = GetString(updated, "id")
                };
                return true;
            }

            public bool PollService(out ServiceInfo service)
            {
                service = default;
                LastPollFinished = false;
                object obj = Activator.CreateInstance(serviceType);
                object[] args = { obj, false };
                object status = pollService.Invoke(null, args);
                string statusName = status?.ToString() ?? "";
                if (IsProcessing(statusName)) return false;
                if (IsFinished(statusName)) { LastPollFinished = true; return false; }
                object updated = args[0];
                service = new ServiceInfo { uuid = GetString(updated, "uuid") };
                return true;
            }

            public bool PollCharacteristic(out CharacteristicInfo characteristic)
            {
                characteristic = default;
                LastPollFinished = false;
                object obj = Activator.CreateInstance(characteristicType);
                object[] args = { obj, false };
                object status = pollCharacteristic.Invoke(null, args);
                string statusName = status?.ToString() ?? "";
                if (IsProcessing(statusName)) return false;
                if (IsFinished(statusName)) { LastPollFinished = true; return false; }
                object updated = args[0];
                characteristic = new CharacteristicInfo
                {
                    uuid = GetString(updated, "uuid"),
                    userDescription = GetString(updated, "userDescription")
                };
                return true;
            }

            public bool PollData(out DataInfo data)
            {
                data = default;
                object obj = Activator.CreateInstance(dataType);
                object[] args = { obj, false };
                object result = pollData.Invoke(null, args);
                bool hasData = result is bool b && b;
                if (!hasData) return false;

                object updated = args[0];
                data = new DataInfo
                {
                    deviceId = GetString(updated, "deviceId"),
                    serviceUuid = GetString(updated, "serviceUuid"),
                    characteristicUuid = GetString(updated, "characteristicUuid"),
                    size = GetInt(updated, "size"),
                    buf = GetBytes(updated, "buf")
                };
                return true;
            }

            private object Invoke(MethodInfo method, params object[] args)
            {
                if (method == null) throw new MissingMethodException("BleApi method not found.");
                return method.Invoke(null, args);
            }

            private static bool IsProcessing(string status) => status.IndexOf("PROCESSING", StringComparison.OrdinalIgnoreCase) >= 0;
            private static bool IsFinished(string status) => status.IndexOf("FINISHED", StringComparison.OrdinalIgnoreCase) >= 0;

            private static Type FindType(string fullName)
            {
                Type direct = Type.GetType(fullName);
                if (direct != null) return direct;

                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    try
                    {
                        Type t = asm.GetType(fullName);
                        if (t != null) return t;
                        foreach (var candidate in asm.GetTypes())
                        {
                            if (candidate.Name == fullName) return candidate;
                        }
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        foreach (var candidate in ex.Types)
                        {
                            if (candidate != null && candidate.Name == fullName) return candidate;
                        }
                    }
                    catch { }
                }
                return null;
            }

            private static string GetString(object obj, string name)
            {
                object value = GetMember(obj, name);
                return value?.ToString() ?? "";
            }

            private static int GetInt(object obj, string name)
            {
                object value = GetMember(obj, name);
                if (value == null) return 0;
                try { return Convert.ToInt32(value); }
                catch { return 0; }
            }

            private static byte[] GetBytes(object obj, string name)
            {
                object value = GetMember(obj, name);
                return value as byte[];
            }

            private static object GetMember(object obj, string name)
            {
                if (obj == null) return null;
                var type = obj.GetType();
                var field = type.GetField(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (field != null) return field.GetValue(obj);
                var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (prop != null) return prop.GetValue(obj);
                return null;
            }
        }

        private struct DeviceInfo { public string name; public string id; }
        private struct ServiceInfo { public string uuid; }
        private struct CharacteristicInfo { public string uuid; public string userDescription; }
        private struct DataInfo { public string deviceId; public string serviceUuid; public string characteristicUuid; public int size; public byte[] buf; }
    }
}
