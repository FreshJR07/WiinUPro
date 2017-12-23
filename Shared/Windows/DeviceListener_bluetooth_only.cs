using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using static Shared.Windows.NativeImports;

namespace Shared.Windows
{
    public class DeviceListener
    {
        #region Class Constants
        // https://msdn.microsoft.com/en-us/library/aa363480(v=vs.85).aspx
        private const int WmDevicechange           = 0x0219;  // device change event message
        // https://msdn.microsoft.com/en-us/library/windows/hardware/ff553426(v=vs.85).aspx
        public static readonly Guid GUID_BLUETOOTH_HCI_EVENT	             = new Guid("FC240062-1541-49BE-B463-84C4DCD7BF7F");
        public static readonly Guid GUID_BLUETOOTH_L2CAP_EVENT               = new Guid("7EAE4030-B709-4AA8-AC55-E953829C9DAA");
        public static readonly Guid EMPTY                                    = new Guid("00000000-0000-0000-0000-000000000000");

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
            var radioParams = new NativeImports.BLUETOOTH_FIND_RADIO_PARAMS();
            radioParams.Initialize();
            IntPtr hRadio;
            IntPtr hFind = NativeImports.BluetoothFindFirstRadio(ref radioParams, out hRadio);

            if (hRadio != IntPtr.Zero)
            {
                do
			    {
                    DEV_BROADCAST_HANDLE filter = new DEV_BROADCAST_HANDLE();
                    filter.dbch_size = Marshal.SizeOf(filter);
                    filter.dbch_handle = hRadio;
                    filter.dbch_devicetype = DBT_DEVTYP.HANDLE;
                    filter.dbch_eventguid = EMPTY;
                        

                    //send messages of these filtered events to main window
                    notificationHandle = RegisterDeviceNotification(windowHandle, ref filter, DEVICE_NOTIFY.WINDOWS_HANDLE);

                    NativeImports.CloseHandle( hRadio );

                } while ( NativeImports.BluetoothFindNextRadio(ref hFind, out hRadio) );
                NativeImports.BluetoothFindRadioClose( hFind );	
            }
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

                if (lparam != IntPtr.Zero)
                {

                    DEV_BROADCAST_HANDLE dbh = (DEV_BROADCAST_HANDLE)Marshal.PtrToStructure(lparam, typeof(DEV_BROADCAST_HANDLE));
                    if (dbh.dbch_eventguid == GUID_BLUETOOTH_HCI_EVENT)
                    {
                        // BTH_HCI_EVENT_INFO
                         OnDevicesUpdated?.Invoke();
                    }
                    else if (dbh.dbch_eventguid == GUID_BLUETOOTH_L2CAP_EVENT)
                    {
                        // BTH_L2CAP_EVENT_INFO (not needed as controller connection triggers both HCI & L2CAP EVENTS)
                    }
                }

            }

            handled = false;
            return IntPtr.Zero;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr RegisterDeviceNotification(IntPtr handle, ref DEV_BROADCAST_HANDLE notificationFilter, DEVICE_NOTIFY flags);

        [DllImport("user32.dll")]
        private static extern bool UnregisterDeviceNotification(IntPtr handle);
        
        [StructLayout(LayoutKind.Sequential)]
        private struct DevBroadcastDeviceInterface
        {
            internal int Size;
            internal int DeviceType;
            internal int Reserved;
            internal Guid ClassGuid;
            internal short Name;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct DEV_BROADCAST_HANDLE
        {
            internal int dbch_size;
            internal DBT_DEVTYP dbch_devicetype;
            private uint dbch_reserved;
            internal IntPtr dbch_handle;
            internal IntPtr dbch_hdevnotify;
            internal Guid dbch_eventguid;
            internal int nameoffset;
        }

        internal enum DBT_DEVTYP
        {
            HANDLE = 6,
        }

        internal enum DEVICE_NOTIFY
        {
            WINDOWS_HANDLE = 0,
            SERVICE_HANDLE = 1,
        }

    }
}
