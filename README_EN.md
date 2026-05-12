# micro:bit Unity Input System

English | [繁體中文](README.md)

> Integrate the BBC micro:bit accelerometer and A/B buttons into the Unity Input System, allowing the micro:bit to work as a custom Unity input device for motion-controlled games and interactive projects.

![Unity](https://img.shields.io/badge/Unity-6.3%2B-black)
![Input System](https://img.shields.io/badge/Unity%20Input%20System-supported-blue)
![micro:bit](https://img.shields.io/badge/BBC%20micro%3Abit-supported-purple)
![License](https://img.shields.io/badge/License-MIT-green)

---

## Project Overview

This project is a **course teaching package developed by Prof. Yu-Han Cheng at Kun Shan University** for the course **Motion Game Development**. It is designed to help students use the low-cost and widely available BBC micro:bit as a motion input device, while learning the full pipeline of “sensor data → Unity Input System → game control”.

The package converts the micro:bit three-axis accelerometer and A/B buttons into a custom Unity Input System device. It is suitable for:

- Motion game development courses
- Unity Input System teaching
- micro:bit sensor-based interaction projects
- Classroom demonstrations and student projects
- Low-cost motion controller prototyping

GitHub Repository:

```text
https://github.com/kaoshou/microbit-unity-input-system.git
```

---

## Features

- Supports the Unity Input System.
- Registers the micro:bit as a custom Unity input device named `MicrobitInputDevice`.
- Supports three-axis acceleration input: X, Y, and Z.
- Supports micro:bit A / B button input.
- Supports BLE wireless mode.
- Supports USB Serial Binary mode for smoother wired input.
- Supports USB Serial CSV mode for teaching and debugging.
- BLE mode uses the built-in micro:bit Accelerometer Service and Button Service; UART is not required.
- USB Binary mode uses a fixed-length packet and checksum for real-time control.
- Sensor values can be read directly in `Update()` or through Unity Input Actions.
- Designed for teaching, with a clear data flow and an extendable structure.

---

## Input Controls

After importing the package, the Unity Input System provides the following control paths:

```text
<MicrobitInputDevice>/acceleration
<MicrobitInputDevice>/acceleration/x
<MicrobitInputDevice>/acceleration/y
<MicrobitInputDevice>/acceleration/z
<MicrobitInputDevice>/buttonA
<MicrobitInputDevice>/buttonB
```

| Control | Type | Description |
|---|---:|---|
| `acceleration` | `Vector3` | Three-axis acceleration normalized to approximately `-1 ~ 1` |
| `acceleration/x` | `float` | X-axis acceleration |
| `acceleration/y` | `float` | Y-axis acceleration |
| `acceleration/z` | `float` | Z-axis acceleration |
| `buttonA` | `Button` | micro:bit A button |
| `buttonB` | `Button` | micro:bit B button |

---

## Supported Modes

| Mode | micro:bit Program | Unity Source | Recommended Use |
|---|---|---|---|
| BLE built-in services | `microbit/makecode/ble_builtin_services.ts` | `BleWinrtMicrobitSource` | Recommended wireless mode |
| USB Serial Binary | `microbit/makecode/usb_binary.ts` | `BinaryUsbSerialMicrobitSource` | Recommended smooth wired mode |
| USB Serial CSV | `microbit/makecode/usb_csv.ts` or `microbit/micropython/usb_csv.py` | `CsvStringUsbSerialMicrobitSource` | Teaching, inspection, and debugging |

### Which Mode Should I Use?

| Scenario | Recommended Mode |
|---|---|
| Classroom teaching and observing raw data | USB CSV |
| Smooth wired motion control | USB Binary |
| Wireless motion control | BLE |
| First-time beginner testing | USB CSV |
| Demonstration or real gameplay | USB Binary or BLE |

> BLE mode does not use the Nordic UART Service. You do not need to configure `6e400001 / 6e400002 / 6e400003`, and you do not need to modify BleWinrtDll. This package uses the built-in micro:bit Accelerometer Service and Button Service.

---

## Requirements

### Unity

- Unity 6.3 or later is recommended.
- Unity Input System.
- Windows is recommended.
- BleWinrtDll is required for BLE mode.

### micro:bit

- BBC micro:bit v1 or v2.
- MakeCode is recommended for the BLE and USB Binary examples.
- MicroPython is only provided for the USB CSV example.

### Additional BLE Requirements

- A Windows computer with Bluetooth Low Energy support.
- BleWinrtDll installed in the Unity project.
- A micro:bit flashed with the MakeCode program that enables the built-in BLE services.

---

## Recommended Repository Structure

```text
microbit-unity-input-system/
├─ MicrobitUnityInputSystem/
│  ├─ Runtime/
│  │  ├─ MicrobitInputDevice.cs
│  │  ├─ MicrobitInputRuntime.cs
│  │  ├─ BleWinrtMicrobitSource.cs
│  │  ├─ BinaryUsbSerialMicrobitSource.cs
│  │  └─ CsvStringUsbSerialMicrobitSource.cs
│  ├─ Samples~/
│  └─ package.json
├─ microbit/
│  ├─ makecode/
│  │  ├─ ble_builtin_services.ts
│  │  ├─ usb_binary.ts
│  │  └─ usb_csv.ts
│  └─ micropython/
│     └─ usb_csv.py
├─ README.md
├─ README_EN.md
└─ LICENSE
```

---

## Installation

### Option 1: Clone the Repository

```bash
git clone https://github.com/kaoshou/microbit-unity-input-system.git
```

You can copy the following folder directly into the `Assets` folder of your Unity project:

```text
MicrobitUnityInputSystem
```

Alternatively, open the Unity Package Manager, click the `+` icon in the top left, select `Add package from disk...`, and then select `MicrobitUnityInputSystem/package.json`.

### Option 2: Install as a Unity Package

If the repository is arranged as a Unity Package, open the Unity Package Manager:

```text
Window → Package Manager → + → Add package from git URL...
```

Enter:

```text
https://github.com/kaoshou/microbit-unity-input-system.git?path=/MicrobitUnityInputSystem
```

---

## Unity Project Setup

### 1. Install the Unity Input System

Open:

```text
Window → Package Manager → Unity Registry → Input System → Install
```

Unity may ask whether to enable the new Input System. Allow Unity to restart if prompted.

### 2. Set Active Input Handling

Open:

```text
Edit → Project Settings → Player → Other Settings → Active Input Handling
```

Recommended setting:

```text
Both
```

or:

```text
Input System Package (New)
```

### 3. Set Api Compatibility Level

Since this project relies on `System.IO.Ports.SerialPort` for USB communication, the API Compatibility Level of the project must be set to `.NET Framework`.

Open:

```text
Edit → Project Settings → Player → Other Settings → Api Compatibility Level
```

Set it to:

```text
.NET Framework
```

### 4. Import This Package

Install via Git URL directly using the Unity Package Manager, or place the `MicrobitUnityInputSystem` folder of this project into your Unity project. You can also import it via the Package Manager from disk (please refer to the **Installation** section).

### 5. Install BleWinrtDll for BLE Mode

If you want to use BLE mode, open Unity Package Manager and select:

```text
+ → Add package from git URL...
```

Enter:

```text
https://github.com/adabru/BleWinrtDll.git?path=/BleWinrtDll-UnityPackage
```

No additional Scripting Define Symbol is required. If BleWinrtDll is not installed, USB modes can still compile and run. The BLE Source will show a Play Mode warning when BleWinrtDll is missing.

---

## Quick Start: Read Acceleration and Buttons

The following example reads the X-axis acceleration and moves a GameObject horizontally. It also detects A and B button presses.

```csharp
using tw.yuhan.MicrobitInputSystem;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 6f;

    private void Update()
    {
        var microbit = MicrobitInputDevice.current;
        if (microbit == null)
        {
            return;
        }

        Vector3 accel = microbit.acceleration.ReadValue();

        transform.Translate(Vector3.right * accel.x * speed * Time.deltaTime);

        if (microbit.buttonA.wasPressedThisFrame)
        {
            Debug.Log("A pressed");
        }

        if (microbit.buttonB.wasPressedThisFrame)
        {
            Debug.Log("B pressed");
        }
    }
}
```

> For continuous sensor values such as acceleration, it is recommended to read `MicrobitInputDevice.current.acceleration.ReadValue()` directly in `Update()`. This is usually more appropriate for real-time movement control than using Input Actions callbacks. Input Actions are still useful for buttons, event-style input, or teaching the Unity Input System binding workflow.

---

# BLE Wireless Mode

BLE mode uses the built-in micro:bit:

```text
Accelerometer Service
Button Service
```

UART is not used.

## micro:bit Setup

Create a new MakeCode project, switch to JavaScript, and paste the following code:

```typescript
bluetooth.startAccelerometerService()
bluetooth.startButtonService()

basic.showIcon(IconNames.SmallDiamond)

bluetooth.onBluetoothConnected(function () {
    basic.showIcon(IconNames.Yes)
})

bluetooth.onBluetoothDisconnected(function () {
    basic.showIcon(IconNames.No)
})
```

You can also use the file provided in this repository:

```text
microbit/makecode/ble_builtin_services.ts
```

After flashing the program to the micro:bit, the built-in BLE accelerometer and button services will be enabled.

## Unity Scene Setup

Create an empty GameObject in your scene, for example:

```text
MicrobitInput
```

Attach the following Components:

```text
MicrobitInputRuntime
BleWinrtMicrobitSource
```

Recommended `BleWinrtMicrobitSource` settings:

| Setting | Recommended Value | Description |
|---|---|---|
| `Device Name Filter` | `micro:bit` | Scans for BLE devices with names containing micro:bit |
| `Subscribe Block` | Disabled initially | Enable it for testing if connected but no data is received |
| `Show Debug Log` | Enabled only for debugging | Disable it after the connection is stable |

## BLE Notes

BLE is usually stable with only one Central program connected at a time. Before testing in Unity, close other tools that may occupy the micro:bit BLE connection, such as:

```text
Bluetooth LE Explorer
nRF Connect
MakeCode pairing window
Other BLE testing tools
```

If Unity shows that the device is connected but no data is received, try the following steps:

1. Reset the micro:bit.
2. Make sure the MakeCode program enables `bluetooth.startAccelerometerService()`.
3. Make sure the MakeCode program enables `bluetooth.startButtonService()`.
4. Close Bluetooth LE Explorer, nRF Connect, MakeCode pairing windows, and other BLE tools.
5. Stop and restart Unity Play Mode.
6. Try enabling `Subscribe Block`.
7. Remove the paired micro:bit from Windows Bluetooth settings and pair it again.

---

# USB Binary Mode

USB Binary is the recommended wired mode. It sends fixed-length binary packets instead of CSV strings, reducing string splitting and number parsing overhead. This makes it more suitable for real-time game control.

## micro:bit Setup

Use the MakeCode program provided in this repository:

```text
microbit/makecode/usb_binary.ts
```

## Packet Format

USB Binary mode uses a fixed 11-byte packet:

```text
AA 55 seq buttons xLo xHi yLo yHi zLo zHi checksum
```

| Field | Description |
|---|---|
| `AA 55` | Packet header |
| `seq` | Sequence number for checking packet continuity |
| `buttons` | Button state |
| `xLo xHi` | X-axis acceleration, Int16 little-endian |
| `yLo yHi` | Y-axis acceleration, Int16 little-endian |
| `zLo zHi` | Z-axis acceleration, Int16 little-endian |
| `checksum` | Checksum |

## Unity Scene Setup

Create an empty GameObject in your scene and attach:

```text
MicrobitInputRuntime
BinaryUsbSerialMicrobitSource
```

Recommended settings:

| Setting | Recommended Value |
|---|---|
| `Port Name` | `COMx` |
| `Baud Rate` | `115200` |
| `Emit Latest Every Frame` | Enabled |
| `Show Debug Log` | Disabled |
| `Show Rate Log` | Enabled for testing, disabled after stable |

> Do not attach `BinaryUsbSerialMicrobitSource`, `CsvStringUsbSerialMicrobitSource`, and `BleWinrtMicrobitSource` at the same time. Only one data source should be active at a time.

---

# USB CSV Teaching Mode

USB CSV mode is the easiest format to inspect and explain. It is useful for teaching, debugging, and helping beginners understand the data flow. However, because it requires string splitting and number parsing, it is less efficient than USB Binary.

## micro:bit Setup

Use the MakeCode version:

```text
microbit/makecode/usb_csv.ts
```

or the MicroPython version:

```text
microbit/micropython/usb_csv.py
```

## Data Format

Each line uses the following format:

```text
x,y,z,a,b
```

Example:

```text
12,-35,980,1,0
```

| Field | Description |
|---|---|
| `x` | X-axis acceleration |
| `y` | Y-axis acceleration |
| `z` | Z-axis acceleration |
| `a` | A button state |
| `b` | B button state |

## Unity Scene Setup

Create an empty GameObject in your scene and attach:

```text
MicrobitInputRuntime
CsvStringUsbSerialMicrobitSource
```

> This file was previously named `UsbSerialMicrobitSource.cs`. It has now been renamed to `CsvStringUsbSerialMicrobitSource.cs` to make it clear that this Source handles CSV string communication and is not a generic USB Serial source for all modes.

---

## Input Actions Setup

This package can be used with Unity Input Actions. However, for continuous acceleration data, using a `performed` callback is not recommended as the main movement-control method. Reading the Action value directly in `Update()` is usually more suitable.

### Bind the Full Three-Axis Acceleration

Input Action settings:

```text
Action Type: Value
Control Type: Vector3
Binding: <MicrobitInputDevice>/acceleration
```

C# example:

```csharp
using UnityEngine;
using UnityEngine.InputSystem;

public class MicrobitActionReader : MonoBehaviour
{
    public InputActionReference accelerationAction;

    private void OnEnable()
    {
        accelerationAction.action.Enable();
    }

    private void OnDisable()
    {
        accelerationAction.action.Disable();
    }

    private void Update()
    {
        Vector3 accel = accelerationAction.action.ReadValue<Vector3>();
        Debug.Log(accel);
    }
}
```

### Bind a Single X Axis

Input Action settings:

```text
Action Type: Value
Control Type: Axis
Binding: <MicrobitInputDevice>/acceleration/x
```

C# example:

```csharp
float x = moveAction.action.ReadValue<float>();
```

Do not write:

```csharp
context.ReadValue<Axis>();
```

The value type of `AxisControl` is `float`, not `Axis`.

---

## How It Works

### Overall Data Flow

```text
micro:bit sensors
        ↓
BLE / USB Binary / USB CSV
        ↓
Unity Source Component
        ↓
MicrobitInputRuntime.SubmitRawValues(...)
        ↓
MicrobitInputDevice
        ↓
Unity Input System
        ↓
GameObject control, Input Actions, interaction logic
```

### Unity Side

`MicrobitInputRuntime` creates a custom Unity Input System device:

```text
MicrobitInputDevice
```

Whether the source is BLE, USB Binary, or USB CSV, the final data is submitted through:

```csharp
MicrobitInputRuntime.SubmitRawValues(rawX, rawY, rawZ, buttonA, buttonB)
```

The runtime normalizes the raw micro:bit acceleration values, approximately `-1024 ~ 1024`, to Unity-side values of approximately `-1 ~ 1`, and then updates the Input System state.

### BLE Side

BLE mode uses the built-in micro:bit services:

```text
Accelerometer Service
Button Service
```

Acceleration data is a fixed 6-byte payload:

```text
X: Int16 little-endian
Y: Int16 little-endian
Z: Int16 little-endian
```

The implementation checks whether enough data exists using `buffer.Length` and does not rely solely on the `size` value returned by BleWinrtDll. In testing, `size` may sometimes be smaller than the actual buffer length. If the implementation depends only on `size`, Unity may appear connected but receive no acceleration data.

### USB Binary Side

USB Binary mode uses a fixed-length packet and checksum. This avoids the overhead of CSV string splitting and numeric parsing, making it more suitable for smooth wired control.

### USB CSV Side

USB CSV mode transmits text data:

```text
x,y,z,a,b
```

Its advantages are readability, ease of teaching, and easy inspection with serial-port tools. Its disadvantage is lower efficiency, so USB Binary is recommended for formal control scenarios.

---

## Teaching Applications

This package is especially suitable for motion game development courses. It helps students understand:

- How sensor data enters a game engine.
- The relationship between X / Y / Z acceleration and physical movement.
- How continuous sensor values can be mapped to character movement.
- How button input can be mapped to jump, attack, confirm, or menu actions.
- Why real-time control must consider latency, noise, sampling rate, and data format.
- The differences between BLE and USB in stability, latency, and usage scenarios.
- How the Unity Input System can receive custom hardware input.

Possible classroom examples:

| Example | Description |
|---|---|
| Horizontal character movement | Tilt the micro:bit along the X axis to move a character left and right |
| Jump detection | Detect a sudden acceleration change as a shake or jump |
| Balance game | Use X / Y axes to control platform tilt |
| Fruit-catching game | Tilt the micro:bit to move a basket |
| Motion-controlled menu | Use A / B buttons for selection and confirmation |
| Swing trigger | Detect acceleration peaks as attack or slicing actions |

---

## Troubleshooting

### `MicrobitInputDevice.current` Is Null

Check the following:

1. `MicrobitInputRuntime` is attached in the scene.
2. At least one data Source is enabled, such as `BleWinrtMicrobitSource`, `BinaryUsbSerialMicrobitSource`, or `CsvStringUsbSerialMicrobitSource`.
3. The Unity Input System is installed and enabled.
4. The Unity Console has no compilation errors.

### BLE Is Connected but No Data Is Received

Try the following:

1. Reset the micro:bit.
2. Confirm that the MakeCode program enables `bluetooth.startAccelerometerService()`.
3. Confirm that the MakeCode program enables `bluetooth.startButtonService()`.
4. Close Bluetooth LE Explorer, nRF Connect, MakeCode pairing windows, and other BLE tools.
5. Stop and restart Unity Play Mode.
6. Test with `Subscribe Block` enabled.
7. Remove the paired micro:bit from Windows Bluetooth settings and pair it again.

### USB Mode Receives No Data

Check the following:

1. The micro:bit is connected through USB.
2. The correct COM Port appears in Windows Device Manager.
3. The Unity Source `Port Name` is set to the correct `COMx`.
4. The Baud Rate matches the micro:bit program.
5. No other serial-port tool is occupying the same COM Port.
6. Only one USB Source is active in the scene.

### Input Actions Callback Is Not Responsive Enough

Acceleration is continuous sensor data. It is recommended to read it actively in `Update()`:

```csharp
Vector3 accel = MicrobitInputDevice.current.acceleration.ReadValue();
```

Input Actions callbacks are more suitable for button events and are not recommended as the main method for acceleration-based movement control.

### `context.ReadValue<Axis>()` Causes an Error

The value type of `AxisControl` is `float`. Use:

```csharp
float x = context.ReadValue<float>();
```

or in `Update()`:

```csharp
float x = moveAction.action.ReadValue<float>();
```

---

## micro:bit Limitations

- MakeCode can use BLE built-in services.
- The MicroPython example only provides USB CSV and does not provide the same BLE built-in services as MakeCode.
- BLE is usually stable with only one Central connected at a time.
- Windows BLE may cache connection states.
- If data suddenly stops, reset the micro:bit, close other BLE tools, and restart Unity.
- micro:bit acceleration values naturally fluctuate. For finished games, consider adding a dead zone, smoothing, or threshold-based detection.

---

## Developer Information

This project was designed and organized by **Prof. Yu-Han Cheng, Kun Shan University**, for the course **Motion Game Development** as a teaching package for Unity, micro:bit, and motion-based game interaction design.

```text
Developer: Yu-Han Cheng, Kun Shan University
Email: kaoshou@gmail.com
GitHub: https://github.com/kaoshou
Repository: https://github.com/kaoshou/microbit-unity-input-system.git
```

---

## Contributing

Issues, pull requests, and improvement suggestions are welcome. Possible contribution areas include:

- More micro:bit game-control examples.
- Compatibility testing with different Unity versions.
- BLE connection stability improvements.
- macOS or additional platform support.
- Teaching materials, sample scenes, and classroom activity designs.
- Examples for signal smoothing, shake detection, and posture or gesture judgment.

---

## License

This project is released under the MIT License. See [`LICENSE`](LICENSE).

---

## Acknowledgements

This project uses or refers to the following technologies:

- [Unity](https://unity.com/)
- [Unity Input System](https://docs.unity3d.com/Packages/com.unity.inputsystem@latest)
- [BBC micro:bit](https://microbit.org/)
- [Microsoft MakeCode for micro:bit](https://makecode.microbit.org/)
- [BleWinrtDll](https://github.com/adabru/BleWinrtDll)

