// micro:bit BLE built-in services mode
// Recommended wireless mode for Unity.
// Unity reads Accelerometer Service and Button Service directly.

bluetooth.startAccelerometerService()
bluetooth.startButtonService()

basic.showIcon(IconNames.SmallDiamond)

bluetooth.onBluetoothConnected(function () {
    basic.showIcon(IconNames.Yes)
})

bluetooth.onBluetoothDisconnected(function () {
    basic.showIcon(IconNames.No)
})
