# 運作原理

本專案並非讓 micro:bit 偽裝成標準的 Xbox 手把或 Gamepad，而是在 Unity Input System 中建立一個自訂裝置 `MicrobitInputDevice`。

資料流如下：

```text
micro:bit (BLE / USB Binary / USB CSV)
→ Unity Source Component (資料來源組件)
→ MicrobitInputRuntime (管理裝置生命週期)
→ MicrobitInputDevice (Unity Input System 自訂裝置)
→ 遊戲程式 (讀取 acceleration / buttonA / buttonB)
```

## 技術細節

### 1. BLE 無線模式
BLE 使用 micro:bit 內建的 Accelerometer Service 與 Button Service。
- 加速度 Characteristic：固定 6 bytes (X/Y/Z 三個 Int16 little-endian)。
- 解析機制：本專案以 `buffer.Length` 判斷資料完整性，不完全依賴 `BleWinrtDll` 的 `size` 回傳值，以確保在資料長度波動時仍能穩定讀取。

### 2. USB Binary 有線模式
使用固定 11 bytes 封包傳輸：
- 包含 Header (0xAA 0x55)、序號 (Seq)、按鈕狀態、X/Y/Z 數值 (Int16) 與 Checksum。
- 優點：解析效率高、延遲低，比字串解析更適合即時體感控制。

### 3. USB CSV 教學模式
格式為 `x,y,z,a,b`。
- 優點：適合初學者使用序列埠監控軟體觀察資料。
- 缺點：字串切割與數值解析較耗能。

## 依賴與相容性

### 1. .NET Framework 的必要性
由於 Unity 的 `.NET Standard 2.1` 設定在部分環境下對 `System.IO.Ports.SerialPort` 的支援不完整，為確保 USB Serial 連線的穩定性與跨版本相容性，本專案建議將 **Api Compatibility Level** 設定為 **.NET Framework**。

### 2. BLE 支援
無線連線依賴 [BleWinrtDll](https://github.com/adabru/BleWinrtDll)，這是一個專為 Windows 平台設計的 BLE 串接庫。
