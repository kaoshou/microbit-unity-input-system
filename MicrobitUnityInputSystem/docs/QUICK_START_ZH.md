# 快速開始

## 1. 匯入套件

- **方法 A**：將 `MicrobitUnityInputSystem` 資料夾放入 Unity 專案的 `Assets` 中。
- **方法 B**：在 Unity Package Manager 中使用 `Add package from git URL...` 輸入 `https://github.com/kaoshou/microbit-unity-input-system.git?path=/MicrobitUnityInputSystem`。

## 2. 專案必要設定

由於 USB 連線使用 SerialPort，必須調整 API 相容性：
- 開啟 `Project Settings -> Player -> Other Settings`。
- 將 **Api Compatibility Level** 設定為 **.NET Framework**。

## 3. 模式選擇與設定

### 無線 BLE 模式
1. micro:bit 燒錄 `microbit/makecode/ble_builtin_services.ts`。
2. Unity 安裝 Input System。
3. Unity 安裝 [BleWinrtDll](https://github.com/adabru/BleWinrtDll) (使用 Git URL：`https://github.com/adabru/BleWinrtDll.git?path=/BleWinrtDll-UnityPackage`)。
4. 場景新增空物件，掛上：
   - `MicrobitInputRuntime`
   - `BleWinrtMicrobitSource`

### 有線 USB Binary Data 模式
1. micro:bit 燒錄 `microbit/makecode/usb_binary.ts`。
2. 場景新增空物件，掛上：
   - `MicrobitInputRuntime`
   - `BinaryUsbSerialMicrobitSource`
3. 設定 COM Port (例如 `COM3`)，Baud Rate 為 `115200`。

## 4. 讀取資料

在 C# 腳本中：

```csharp
using tw.yuhan.MicrobitInputSystem;
using UnityEngine;

// ...
var microbit = MicrobitInputDevice.current;
if (microbit != null) {
    Vector3 accel = microbit.acceleration.ReadValue();
    bool isPressed = microbit.buttonA.isPressed;
}
```

## 5. 使用範例 (Samples)

本套件附帶了一個 **Basic Demo** 範例，包含已設定好的場景與控制腳本：

1. 開啟 Unity Package Manager。
2. 在清單中選擇 **micro:bit Unity Input System**。
3. 在右側細節面板中找到 **Samples** 區塊。
4. 點選 **Basic Demo** 旁的 **Import** 按鈕。
5. 匯入後，您可以在專案視窗的 `Assets/Samples/micro:bit Unity Input System/[版本號]/Basic Demo` 中找到範例場景並開啟測試。

### 範例腳本說明

範例中包含以下三個核心腳本，展示了不同的互動方式：

- **`MicrobitInputDebugText.cs`**：
  使用 `OnGUI` 在遊戲畫面左上角即時顯示加速度 (X, Y, Z) 的數值以及 A/B 按鈕的開關狀態。適合用於初步確認連線是否正常。

- **`MicrobitInputObjTiltExample.cs`**：
  展示如何將 micro:bit 的傾斜程度轉換為 3D 物件的旋轉角度。此範例特別加入了 `Vector3.Lerp` 平滑濾波，讓物件的轉動更加流暢，減少感測器的雜訊抖動。

- **`MicrobitPlayerMoveExample.cs`**：
  展示經典的體感移動控制。使用加速度 X 軸控制物件左右位移，並示範如何偵測 A/B 按鈕的 `wasPressedThisFrame` 事件（單次觸發）來執行特定動作。
