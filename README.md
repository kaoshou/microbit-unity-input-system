# micro:bit Unity Input System

[English](README_EN.md) | 繁體中文

> 將 BBC micro:bit 的三軸加速度與 A/B 按鈕整合到 Unity Input System，讓 micro:bit 可以像鍵盤、滑鼠、手把一樣成為 Unity 的輸入裝置。

![Unity](https://img.shields.io/badge/Unity-6.3%2B-black)
![Input System](https://img.shields.io/badge/Unity%20Input%20System-supported-blue)
![micro:bit](https://img.shields.io/badge/BBC%20micro%3Abit-supported-purple)
![License](https://img.shields.io/badge/License-MIT-green)

---

## 專案說明

本專案為崑山科技大學鄭郁翰老師於「體感遊戲製作」課程所製作的課程教學套件，目的在於讓學生能以低成本、容易取得的 BBC micro:bit 作為體感輸入裝置，快速理解「感測器資料 → Unity 輸入系統 → 遊戲控制」的完整流程。

### 展示影片 (Demo Video)

[![micro:bit Unity Input System Demo](https://img.youtube.com/vi/_y0Zz7hW7lc/0.jpg)](https://www.youtube.com/watch?v=_y0Zz7hW7lc)

本套件將 micro:bit 的三軸加速度與 A/B 按鈕整合為 Unity Input System 的自訂輸入裝置，適合用於：

- 體感遊戲製作課程
- Unity Input System 教學
- micro:bit 感測器互動實作
- 教學展示、實驗課程與學生專題
- 低成本體感控制器原型開發

## 主要特色

- 支援 Unity Input System。
- 將 micro:bit 註冊為 Unity 自訂輸入裝置 `MicrobitInputDevice`。
- 支援三軸加速度讀取：X、Y、Z。
- 支援 micro:bit A / B 按鈕輸入。
- 支援 BLE 無線模式。
- 支援 USB Serial Binary Data。
- 支援 USB Serial CSV String (教學與除錯用，效能不佳)。
- BLE 使用 micro:bit 內建 Accelerometer Service 與 Button Service，不需使用 UART。
- USB Binary 使用固定長度封包與 checksum，適合即時遊戲控制。
- 可直接在 `Update()` 中讀取感測值，也可搭配 Input Actions 使用。
- 適合教學使用，程式架構清楚，便於學生觀察資料流與修改擴充。

---

## 支援的輸入控制

匯入本套件後，Unity Input System 會提供下列控制路徑：

```text
<MicrobitInputDevice>/acceleration
<MicrobitInputDevice>/acceleration/x
<MicrobitInputDevice>/acceleration/y
<MicrobitInputDevice>/acceleration/z
<MicrobitInputDevice>/buttonA
<MicrobitInputDevice>/buttonB
```

其中：

| 控制項 | 型別 | 說明 |
|---|---:|---|
| `acceleration` | `Vector3` | micro:bit 三軸加速度，已正規化為約 `-1 ~ 1` |
| `acceleration/x` | `float` | X 軸加速度 |
| `acceleration/y` | `float` | Y 軸加速度 |
| `acceleration/z` | `float` | Z 軸加速度 |
| `buttonA` | `Button` | micro:bit A 按鈕 |
| `buttonB` | `Button` | micro:bit B 按鈕 |

---

## 支援模式比較

| 模式 | micro:bit 程式 | Unity Source | 建議用途 |
|---|---|---|---|
| BLE 內建服務 | `microbit/makecode/ble_builtin_services.ts` | `BleWinrtMicrobitSource` | 用於無線傳輸情境 |
| USB Serial Binary | `microbit/makecode/usb_binary.ts` | `BinaryUsbSerialMicrobitSource` | 用於有線傳輸的情境 |
| USB Serial CSV | `microbit/makecode/usb_csv.ts` 或 `microbit/micropython/usb_csv.py` | `CsvStringUsbSerialMicrobitSource` | 教學、觀察資料與除錯 |

### 模式選擇建議

| 使用情境 | 建議模式 |
|---|---|
| 課堂教學、想讓學生觀察資料格式 | USB CSV |
| 要做一般用無地的有線體感遊戲 | USB Binary |
| 要做無線體感控制器或開發穿戴式應用 | BLE |
| 初學者第一次測試 | USB CSV |
| 正式展示或遊戲控制 | USB Binary 或 BLE |

---

## 系統需求

### Unity 端

- 目前使用 Unity 6.3 開發，其他版本未測試，理論上其他版本亦可使用。
- 需安裝 Unity Input System
- 建議使用 Windows 平台
- 若使用 BLE 模式，需要安裝 BleWinrtDll
- 由於有使用 SerialPort ，故需要將 Project Settings 的 Api Compatibility Level 設定為 .NET Framework

### micro:bit 端

- BBC micro:bit v1 或 v2
- MakeCode：用於 BLE 與 USB Binary Data 範例
- MicroPython：僅提供 USB CSV String 範例

### BLE 模式額外需求

- Windows 電腦需支援 Bluetooth Low Energy
- Unity 專案需安裝 BleWinrtDll
- micro:bit 需先燒錄啟用 BLE 內建服務的 MakeCode 程式

---

## Repository 結構建議

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

## 安裝方式

### 方法一：Clone Repository

```bash
git clone https://github.com/kaoshou/microbit-unity-input-system.git
```

您可以將下列資料夾直接複製到 Unity 專案中的 `Assets` 資料夾內：

```text
MicrobitUnityInputSystem
```

或是在 Unity Package Manager 中點選左上角的 `+`，選擇 `Add package from disk...`，然後選擇 `MicrobitUnityInputSystem/package.json`。

### 方法二：作為 Unity Package 使用

可在 Unity Package Manager 使用 Git URL 安裝：

```text
Window → Package Management → Package Manager → + → Add package from git URL...
```

輸入：

```text
https://github.com/kaoshou/microbit-unity-input-system.git?path=/MicrobitUnityInputSystem
```

---

## Unity 專案設定

### 1. 安裝 Unity Input System

在 Unity 中開啟：

```text
Window → Package Management → Package Manager → Unity Registry → Input System → Install
```

安裝後，Unity 可能會詢問是否啟用新的 Input System。建議允許 Unity 重新啟動。

### 2. 設定 Active Input Handling

開啟：

```text
Edit → Project Settings → Player → Other Settings → Active Input Handling
```

建議設定為：

```text
Both
```

或：

```text
Input System Package (New)
```

### 3. 設定 Api Compatibility Level

由於本專案的 USB 傳輸功能依賴 `System.IO.Ports.SerialPort`，需要將專案的 API 相容性層級設定為 `.NET Framework`。

開啟：

```text
Edit → Project Settings → Player → Other Settings → Api Compatibility Level
```

將其設定為：

```text
.NET Framework
```

### 4. 匯入本套件

直接透過 Unity Package Manager 使用 Git URL 安裝 ，或將本專案的 `MicrobitUnityInputSystem` 資料夾放入 Unity 專案中，也可以透過 Package Manager 由 disk 匯入（請參考**安裝方式**一節）。

### 5. BLE 模式安裝 BleWinrtDll

若要使用 BLE 模式，請在 Unity Package Manager 選擇：

```text
Window → Package Management → Package Manager → + → Add package from git URL...
```

輸入：

```text
https://github.com/adabru/BleWinrtDll.git?path=/BleWinrtDll-UnityPackage
```

---

## 快速開始：讀取 micro:bit 加速度與按鈕

以下範例會讀取 micro:bit X 軸加速度，控制物件左右移動，並偵測 A 按鈕是否被按下。

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

> 對於加速度這種連續感測資料，建議直接在 `Update()` 中讀取 `MicrobitInputDevice.current.acceleration.ReadValue()`。這種方式比 Input Actions callback 更適合即時移動控制。Input Actions 較適合按鈕、事件型輸入，或課程中示範 Unity Input System 綁定流程。

---

# BLE 無線模式

BLE 模式使用 micro:bit 內建的：

```text
Accelerometer Service
Button Service
```

不使用 UART。

## micro:bit 端設定

在 MakeCode 建立新專案，並請先在 MakeCode 的 設定→ 擴展 中搜尋 `bluetooth`，安裝 `bluetooth` 擴展。

回到程式編輯介面，切換到 JavaScript，貼上下列程式：

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

或直接使用本專案中的程式碼：

```text
microbit/makecode/ble_builtin_services.ts
```

將程式下載到 micro:bit 後，micro:bit 會啟用 BLE 加速度與按鈕服務。

(如果程式碼貼入後，程式碼 bluetooth 處出現錯誤，找不到該物件，則可能是因為 MakeCode 中沒有安裝 bluetooth 擴展所致。)

## Unity 場景設定

在 Unity 場景中建立一個空物件，例如：

```text
MicrobitInput
```

掛上以下 Components：

```text
MicrobitInputRuntime
BleWinrtMicrobitSource
```

`BleWinrtMicrobitSource` 建議設定：

| 設定 | 建議值 | 說明 |
|---|---|---|
| `Device Name Filter` | `micro:bit` | 掃描名稱包含 micro:bit 的 BLE 裝置 |
| `Subscribe Block` | 先不勾 | 若連線成功但收不到資料，可勾選測試 |
| `Show Debug Log` | 除錯時開啟 | 穩定後建議關閉 |

## BLE 使用注意事項

BLE 一次通常只適合由一個程式穩定連線。測試 Unity 前，請先關閉可能占用 micro:bit BLE 連線的程式。

若 Unity 顯示已連線但沒有收到資料，可依序嘗試：

1. Reset micro:bit。
2. 關閉其他 BLE 工具。
3. 關閉 Unity Play Mode 後重新進入。
4. 在 Windows Bluetooth 設定中移除 micro:bit 後重新配對。
5. 勾選 `Subscribe Block` 測試是否與訂閱流程有關。

---

# USB Binary Data 傳輸模式

USB Binary Data 模式，用於有線傳輸情境。它不是傳送 CSV 字串，而是傳送固定長度二進位封包，可減少字串切割與數值解析成本，較適合即時遊戲控制。

## micro:bit 端設定

使用專案中的 MakeCode 程式：

```text
microbit/makecode/usb_binary.ts
```

## 封包格式

USB Binary 模式使用固定 11 bytes 封包：

```text
AA 55 seq buttons xLo xHi yLo yHi zLo zHi checksum
```

| 欄位 | 說明 |
|---|---|
| `AA 55` | 封包起始標記 |
| `seq` | 序號，可用於觀察封包是否連續 |
| `buttons` | 按鈕狀態 |
| `xLo xHi` | X 軸加速度，Int16 little-endian |
| `yLo yHi` | Y 軸加速度，Int16 little-endian |
| `zLo zHi` | Z 軸加速度，Int16 little-endian |
| `checksum` | 檢查碼 |

## Unity 場景設定

在 Unity 場景中建立空物件，掛上：

```text
MicrobitInputRuntime
BinaryUsbSerialMicrobitSource
```

建議設定：

| 設定 | 建議值 |
|---|---|
| `Port Name` | `COMx` |
| `Baud Rate` | `115200` |
| `Emit Latest Every Frame` | 勾選 |
| `Show Debug Log` | 關閉 |
| `Show Rate Log` | 測試時可開啟，穩定後關閉 |

> 同一個場景中請不要同時掛上 `BinaryUsbSerialMicrobitSource`、`CsvStringUsbSerialMicrobitSource`、`BleWinrtMicrobitSource`。同一時間請只啟用一種資料來源，否則會產生衝突。

---

# USB CSV String 教學模式

USB CSV 模式最容易觀察資料內容，適合教學、除錯與初學者理解資料流。不過因為需要處理字串切割與數值解析，因此不如 USB Binary Data 模式來得流暢。

## micro:bit 端設定

可使用 MakeCode：

```text
microbit/makecode/usb_csv.ts
```

或 MicroPython：

```text
microbit/micropython/usb_csv.py
```

## 資料格式

每一列輸出格式為：

```text
x,y,z,a,b
```

範例：

```text
12,-35,980,1,0
```

| 欄位 | 說明 |
|---|---|
| `x` | X 軸加速度 |
| `y` | Y 軸加速度 |
| `z` | Z 軸加速度 |
| `a` | A 按鈕狀態 |
| `b` | B 按鈕狀態 |

## Unity 場景設定

在 Unity 場景中建立空物件，掛上：

```text
MicrobitInputRuntime
CsvStringUsbSerialMicrobitSource
```

---

## Input Actions 設定方式

本專案可以搭配 Unity Input Actions 使用。建議將 Input Actions 主要用於 **A / B 按鈕** 這類事件型輸入，例如跳躍、確認、攻擊、切換選單等。

對於 **加速度計** 這類連續感測資料，雖然可以綁定到 Input Actions，但不建議作為主要控制方式。原因是加速度資料會持續快速變化，若使用 `performed` callback 來處理移動，容易受到觸發頻率、資料抖動與事件更新時機影響，導致移動不夠穩定或反應不如預期。正式遊戲控制時，建議直接在 `Update()` 中讀取 `MicrobitInputDevice.current.acceleration.ReadValue()`，再自行進行 dead zone、平滑化或門檻判斷。

---

### 綁定 buttonA

在 Input Actions Asset 中新增一個 Action，例如：

```text
Action Name: ButtonA
Action Type: Button
Binding: <MicrobitInputDevice>/buttonA
```
---

### 綁定 buttonB

在 Input Actions Asset 中新增另一個 Action，例如：

```text
Action Name: ButtonB
Action Type: Button
Binding: <MicrobitInputDevice>/buttonB
```

---

### Input Actions Asset 設定流程

1. 在 Unity 專案中建立或開啟 `.inputactions` 檔案。
2. 新增一個 Action Map，例如：

```text
Action Map: Microbit
```

3. 在該 Action Map 底下新增 `ButtonA` 與 `ButtonB` 兩個 Action。
4. 將 `ButtonA` 設定為：

```text
Action Type: Button
Binding Path: <MicrobitInputDevice>/buttonA
```

5. 將 `ButtonB` 設定為：

```text
Action Type: Button
Binding Path: <MicrobitInputDevice>/buttonB
```

6. 儲存 Input Actions Asset。
7. 若使用 `PlayerInput` 元件，請確認該元件有指定正確的 Input Actions Asset 與 Action Map。

---

### 不建議作為主要控制：加速度計綁定

加速度計仍可在 Input Actions 中綁定，適合用於教學展示、資料觀察或簡單測試。

若要綁定完整三軸加速度，可設定為：

```text
Action Name: Acceleration
Action Type: Value
Control Type: Vector3
Binding: <MicrobitInputDevice>/acceleration
```

若只想觀察單一軸向，例如 X 軸，可設定為：

```text
Action Name: AccelerationX
Action Type: Value
Control Type: Axis
Binding: <MicrobitInputDevice>/acceleration/x
```

但在正式遊戲控制中，仍建議直接讀取 micro:bit 裝置目前的加速度值，而不是依賴 Input Actions callback。這樣較容易處理連續輸入的平滑化、靈敏度、死區與瞬間晃動判斷。

---

## 運作原理

### 整體資料流程

```text
micro:bit 感測器
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
遊戲物件控制、Input Actions、互動邏輯
```

### Unity 端

`MicrobitInputRuntime` 會建立 Unity Input System 的自訂裝置：

```text
MicrobitInputDevice
```

不論資料來源是 BLE、USB Binary 或 USB CSV，最後都會整理為：

```csharp
MicrobitInputRuntime.SubmitRawValues(rawX, rawY, rawZ, buttonA, buttonB)
```

Runtime 會將 micro:bit 原始加速度值約 `-1024 ~ 1024` 正規化為 Unity 端約 `-1 ~ 1`，再更新 Input System state。

### BLE 端

BLE 模式使用 micro:bit 內建服務：

```text
Accelerometer Service
Button Service
```

加速度資料為固定 6 bytes：

```text
X: Int16 little-endian
Y: Int16 little-endian
Z: Int16 little-endian
```

程式以 `buffer.Length` 判斷資料是否足夠，不完全依賴 BleWinrtDll 回傳的 `size`。這是因為實測中 `size` 有時小於實際 buffer 長度，若只依賴 `size`，可能發生 Unity 顯示已連線但收不到加速度資料的情況。

### USB Binary Data 端

USB Binary Data使用固定長度封包與 checksum，可避免 CSV 模式的字串切割與數值解析成本，因此較適合有線高流暢控制。

### USB CSV String 端

USB CSV String使用文字格式傳輸：

```text
x,y,z,a,b
```

優點是容易觀察、容易教學、容易用序列埠工具檢查；缺點是效率較低，正式控制時建議改用 USB Binary Data 模式。

---

## 教學應用

本套件特別適合用於體感遊戲製作課程，可讓學生理解：

- 感測器資料如何進入遊戲引擎。
- 加速度 X / Y / Z 軸與實際動作之間的關係。
- 如何把連續感測資料轉換為角色移動。
- 如何把按鈕輸入轉換為跳躍、攻擊、確認等事件。
- 為什麼即時控制需要考慮延遲、雜訊、取樣率與資料格式。
- BLE 與 USB 在穩定性、延遲與使用情境上的差異。
- Unity Input System 如何接收自訂硬體輸入裝置。

可延伸的課堂實作範例：

| 範例 | 說明 |
|---|---|
| 左右移動角色 | 使用 X 軸傾斜控制角色左右移動 |
| 跳躍判斷 | 使用加速度突變判斷甩動或跳躍 |
| 平衡遊戲 | 使用 X / Y 軸控制平台傾斜 |
| 接水果遊戲 | 使用 micro:bit 傾斜控制籃子位置 |
| 體感選單 | 使用 A / B 按鈕切換與確認 |
| 揮動觸發 | 偵測加速度峰值作為攻擊或切水果動作 |

---

## 常見問題與排除

### Unity 找不到 `MicrobitInputDevice.current`

請確認：

1. 場景中已掛上 `MicrobitInputRuntime`。
2. 有啟用至少一個資料來源 Source，例如 `BleWinrtMicrobitSource`、`BinaryUsbSerialMicrobitSource` 或 `CsvStringUsbSerialMicrobitSource`。
3. Unity Input System 已安裝並啟用。
4. Console 沒有編譯錯誤。

### BLE 顯示連線但沒有資料

請嘗試：

1. Reset micro:bit。
2. 確認 MakeCode 已啟用 `bluetooth.startAccelerometerService()`。
3. 確認 MakeCode 已啟用 `bluetooth.startButtonService()`。
4. 關閉 Bluetooth LE Explorer、nRF Connect、MakeCode Pairing 視窗等其他 BLE 工具。
5. 停止 Unity Play Mode 後重新播放。
6. 測試勾選 `Subscribe Block`。
7. 移除 Windows 已配對的 micro:bit 後重新配對。

### USB 模式收不到資料

請確認：

1. micro:bit 已透過 USB 連接。
2. Windows 裝置管理員中有出現對應 COM Port。
3. Unity Source 的 `Port Name` 設定為正確的 `COMx`。
4. Baud Rate 與 micro:bit 程式一致。
5. 沒有其他序列埠工具正在占用同一個 COM Port。
6. 場景中只啟用一個 USB Source。

### Input Actions callback 不夠即時

加速度屬於連續感測資料，建議在 `Update()` 中主動讀取：

```csharp
Vector3 accel = MicrobitInputDevice.current.acceleration.ReadValue();
```

Input Actions callback 較適合按鈕事件，不建議作為加速度移動控制的主要方式。

### `context.ReadValue<Axis>()` 發生錯誤

`AxisControl` 的值型別是 `float`，請改用：

```csharp
float x = context.ReadValue<float>();
```

或在 `Update()` 中：

```csharp
float x = moveAction.action.ReadValue<float>();
```

---

## micro:bit 使用限制

- MakeCode 可使用 BLE built-in services。
- MicroPython 範例僅提供 USB CSV，不提供與 MakeCode 相同的 BLE built-in services。
- BLE 一次通常只適合單一個程式穩定連線。
- Windows BLE 有時會快取連線狀態。
- 若突然無資料，建議先 Reset micro:bit、關閉其他 BLE 工具、重開 Unity。
- micro:bit 加速度資料會有自然抖動，正式遊戲可加入 dead zone、平滑化或門檻判斷。

---

## 開發者資訊

本專案由**崑山科技大學 鄭郁翰老師**於「體感遊戲製作」課程中設計與整理，作為 Unity、micro:bit 與體感遊戲互動設計的教學套件。

```text
開發人員：崑山科技大學 鄭郁翰（Yu-Han Cheng）
Email：kaoshou@gmail.com
GitHub：https://github.com/kaoshou
Repository：https://github.com/kaoshou/microbit-unity-input-system.git
```

---

## 貢獻方式

歡迎提出 Issue、Pull Request 或改良建議。適合貢獻的方向包含：

- 增加更多 micro:bit 遊戲控制範例。
- 增加不同 Unity 版本的相容性測試。
- 改善 BLE 連線穩定性。
- 增加 macOS 或其他平台支援。
- 增加教學文件、範例場景與課堂活動設計。
- 補充訊號平滑化、甩動偵測、姿態判斷等範例。

---

## 授權

本專案以 MIT License 開源發布。請見 [`LICENSE`](LICENSE)。

---

## 致謝

本專案使用或參考下列技術：

- [Unity](https://unity.com/)
- [Unity Input System](https://docs.unity3d.com/Packages/com.unity.inputsystem@latest)
- [BBC micro:bit](https://microbit.org/)
- [Microsoft MakeCode for micro:bit](https://makecode.microbit.org/)
- [BleWinrtDll](https://github.com/adabru/BleWinrtDll)

