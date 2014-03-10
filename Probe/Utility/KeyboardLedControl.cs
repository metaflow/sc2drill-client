using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Timer = System.Timers.Timer;

namespace Probe.Utility
{
    unsafe static class KeyboardLeds
    {
        const uint GenericWrite = 0x40000000;
        const uint OpenExisting = 0x3;

        const uint IoctlKeyboardQueryIndicators = 0xB0040;
        const uint IoctlKeyboardSetIndicators = 0xB0008;

        private static Timer _blinkTimer;

        enum DosDevices : uint
        {
            RawTargetPath = 0x00000001,
            RemoveDefinition = 0x00000002,
            ExactMatchOnRemove = 0x00000004,
            NoBroadcastSystem = 0x00000008,
            LuidBroadcastDrive = 0x00000010
        }

        struct KeyboardIndicatorParameters
        {
            internal ushort UnitId;     // Unit identifier.
            internal ushort LedFlags;   // LED indicator state.
        }

        private static Dictionary<Keys, bool> _blink = new Dictionary<Keys, bool>();

        public static bool Blinking(Keys key)
        {
            return (_blinkTimer != null) && _blinkTimer.Enabled;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool DefineDosDevice(
            [In] DosDevices dwFlags,
            [In] string lpDeviceName,
            [In] string lpTargetPath
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr CreateFile(
            [In] string lpFileName,
            [In] uint dwDesiredAccess,
            [In] uint dwShareMode,
            [In] IntPtr lpSecurityAttributes,
            [In] uint dwCreationDisposition,
            [In] uint dwFlagsAndAttributes,
            [In] IntPtr hTemplateFile
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CloseHandle([In] IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool DeviceIoControl(
            [In] IntPtr hDevice,
            [In] uint dwIoControlCode,
            [In] KeyboardIndicatorParameters* lpInBuffer,
            [In] uint nInBufferSize,
            [Out] KeyboardIndicatorParameters* lpOutBuffer,
            [In] uint nOutBufferSize,
            [Out] uint* lpBytesReturned,
            [In, Out] IntPtr lpOverlapped
            );

        static IntPtr hKbdDev;
        /// <summary>
        /// Должно вызываться первым для того чтобы подготовить устройство для использования.
        /// </summary>
        /// <returns>Номер ошибки.</returns>
        internal static int OpenKeyboardDevice()
        {
            if (!DefineDosDevice(DosDevices.RawTargetPath, "Kbd", "\\Device\\KeyboardClass0"))
                return Marshal.GetLastWin32Error();

            hKbdDev = CreateFile("\\\\.\\Kbd", GenericWrite, 0U, IntPtr.Zero, OpenExisting, 0U, IntPtr.Zero);

            if (hKbdDev == (IntPtr)(-1))
                return Marshal.GetLastWin32Error();

            return 0;
        }

        internal static int Close()
        {
            int err = 0;

            if (!DefineDosDevice(DosDevices.RemoveDefinition, "Kbd", null))
                err = Marshal.GetLastWin32Error();

            if (!CloseHandle(hKbdDev))
                err = Marshal.GetLastWin32Error();

            return err;
        }

        public static void Set(Keys key, bool state)
        {
            uint flag = KeysToFlag(key);
            OpenKeyboardDevice();
            KeyboardIndicatorParameters inBuff = new KeyboardIndicatorParameters() { UnitId = 0 }, outBuff = new KeyboardIndicatorParameters() {UnitId = 0};

            uint dataLen = (uint)Marshal.SizeOf(typeof(KeyboardIndicatorParameters));
            uint retLength = 0;
            
            if (DeviceIoControl(hKbdDev, IoctlKeyboardQueryIndicators, &inBuff, dataLen, &outBuff, dataLen, &retLength, IntPtr.Zero))
            {
                bool value = (outBuff.LedFlags & flag) == flag;
                if (value ^ state)
                {
                    inBuff.LedFlags = (ushort)(outBuff.LedFlags ^ flag);
                    DeviceIoControl(hKbdDev, IoctlKeyboardSetIndicators, &inBuff, dataLen, null, 0, &retLength, IntPtr.Zero);
                }
            }
            Close();
        }

        public static void DelayedSet(Keys key, bool state, int delay)
        {
            var t = new System.Timers.Timer() {AutoReset = false, Interval = delay, Enabled = false};
            t.Elapsed += delegate { Set(key, state); };
            t.Start();
        }

        public static bool Get(Keys key)
        {
            bool result = false;
            uint flag = KeysToFlag(key);
            OpenKeyboardDevice();

            KeyboardIndicatorParameters inBuff = new KeyboardIndicatorParameters() {UnitId = 0}, outBuff = new KeyboardIndicatorParameters() {UnitId = 0};
            uint dataLen = (uint)Marshal.SizeOf(typeof(KeyboardIndicatorParameters));
            uint retLength = 0;

            if (DeviceIoControl(hKbdDev, IoctlKeyboardQueryIndicators, &inBuff, dataLen, &outBuff, dataLen, &retLength, IntPtr.Zero))
            {
                result = (outBuff.LedFlags & flag) == flag;
            }
            Close();
            return result;
        }

        private static uint KeysToFlag(Keys key)
        {
            switch (key)
            {
                case Keys.Scroll:
                    return 1;
                case Keys.NumLock:
                    return 2;
                case Keys.CapsLock:
                    return 4;
                default:
                    return 1;
            }
        }

        public static void StartBlink(Keys key, int interval)
        {
            _blinkTimer = new Timer() {AutoReset = true, Enabled = false, Interval = interval};
            _blinkTimer.Elapsed += delegate { Set(key, !Get(key)); };
            _blinkTimer.Start();
        }

        public static void StopBlink()
        {
            if (_blinkTimer == null) return;
            _blinkTimer.Stop();
        }
    }
}
