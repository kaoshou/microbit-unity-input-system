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

        protected override void FinishSetup()
        {
            base.FinishSetup();
            acceleration = GetChildControl<Vector3Control>("acceleration");
            x = GetChildControl<AxisControl>("acceleration/x");
            y = GetChildControl<AxisControl>("acceleration/y");
            z = GetChildControl<AxisControl>("acceleration/z");
            buttonA = GetChildControl<ButtonControl>("buttonA");
            buttonB = GetChildControl<ButtonControl>("buttonB");
        }

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
