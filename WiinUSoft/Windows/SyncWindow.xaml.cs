using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Shared.Windows;

namespace WiinUSoft.Windows
{

    /// <summary>
    /// Interaction logic for SyncWindow.xaml
    /// </summary>
    public partial class SyncWindow : Window
    {
        public bool Cancelled { get; protected set; }
        public int Count { get; protected set; }

        bool _notCompatable = false;

        public SyncWindow()
        {
            InitializeComponent();
        }

	public void Sync()
	{
		var radioParams = new NativeImports.BLUETOOTH_FIND_RADIO_PARAMS();			
		radioParams.Initialize();
		Guid HidServiceClass = Guid.Parse(NativeImports.HID_GUID);
		IntPtr hRadio;									//handle of bluetooth radio, 							close with CloseHandle()
		IntPtr hFind;									//handle needed to pass into BluetoothFindNextRadio, 	close with BluetoothFindRadioClose()

		
		// Get first BT Radio and execute commands
		hFind = NativeImports.BluetoothFindFirstRadio(ref radioParams, out hRadio);
		if (hRadio != IntPtr.Zero)
		{
			
			//commands to execute per BT Radio
			do
			{

				Prompt("Searching for controllers...");

                // Search until cancelled or at least one device is paired
                while (Count == 0 && !Cancelled)
                {
					IntPtr found;
					var radioInfo = new NativeImports.BLUETOOTH_RADIO_INFO();
					var deviceInfo = new NativeImports.BLUETOOTH_DEVICE_INFO();
					var searchParams = new NativeImports.BLUETOOTH_DEVICE_SEARCH_PARAMS();

					radioInfo.Initialize();
					deviceInfo.Initialize();
					searchParams.Initialize();

					// Access radio information
					uint getInfoError = NativeImports.BluetoothGetRadioInfo(hRadio, ref radioInfo);

					// Success
					if (getInfoError == 0)
					{
						// Set search parameters
						searchParams.hRadio = hRadio;
						searchParams.fIssueInquiry = true;
						searchParams.fReturnUnknown = true;
						searchParams.fReturnConnected = false;
						searchParams.fReturnRemembered = false;
						searchParams.fReturnAuthenticated = false;
						searchParams.cTimeoutMultiplier = 2;

						// Search for a device
						found = NativeImports.BluetoothFindFirstDevice(ref searchParams, ref deviceInfo);

						// Success
						if (found != IntPtr.Zero)
						{
							do
							{
								// Note: Switch Pro Controller is simply called "Pro Controller"
								if (deviceInfo.szName.StartsWith("Nintendo"))
								{
									Prompt("Found " + deviceInfo.szName);

									StringBuilder password = new StringBuilder();
									uint pcService = 16;
									Guid[] guids = new Guid[16];
									bool success = true;

									// Create Password out of BT radio MAC address
									var bytes = BitConverter.GetBytes(radioInfo.address);
									for (int i = 0; i < 6; i++)
									{
										password.Append((char)bytes[i]);
									}

									// Authenticate
									if (success)
									{
										var errAuth = NativeImports.BluetoothAuthenticateDevice(IntPtr.Zero, hRadio, ref deviceInfo, password.ToString(), 6);
										success = errAuth == 0;
									}

									// Install PC Service
									if (success)
									{
										var errService = NativeImports.BluetoothEnumerateInstalledServices(hRadio, ref deviceInfo, ref pcService, guids);
										success = errService == 0;
									}

									// Set to HID service
									if (success)
									{
										var errActivate = NativeImports.BluetoothSetServiceState(hRadio, ref deviceInfo, ref HidServiceClass, 0x01);
										success = errActivate == 0;
									}

									if (success)
									{
										Prompt("Successfully Paired!");
										Count += 1;
									}
									else
									{
										Prompt("Failed to Pair.");
									}
								}
							} while (NativeImports.BluetoothFindNextDevice(found, ref deviceInfo));
						}
					}
					else
					{
						// Failed to get BT Radio info
					}
                }
				
				NativeImports.CloseHandle( hRadio );
				
			// Repeat commands if more BT Radio's exist
			} while ( NativeImports.BluetoothFindNextRadio(ref hFind, out hRadio) );
			
			//close hFind that was used with BluetoothFindNextRadio()
			NativeImports.BluetoothFindRadioClose( hFind );	
		}
		else
		{
			// No (compatable) Bluetooth
			SetPrompt(
				"No compatable Bluetooth Radios found." + 
				Environment.NewLine + 
				"This only works for the Microsoft Bluetooth Stack.");
			_notCompatable = true;
			return;
		}

            // Close this window
            Dispatcher.BeginInvoke((Action)(() => Close()));
    }


		
        private void Prompt(string text)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                prompt.Text += text + Environment.NewLine;
            }));
        }

        private void SetPrompt(string text)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                prompt.Text = text;
            }));
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_notCompatable)
            {
                Close();
            }

            Prompt("Cancelling");
            Cancelled = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!Cancelled && Count == 0 && !_notCompatable)
            {
                Cancelled = true;
                Prompt("Cancelling");
                e.Cancel = true;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Task t = new Task(() => Sync());
            t.Start();
        }
    }

}
