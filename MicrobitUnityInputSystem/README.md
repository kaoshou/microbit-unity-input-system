# micro:bit Unity Input System

> 將 BBC micro:bit 的三軸加速度與 A/B 按鈕整合到 Unity Input System，讓 micro:bit 可以像鍵盤、滑鼠、手把一樣成為 Unity 的輸入裝置。
> Integrate the BBC micro:bit accelerometer and A/B buttons into the Unity Input System, allowing the micro:bit to work as a custom Unity input device.

---

## 繁體中文 (Traditional Chinese)

### 專案說明
本專案為**崑山科技大學鄭郁翰老師**於「體感遊戲製作」課程所製作的課程教學套件，目的在於讓學生能以低成本、容易取得的 BBC micro:bit 作為體感輸入裝置，快速理解「感測器資料 → Unity 輸入系統 → 遊戲控制」的完整流程。

### 主要特色
- 支援 Unity Input System，將 micro:bit 註冊為自訂輸入裝置 `MicrobitInputDevice`。
- 支援三軸加速度 (X, Y, Z) 與 A / B 按鈕輸入。
- 支援 BLE 無線模式與 USB Serial (Binary / CSV) 有線模式。
- 適合教學使用，程式架構清楚，便於學生觀察資料流與修改擴充。

### Unity 專案設定
1. **安裝 Input System**：透過 Package Manager 安裝 Unity Input System。
2. **API 相容性設定**：開啟 `Project Settings -> Player -> Other Settings`，將 **Api Compatibility Level** 設定為 **.NET Framework** (USB Serial 功能所需)。
3. **BLE 支援**：若使用 BLE 模式，需額外安裝 [BleWinrtDll](https://github.com/adabru/BleWinrtDll)。

### 開發者資訊
- **開發人員**：崑山科技大學 鄭郁翰 (Yu-Han Cheng)
- **Email**：kaoshou@gmail.com
- **GitHub**：[https://github.com/kaoshou](https://github.com/kaoshou)

---

## English

### Project Overview
This project is a course teaching package developed by **Prof. Yu-Han Cheng at Kun Shan University** for the course **Motion Game Development**. It is designed to help students use the low-cost BBC micro:bit as a motion input device, while learning the full pipeline of "sensor data → Unity Input System → game control".

### Features
- Supports Unity Input System; registers micro:bit as a custom `MicrobitInputDevice`.
- Supports three-axis acceleration (X, Y, Z) and A / B button inputs.
- Supports BLE wireless mode and USB Serial (Binary / CSV) wired modes.
- Designed for education, with a clear structure for observing and extending data flows.

### Unity Project Setup
1. **Install Input System**: Install via Unity Package Manager.
2. **API Compatibility**: Go to `Project Settings -> Player -> Other Settings` and set **Api Compatibility Level** to **.NET Framework** (required for USB Serial).
3. **BLE Support**: Install [BleWinrtDll](https://github.com/adabru/BleWinrtDll) if using BLE mode.

### Developer Info
- **Developer**: Yu-Han Cheng, Kun Shan University
- **Email**: kaoshou@gmail.com
- **GitHub**: [https://github.com/kaoshou](https://github.com/kaoshou)

---

## 授權 / License
本專案以 MIT License 開源發布。詳見目錄下的 `LICENSE` 檔案。
This project is released under the MIT License. See the `LICENSE` file for details.
