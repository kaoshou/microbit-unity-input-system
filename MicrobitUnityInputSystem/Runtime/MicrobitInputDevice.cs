using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

namespace tw.yuhan.MicrobitInputSystem
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MicrobitInputState : IInputStateTypeInfo
    {
        public FourCC format => new FourCC('M', 'B', 'I', 'T');

        [InputControl(name = "acceleration", layout = "Vector3")]
        [InputControl(name = "acceleration/x", layout = "Axis")]
        [InputControl(name = "acceleration/y", layout = "Axis")]
        [InputControl(name = "acceleration/z", layout = "Axis")]
        public Vector3 acceleration;

        [InputControl(name = "buttonA", layout = "Button", bit = 0)]
        [InputControl(name = "buttonB", layout = "Button", bit = 1)]
        public byte buttons;

        [InputControl(name = "shake", layout = "Button", bit = 0)]
        [InputControl(name = "swing", layout = "Button", bit = 1)]
        [InputControl(name = "tiltLeft", layout = "Button", bit = 2)]
        [InputControl(name = "tiltRight", layout = "Button", bit = 3)]
        [InputControl(name = "tiltUp", layout = "Button", bit = 4)]
        [InputControl(name = "tiltDown", layout = "Button", bit = 5)]
        [InputControl(name = "faceUp", layout = "Button", bit = 6)]
        [InputControl(name = "faceDown", layout = "Button", bit = 7)]
        public byte gestures;
    }

    [InputControlLayout(stateType = typeof(MicrobitInputState), displayName = "micro:bit Input Device")]
    public class MicrobitInputDevice : InputDevice
    {
        public static MicrobitInputDevice current { get; private set; }

        public Vector3Control acceleration { get; private set; }
        public AxisControl x { get; private set; }
        public AxisControl y { get; private set; }
        public AxisControl z { get; private set; }
        public ButtonControl buttonA { get; private set; }
        public ButtonControl buttonB { get; private set; }

        // Gestures
        public ButtonControl shake { get; private set; }
        public ButtonControl swing { get; private set; }
        public ButtonControl tiltLeft { get; private set; }
        public ButtonControl tiltRight { get; private set; }
        public ButtonControl tiltUp { get; private set; }
        public ButtonControl tiltDown { get; private set; }
        public ButtonControl faceUp { get; private set; }
        public ButtonControl faceDown { get; private set; }

        protected override void FinishSetup()
        {
            base.FinishSetup();
            acceleration = GetChildControl<Vector3Control>("acceleration");
            x = GetChildControl<AxisControl>("acceleration/x");
            y = GetChildControl<AxisControl>("acceleration/y");
            z = GetChildControl<AxisControl>("acceleration/z");
            buttonA = GetChildControl<ButtonControl>("buttonA");
            buttonB = GetChildControl<ButtonControl>("buttonB");

            shake = GetChildControl<ButtonControl>("shake");
            swing = GetChildControl<ButtonControl>("swing");
            tiltLeft = GetChildControl<ButtonControl>("tiltLeft");
            tiltRight = GetChildControl<ButtonControl>("tiltRight");
            tiltUp = GetChildControl<ButtonControl>("tiltUp");
            tiltDown = GetChildControl<ButtonControl>("tiltDown");
            faceUp = GetChildControl<ButtonControl>("faceUp");
            faceDown = GetChildControl<ButtonControl>("faceDown");
        }

        public bool IsShake() => shake.isPressed;
        public bool IsSwing() => swing.isPressed;

        public override void MakeCurrent()
        {
            base.MakeCurrent();
            current = this;
        }

        protected override void OnRemoved()
        {
            if (current == this)
            {
                current = null;
            }
            base.OnRemoved();
        }
    }
}
