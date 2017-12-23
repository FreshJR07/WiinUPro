using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using static Shared.Windows.NativeImports;
using System.Runtime.InteropServices;

namespace Shared.Windows
{
    public class DeviceListener
    {
        #region Class Constants
        // https://msdn.microsoft.com/en-us/library/aa363480(v=vs.85).aspx
        private const int DbtDeviceArrival         = 0x8000;  // system detected a new device        
        private const int DbtDeviceRemoveComplete  = 0x8004;  // device is gone     
        private const int DbtDevNodesChanged       = 0x0007;  // A device has been added to or removed from the system.
        private const int WmDevicechange           = 0x0219;  // device change event message
        private const int DbtDevtypDeviceinterface = 5;
        // https://msdn.microsoft.com/en-us/library/windows/hardware/ff553426(v=vs.85).aspx
        public static readonly Guid GuidInterfaceHID= new Guid("4D1E55B2-F16F-11CF-88CB-001111000030");

        #endregion
        private IntPtr notificationHandle;

        
        public static DeviceListener Instance { get; private set; }

        public event Action OnDevicesUpdated;


        static DeviceListener()
        {
            Instance = new DeviceListener();
        }

        private DeviceListener() { }
        
        public void RegisterDeviceNotification(Window window)
        {
            //get main window handle and hook its message events
            var source = HwndSource.FromHwnd(new WindowInteropHelper(window).Handle);
            IntPtr windowHandle = source.Handle;
            source.AddHook(HwndHandler);

            //create filter to find bluetooth WmDevicechange messages only
            DevBroadcastDeviceInterface deviceInterface = new DevBroadcastDeviceInterface
            {
                ClassGuid = GuidInterfaceHID,
                DeviceType = DbtDevtypDeviceinterface,
            };
            deviceInterface.Size = Marshal.SizeOf(deviceInterface);
            IntPtr buffer = Marshal.AllocHGlobal(deviceInterface.Size);
            Marshal.StructureToPtr(deviceInterface, buffer, false);

            //send messages of these filtered events to main window
            notificationHandle = RegisterDeviceNotification(windowHandle, buffer, DEVICE_NOTIFY.WINDOWS_HANDLE);
        }
        
        public void UnregisterDeviceNotification()
        {
            UnregisterDeviceNotification(notificationHandle);
        }

        private IntPtr HwndHandler(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled)
        {
            // Only checking changed event since it gets called when devices are added and removed
            // while remove notifications don't always get called.
            if (msg == WmDevicechange)
            {
                if ( (int)wparam == DbtDeviceArrival || (int)wparam == DbtDeviceRemoveComplete )
                {
                    if (lparam != IntPtr.Zero)
                    {
                        //Get Device name
                        DevBroadcastDeviceInterface deviceInterface = (DevBroadcastDeviceInterface)Marshal.PtrToStructure(lparam, typeof(DevBroadcastDeviceInterface));
                        string deviceName = "";
                        deviceName = System.Text.Encoding.Unicode.GetString(deviceInterface.Name);          //convert byte array into unicode
                        deviceName = deviceName.Replace("\0", string.Empty);                                //removes all unicode null "\0" charecters

                        if ( deviceName.Contains("057e") )      // nintendo VID = 057e
                        {
                            OnDevicesUpdated?.Invoke();
                        }
                    }
                }
            }

            handled = false;
            return IntPtr.Zero;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr RegisterDeviceNotification(IntPtr recipient, IntPtr notificationFilter, DEVICE_NOTIFY flags);

        [DllImport("user32.dll")]
        private static extern bool UnregisterDeviceNotification(IntPtr handle);

        [DllImport("kernel32.dll")]
        public static extern bool Beep(int freq,int duration);

        [StructLayout(LayoutKind.Sequential)]
        private struct DevBroadcastDeviceInterface
        {
            internal int Size;
            internal int DeviceType;
            internal int Reserved;
            internal Guid ClassGuid;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)] internal byte[] Name;
        }
        
        internal enum DEVICE_NOTIFY
        {

            WINDOWS_HANDLE = 0,

            SERVICE_HANDLE = 1,

        }
    }
}
