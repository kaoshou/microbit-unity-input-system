# micro:bit Unity Input System

English | [繁體中文](README.md)

> Integrates the BBC micro:bit three-axis accelerometer and A/B buttons into the Unity Input System, allowing the micro:bit to work as a Unity input device like a keyboard, mouse, or gamepad.

![Unity](https://img.shields.io/badge/Unity-6.3%2B-black)
![Input System](https://img.shields.io/badge/Unity%20Input%20System-supported-blue)
![micro:bit](https://img.shields.io/badge/BBC%20micro%3Abit-supported-purple)
![License](https://img.shields.io/badge/License-MIT-green)

---

## Project Overview

This project is a course teaching package developed by Yu-Han Cheng (鄭郁翰) of Kun Shan University for the course **Motion Game Development**. It is designed to help students use the low-cost and widely available BBC micro:bit as a motion input device and quickly understand the complete workflow of:

```text
sensor data → Unity Input System → game control
```

### Demo Video

[![micro:bit Unity Input System Demo](https://img.youtube.com/vi/_y0Zz7hW7lc/0.jpg)](https://www.youtube.com/watch?v=_y0Zz7hW7lc)

This package converts the micro:bit three-axis accelerometer and A/B buttons into a custom Unity Input System device. It is suitable for:

- Motion game development courses
- Unity Input System teaching
- micro:bit sensor-based interaction exercises
- Classroom demonstrations, lab activities, and student projects
- Low-cost motion controller prototyping

---

## Features

- Supports the Unity Input System.
- Registers the micro:bit as a custom Unity input device named `MicrobitInputDevice`.
- Supports three-axis acceleration input: X, Y, and Z.
- Supports micro:bit A / B button input.
- Supports BLE wireless mode.
- Supports USB Serial Binary Data.
- Supports USB Serial CSV String for teaching and debugging, with lower performance.
- BLE mode uses the built-in micro:bit Accelerometer Service and Button Service, without UART.
- USB Binary mode uses a fixed-length packet and checksum, making it suitable for real-time game control.
- Sensor values can be read directly in `Update()` or used with Unity Input Actions.
- Designed for teaching, with a clear program structure that helps students observe, modify, and extend the data flow.

---

## Supported Input Controls

After importing this package, the Unity Input System provides the following control paths:

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
| `acceleration` | `Vector3` | micro:bit three-axis acceleration, normalized to approximately `-1 ~ 1` |
| `acceleration/x` | `float` | X-axis acceleration |
| `acceleration/y` | `float` | Y-axis acceleration |
| `acceleration/z` | `float` | Z-axis acceleration |
| `buttonA` | `Button` | micro:bit A button |
| `buttonB` | `Button` | micro:bit B button |

---

## Supported Modes

| Mode | micro:bit Program | Unity Source | Recommended Use |
|---|---|---|---|
| BLE built-in services | `microbit/makecode/ble_builtin_services.ts` | `BleWinrtMicrobitSource` | Wireless transmission scenarios |
| USB Serial Binary | `microbit/makecode/usb_binary.ts` | `BinaryUsbSerialMicrobitSource` | Wired transmission scenarios |
| USB Serial CSV | `microbit/makecode/usb_csv.ts` or `microbit/micropython/usb_csv.py` | `CsvStringUsbSerialMicrobitSource` | Teaching, data inspection, and debugging |

### Mode Selection Guide

| Scenario | Recommended Mode |
|---|---|
| Classroom teaching and observing raw data formats | USB CSV |
| General wired motion-controlled games | USB Binary |
| Wireless motion controllers or wearable applications | BLE |
| First-time beginner testing | USB CSV |
| Formal demonstrations or gameplay control | USB Binary or BLE |

---

## System Requirements

### Unity Side

- Developed with Unity 6.3. Other versions have not been fully tested, but they may also work.
- Unity Input System is required.
- Windows is recommended.
- BleWinrtDll is required for BLE mode.
- Because this package uses `SerialPort`, set the Project Settings `Api Compatibility Level` to `.NET Framework`.

### micro:bit Side

- BBC micro:bit v1 or v2
- MakeCode: used for BLE and USB Binary Data examples
- MicroPython: only provided for the USB CSV String example

### Additional Requirements for BLE Mode

- A Windows computer with Bluetooth Low Energy support
- BleWinrtDll installed in the Unity project
- A micro:bit flashed with the MakeCode program that enables the built-in BLE services

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

You can also use `MicrobitUnityInputSystem` as a local Unity package by opening Unity Package Manager, selecting `Add package from disk...`, and choosing:

```text
MicrobitUnityInputSystem/package.json
```

### Option 2: Install as a Unity Package

You can install this package using Git URL in Unity Package Manager:

```text
Window → Package Management → Package Manager → + → Add package from git URL...
```

Enter:

```text
https://github.com/kaoshou/microbit-unity-input-system.git?path=/MicrobitUnityInputSystem
```

---

## Unity Project Setup

### 1. Install the Unity Input System

In Unity, open:

```text
Window → Package Manager → Unity Registry → Input System → Install
```

After installation, Unity may ask whether to enable the new Input System. It is recommended to allow Unity to restart if prompted.

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

Because the USB transmission feature depends on `System.IO.Ports.SerialPort`, set the API compatibility level to `.NET Framework`.

Open:

```text
Edit → Project Settings → Player → Other Settings → Api Compatibility Level
```

Set it to:

```text
.NET Framework
```

### 4. Import This Package

Install the package using Unity Package Manager with the Git URL, place the `MicrobitUnityInputSystem` folder into your Unity project, or import it from disk through Package Manager. See the **Installation** section above.

### 5. Install BleWinrtDll for BLE Mode

To use BLE mode, open Unity Package Manager and select:

```text
Window → Package Management → Package Manager → + → Add package from git URL...
```

Enter:

```text
https://github.com/adabru/BleWinrtDll.git?path=/BleWinrtDll-UnityPackage
```

---

## Quick Start: Read micro:bit Acceleration and Buttons

The following example reads the micro:bit X-axis acceleration to move an object left and right, and detects whether the A and B buttons are pressed.

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

> For continuous sensor data such as acceleration, it is recommended to read `MicrobitInputDevice.current.acceleration.ReadValue()` directly in `Update()`. This approach is more suitable for real-time movement control than using Input Actions callbacks. Input Actions are more suitable for buttons, event-style input, or demonstrating the Unity Input System binding workflow in class.

---

# BLE Wireless Mode

BLE mode uses the built-in micro:bit:

```text
Accelerometer Service
Button Service
```

It does not use UART.

## micro:bit Setup

Create a new project in MakeCode. Before writing the program, open **Settings → Extensions**, search for `bluetooth`, and install the `bluetooth` extension.

Return to the editor, switch to JavaScript, and paste the following code:

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

After downloading the program to the micro:bit, the micro:bit will enable BLE accelerometer and button services.

If the pasted code shows an error at `bluetooth` and the object cannot be found, the MakeCode `bluetooth` extension is probably not installed.

## Unity Scene Setup

Create an empty GameObject in the Unity scene, for example:

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
| `Device Name Filter` | `micro:bit` | Scans for BLE devices whose names contain micro:bit |
| `Subscribe Block` | Initially disabled | If the device connects but no data is received, enable it for testing |
| `Show Debug Log` | Enabled only for debugging | Disable it after the connection is stable |

## BLE Notes

BLE is usually stable with only one Central program connected at a time. Before testing in Unity, close any programs that may occupy the micro:bit BLE connection.

If Unity shows that the micro:bit is connected but no data is received, try the following steps:

1. Reset the micro:bit.
2. Close other BLE tools.
3. Stop Unity Play Mode and enter Play Mode again.
4. Remove the micro:bit from Windows Bluetooth settings and pair it again.
5. Enable `Subscribe Block` to test whether the issue is related to the subscription process.

---

# USB Binary Data Transmission Mode

USB Binary Data mode is used for wired transmission scenarios. Instead of sending CSV strings, it sends fixed-length binary packets. This reduces the cost of string splitting and numeric parsing, making it more suitable for real-time game control.

## micro:bit Setup

Use the MakeCode program provided in the repository:

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
| `seq` | Sequence number, useful for observing packet continuity |
| `buttons` | Button state |
| `xLo xHi` | X-axis acceleration, Int16 little-endian |
| `yLo yHi` | Y-axis acceleration, Int16 little-endian |
| `zLo zHi` | Z-axis acceleration, Int16 little-endian |
| `checksum` | Checksum |

## Unity Scene Setup

Create an empty GameObject in the Unity scene and attach:

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
| `Show Rate Log` | Enabled during testing, disabled after stable |

> Do not attach `BinaryUsbSerialMicrobitSource`, `CsvStringUsbSerialMicrobitSource`, and `BleWinrtMicrobitSource` at the same time in the same scene. Only one data source should be enabled at a time; otherwise, conflicts may occur.

---

# USB CSV String Teaching Mode

USB CSV mode is the easiest format to inspect and explain. It is suitable for teaching, debugging, and helping beginners understand the data flow. However, because it requires string splitting and numeric parsing, it is less efficient than USB Binary Data mode.

## micro:bit Setup

You can use the MakeCode version:

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

Create an empty GameObject in the Unity scene and attach:

```text
MicrobitInputRuntime
CsvStringUsbSerialMicrobitSource
```

---

## Input Actions Setup

This package can be used with Unity Input Actions. It is recommended to use Input Actions mainly for **event-style input** such as the micro:bit **A / B buttons**, including jump, confirm, attack, menu navigation, and similar actions.

For continuous sensor data such as the **accelerometer**, it can technically be bound to Input Actions, but it is not recommended as the main control method. Acceleration values change continuously and rapidly. If you use `performed` callbacks for movement control, the behavior may be affected by callback timing, trigger frequency, sensor jitter, and event update timing. This can result in unstable movement or delayed response. For finished gameplay control, it is recommended to read `MicrobitInputDevice.current.acceleration.ReadValue()` directly in `Update()` and then apply dead zones, smoothing, sensitivity adjustment, or threshold-based detection as needed.

---

### Binding: buttonA

In the Input Actions Asset, create a new Action, for example:

```text
Action Name: ButtonA
Action Type: Button
Binding: <MicrobitInputDevice>/buttonA
```

---

### Binding: buttonB

In the Input Actions Asset, create another Action, for example:

```text
Action Name: ButtonB
Action Type: Button
Binding: <MicrobitInputDevice>/buttonB
```

---

### Input Actions Asset Setup Flow

1. Create or open a `.inputactions` file in your Unity project.
2. Add an Action Map, for example:

```text
Action Map: Microbit
```

3. Under this Action Map, add two Actions: `ButtonA` and `ButtonB`.
4. Set `ButtonA` as follows:

```text
Action Type: Button
Binding Path: <MicrobitInputDevice>/buttonA
```

5. Set `ButtonB` as follows:

```text
Action Type: Button
Binding Path: <MicrobitInputDevice>/buttonB
```

6. Save the Input Actions Asset.
7. If you use a `PlayerInput` component, make sure it references the correct Input Actions Asset and Action Map.

---

### Not Recommended as the Main Control Method: Accelerometer Binding

The accelerometer can still be bound in Input Actions. This may be useful for teaching demonstrations, data observation, or simple testing.

To bind the full three-axis acceleration, use:

```text
Action Name: Acceleration
Action Type: Value
Control Type: Vector3
Binding: <MicrobitInputDevice>/acceleration
```

To observe only one axis, such as the X axis, use:

```text
Action Name: AccelerationX
Action Type: Value
Control Type: Axis
Binding: <MicrobitInputDevice>/acceleration/x
```

However, for finished gameplay control, it is still recommended to read the current acceleration value directly from the micro:bit device instead of relying on Input Actions callbacks. This makes it easier to handle smoothing, sensitivity, dead zones, and sudden shake detection for continuous input.

---

## How It Works

### Overall Data Flow

```text
micro:bit sensors
        ↓
BLE / USB Binary / USB CSV String
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

`MicrobitInputRuntime` creates a custom device for the Unity Input System:

```text
MicrobitInputDevice
```

Regardless of whether the data source is BLE, USB Binary, or USB CSV, the final data is organized and submitted through:

```csharp
MicrobitInputRuntime.SubmitRawValues(rawX, rawY, rawZ, buttonA, buttonB)
```

The runtime normalizes the raw micro:bit acceleration values, approximately `-1024 ~ 1024`, into Unity-side values of approximately `-1 ~ 1`, and then updates the Input System state.

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

The implementation checks whether enough data exists using `buffer.Length` and does not rely completely on the `size` value returned by BleWinrtDll. In testing, `size` may sometimes be smaller than the actual buffer length. If the program depends only on `size`, Unity may appear connected but receive no acceleration data.

### USB Binary Data Side

USB Binary Data uses a fixed-length packet and checksum. This avoids the overhead of CSV string splitting and numeric parsing, making it more suitable for smooth wired control.

### USB CSV String Side

USB CSV String uses a text-based transmission format:

```text
x,y,z,a,b
```

Its advantages are readability, ease of teaching, and easy inspection with serial-port tools. Its disadvantage is lower efficiency, so USB Binary Data is recommended for formal control scenarios.

---

## Teaching Applications

This package is especially suitable for motion game development courses. It helps students understand:

- How sensor data enters a game engine.
- The relationship between X / Y / Z acceleration and physical movement.
- How continuous sensor data can be converted into character movement.
- How button input can be converted into jump, attack, confirm, and other events.
- Why real-time control needs to consider latency, noise, sampling rate, and data format.
- The differences between BLE and USB in stability, latency, and usage scenarios.
- How the Unity Input System receives custom hardware input devices.

Possible classroom exercises:

| Example | Description |
|---|---|
| Move a character left and right | Use X-axis tilt to control horizontal character movement |
| Jump detection | Detect sudden acceleration changes as a shake or jump |
| Balance game | Use X / Y axes to control platform tilt |
| Fruit-catching game | Tilt the micro:bit to move a basket |
| Motion-controlled menu | Use A / B buttons to switch and confirm selections |
| Swing trigger | Detect acceleration peaks as attack or fruit-slicing actions |

---

## Troubleshooting

### `MicrobitInputDevice.current` Is Null

Check the following:

1. `MicrobitInputRuntime` is attached in the scene.
2. At least one data Source is enabled, such as `BleWinrtMicrobitSource`, `BinaryUsbSerialMicrobitSource`, or `CsvStringUsbSerialMicrobitSource`.
3. The Unity Input System is installed and enabled.
4. There are no compilation errors in the Console.

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
2. The corresponding COM Port appears in Windows Device Manager.
3. The Unity Source `Port Name` is set to the correct `COMx`.
4. The Baud Rate matches the micro:bit program.
5. No other serial-port tool is occupying the same COM Port.
6. Only one USB Source is enabled in the scene.

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

This project was designed and organized by Yu-Han Cheng (鄭郁翰) of Kun Shan University for the course Motion Game Development, as a teaching package for Unity, micro:bit, and motion-based game interaction design.

```text
Developer: Yu-Han Cheng, Kun Shan University
Email: kaoshou@gmail.com
GitHub: https://github.com/kaoshou
Repository: https://github.com/kaoshou/microbit-unity-input-system.git
```

---

## Contributing

Issues, pull requests, and improvement suggestions are welcome. Possible contribution areas include:

- Adding more micro:bit game-control examples.
- Adding compatibility tests for different Unity versions.
- Improving BLE connection stability.
- Adding macOS or other platform support.
- Adding teaching documents, sample scenes, and classroom activity designs.
- Adding examples for signal smoothing, shake detection, posture judgment, and gesture recognition.

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
