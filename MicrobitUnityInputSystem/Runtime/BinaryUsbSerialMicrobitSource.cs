using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using UnityEngine;

namespace tw.yuhan.MicrobitInputSystem
{
    /// <summary>
    /// 高流暢 USB 模式：讀取 micro:bit 傳出的固定長度 Binary Frame。
    /// 封包格式：AA 55 seq buttons xLo xHi yLo yHi zLo zHi checksum，共 11 bytes。
    /// </summary>
    [DefaultExecutionOrder(-32000)]
    public class BinaryUsbSerialMicrobitSource : MonoBehaviour
    {
        [Header("Target Runtime")]
        public MicrobitInputRuntime runtime;

        [Header("Serial Port")]
        public string portName = "COM3";
        public int baudRate = 115200;

        [Header("輸出策略")]
        [Tooltip("勾選後，Unity 每一幀都會輸出最後收到的資料，讓控制較連續。")]
        public bool emitLatestEveryFrame = true;

        [Tooltip("超過幾秒沒有收到新資料，視為 stale。")]
        public float staleTimeoutSeconds = 1f;

        [Tooltip("資料 stale 時輸出中立值，避免角色停在舊方向。")]
        public bool emitNeutralWhenStale = true;

        [Header("Debug")]
        public bool showDebugLog = false;
        public bool showRateLog = false;
        public float rateLogInterval = 1f;

        private SerialPort serialPort;
        private Thread readThread;
        private volatile bool running;

        private readonly object latestLock = new object();
        private bool hasLatest;
        private bool hasNewSinceLastFrame;
        private short latestX;
        private short latestY;
        private short latestZ;
        private int latestA;
        private int latestB;
        private double latestReceiveTimeSec;

        private int framesReceived;
        private int framesBadChecksum;
        private int bytesDiscarded;
        private float nextRateLogTime;

        private readonly List<byte> rxBuffer = new List<byte>(512);

        private const byte Header1 = 0xAA;
        private const byte Header2 = 0x55;
        private const int FrameSize = 11;

        private void Start()
        {
            if (runtime == null)
            {
                runtime = FindFirstObjectByType<MicrobitInputRuntime>();
            }
            Open();
        }

        private void OnDisable() => Close();
        private void OnApplicationQuit() => Close();

        private void Update()
        {
            if (runtime == null)
            {
                runtime = FindFirstObjectByType<MicrobitInputRuntime>();
                if (runtime == null) return;
            }

            short x = 0, y = 0, z = 0;
            int a = 0, b = 0;
            bool shouldEmit = false;
            bool isStale = false;
            bool localHasLatest;

            lock (latestLock)
            {
                localHasLatest = hasLatest;
                if (hasLatest)
                {
                    double age = NowSeconds() - latestReceiveTimeSec;
                    isStale = age > staleTimeoutSeconds;

                    shouldEmit = emitLatestEveryFrame || hasNewSinceLastFrame;
                    x = latestX;
                    y = latestY;
                    z = latestZ;
                    a = latestA;
                    b = latestB;
                    hasNewSinceLastFrame = false;
                }
            }

            if (localHasLatest && shouldEmit)
            {
                if (isStale && emitNeutralWhenStale)
                {
                    runtime.SubmitNeutral();
                }
                else
                {
                    runtime.SubmitRawValues(x, y, z, a, b);
                }
            }

            if (showRateLog && Time.unscaledTime >= nextRateLogTime)
            {
                nextRateLogTime = Time.unscaledTime + rateLogInterval;
                int received;
                int bad;
                int discarded;
                lock (latestLock)
                {
                    received = framesReceived;
                    bad = framesBadChecksum;
                    discarded = bytesDiscarded;
                    framesReceived = 0;
                    framesBadChecksum = 0;
                    bytesDiscarded = 0;
                }
                Debug.Log($"USB Binary Rate: frames={received}/s, badChecksum={bad}, discardedBytes={discarded}, hasLatest={localHasLatest}");
            }
        }

        public void Open()
        {
            Close();
            try
            {
                serialPort = new SerialPort(portName, baudRate)
                {
                    ReadTimeout = 100,
                    WriteTimeout = 100,
                    DtrEnable = true,
                    RtsEnable = true
                };
                serialPort.Open();

                running = true;
                readThread = new Thread(ReadLoop) { IsBackground = true };
                readThread.Start();

                if (showDebugLog)
                {
                    Debug.Log($"Binary USB Serial opened: {portName}, baud={baudRate}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Binary USB Serial open failed: {ex.Message}");
            }
        }

        public void Close()
        {
            running = false;
            if (readThread != null && readThread.IsAlive)
            {
                readThread.Join(500);
                readThread = null;
            }

            if (serialPort != null)
            {
                try
                {
                    if (serialPort.IsOpen) serialPort.Close();
                }
                catch { }
                serialPort = null;
            }
        }

        private void ReadLoop()
        {
            byte[] temp = new byte[64];
            while (running)
            {
                try
                {
                    if (serialPort == null || !serialPort.IsOpen)
                    {
                        Thread.Sleep(10);
                        continue;
                    }

                    int count = serialPort.Read(temp, 0, temp.Length);
                    if (count <= 0) continue;

                    lock (rxBuffer)
                    {
                        for (int i = 0; i < count; i++) rxBuffer.Add(temp[i]);
                        ParseFramesLocked();
                    }
                }
                catch (TimeoutException)
                {
                    // 正常：沒有資料時略過。
                }
                catch (Exception ex)
                {
                    if (showDebugLog) Debug.LogWarning($"Binary USB read error: {ex.Message}");
                    Thread.Sleep(100);
                }
            }
        }

        private void ParseFramesLocked()
        {
            while (rxBuffer.Count >= FrameSize)
            {
                int headerIndex = FindHeaderLocked();
                if (headerIndex < 0)
                {
                    lock (latestLock) bytesDiscarded += rxBuffer.Count;
                    rxBuffer.Clear();
                    return;
                }

                if (headerIndex > 0)
                {
                    lock (latestLock) bytesDiscarded += headerIndex;
                    rxBuffer.RemoveRange(0, headerIndex);
                }

                if (rxBuffer.Count < FrameSize) return;

                byte expectedChecksum = CalculateChecksum(rxBuffer, 0, 10);
                byte actualChecksum = rxBuffer[10];
                if (expectedChecksum != actualChecksum)
                {
                    lock (latestLock) framesBadChecksum++;
                    rxBuffer.RemoveAt(0);
                    continue;
                }

                short x = ReadInt16LE(rxBuffer, 4);
                short y = ReadInt16LE(rxBuffer, 6);
                short z = ReadInt16LE(rxBuffer, 8);
                byte buttons = rxBuffer[3];
                int a = (buttons & 0x01) != 0 ? 1 : 0;
                int b = (buttons & 0x02) != 0 ? 1 : 0;

                lock (latestLock)
                {
                    latestX = x;
                    latestY = y;
                    latestZ = z;
                    latestA = a;
                    latestB = b;
                    latestReceiveTimeSec = NowSeconds();
                    hasLatest = true;
                    hasNewSinceLastFrame = true;
                    framesReceived++;
                }

                rxBuffer.RemoveRange(0, FrameSize);
            }
        }

        private int FindHeaderLocked()
        {
            for (int i = 0; i <= rxBuffer.Count - 2; i++)
            {
                if (rxBuffer[i] == Header1 && rxBuffer[i + 1] == Header2) return i;
            }
            return -1;
        }

        private static byte CalculateChecksum(List<byte> buffer, int start, int count)
        {
            int sum = 0;
            for (int i = start; i < start + count; i++) sum = (sum + buffer[i]) & 0xFF;
            return (byte)sum;
        }

        private static short ReadInt16LE(List<byte> buffer, int offset)
        {
            unchecked
            {
                return (short)(buffer[offset] | (buffer[offset + 1] << 8));
            }
        }

        private static double NowSeconds()
        {
            return DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds;
        }
    }
}
