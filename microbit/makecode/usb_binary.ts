// micro:bit USB Binary Serial mode
// High-fluidity wired mode for Unity.
// Frame: AA 55 seq buttons x:int16 y:int16 z:int16 checksum

serial.redirectToUSB()
serial.setBaudRate(BaudRate.BaudRate115200)

basic.showIcon(IconNames.Yes)

let seq = 0

basic.forever(function () {
    let x = input.acceleration(Dimension.X)
    let y = input.acceleration(Dimension.Y)
    let z = input.acceleration(Dimension.Z)

    let buttons = 0
    if (input.buttonIsPressed(Button.A)) {
        buttons = buttons | 0x01
    }
    if (input.buttonIsPressed(Button.B)) {
        buttons = buttons | 0x02
    }

    let buf = pins.createBuffer(11)
    buf.setNumber(NumberFormat.UInt8LE, 0, 0xAA)
    buf.setNumber(NumberFormat.UInt8LE, 1, 0x55)
    buf.setNumber(NumberFormat.UInt8LE, 2, seq)
    buf.setNumber(NumberFormat.UInt8LE, 3, buttons)
    buf.setNumber(NumberFormat.Int16LE, 4, x)
    buf.setNumber(NumberFormat.Int16LE, 6, y)
    buf.setNumber(NumberFormat.Int16LE, 8, z)

    let checksum = 0
    for (let i = 0; i < 10; i++) {
        checksum = (checksum + buf.getNumber(NumberFormat.UInt8LE, i)) & 0xFF
    }
    buf.setNumber(NumberFormat.UInt8LE, 10, checksum)

    serial.writeBuffer(buf)
    seq = (seq + 1) & 0xFF

    basic.pause(20)
})
