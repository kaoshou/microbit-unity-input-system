// micro:bit USB CSV Serial mode
// Easy-to-read teaching/debugging mode. For smoother control, use usb_binary.ts.

serial.redirectToUSB()
serial.setBaudRate(BaudRate.BaudRate115200)

basic.showIcon(IconNames.Yes)

basic.forever(function () {
    let x = input.acceleration(Dimension.X)
    let y = input.acceleration(Dimension.Y)
    let z = input.acceleration(Dimension.Z)
    let a = input.buttonIsPressed(Button.A) ? 1 : 0
    let b = input.buttonIsPressed(Button.B) ? 1 : 0

    serial.writeLine("" + x + "," + y + "," + z + "," + a + "," + b)
    basic.pause(50)
})
