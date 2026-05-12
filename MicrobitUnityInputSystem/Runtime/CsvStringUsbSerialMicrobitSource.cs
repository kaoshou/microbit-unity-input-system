using System;
using System.Collections.Concurrent;
using System.IO.Ports;
using System.Threading;
using UnityEngine;

namespace tw.yuhan.MicrobitInputSystem
{
    /// <summary>
    /// 教學與除錯用 USB CSV 模式：讀取 x,y,z,a,b 文字行。
    /// 若追求流暢度，建議改用 BinaryUsbSerialMicrobitSource。
    /// </summary>
    [DefaultExecutionOrder(-32000)]
    public class CsvStringUsbSerialMicrobitSource : MonoBehaviour
    {
        [Header("Target Runtime")]
        public MicrobitInputRuntime runtime;

        [Header("Serial Port")]
        public string portName = "COM3";
        public int baudRate = 115200;

        [Header("輸出策略")]
        public bool emitLatestEveryFrame = true;
        public float staleTimeoutSeconds = 1f;
        public bool emitNeutralWhenStale = true;

        [Header("Debug")]
        public bool showDebugLog = false;
        public bool showRateLog = false;
        public float rateLogInterval = 1f;

        private SerialPort serialPort;
        private Thread readThread;
        private volatile bool running;

        private readonly ConcurrentQueue<string> lineQueue = new ConcurrentQueue<string>();
        private string latestLine = "";
        private float latestReceiveTime;
        private bool hasLatest;
        private bool hasNewSinceLastFrame;

        private int linesReceived;
        private float nextRateLogTime;

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

            string newest = null;
            while (lineQueue.TryDequeue(out string line))
            {
                newest = line;
            }

            if (!string.IsNullOrWhiteSpace(newest))
            {
                latestLine = newest;
                latestReceiveTime = Time.unscaledTime;
                hasLatest = true;
                hasNewSinceLastFrame = true;
                linesReceived++;
            }

            bool isStale = hasLatest && Time.unscaledTime - latestReceiveTime > staleTimeoutSeconds;
            bool shouldEmit = hasLatest && (emitLatestEveryFrame || hasNewSinceLastFrame);
            hasNewSinceLastFrame = false;

            if (shouldEmit)
            {
                if (isStale && emitNeutralWhenStale)
                {
                    runtime.SubmitNeutral();
                }
                else
                {
                    runtime.SubmitLine(latestLine);
                }
            }

            if (showRateLog && Time.unscaledTime >= nextRateLogTime)
            {
                nextRateLogTime = Time.unscaledTime + rateLogInterval;
                Debug.Log($"USB CSV Rate: lines={linesReceived}/s, hasLatest={hasLatest}");
                linesReceived = 0;
            }
        }

        public void Open()
        {
            Close();
            try
            {
                serialPort = new SerialPort(portName, baudRate)
                {
                    NewLine = "\n",
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
                    Debug.Log($"CSV USB Serial opened: {portName}, baud={baudRate}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"CSV USB Serial open failed: {ex.Message}");
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
            while (running)
            {
                try
                {
                    if (serialPort == null || !serialPort.IsOpen)
                    {
                        Thread.Sleep(10);
                        continue;
                    }

                    string line = serialPort.ReadLine();
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        lineQueue.Enqueue(line.Trim());
                    }
                }
                catch (TimeoutException)
                {
                }
                catch (Exception ex)
                {
                    if (showDebugLog) Debug.LogWarning($"CSV USB read error: {ex.Message}");
                    Thread.Sleep(100);
                }
            }
        }
    }
}
