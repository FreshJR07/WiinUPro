using System;
using System.Windows;
using System.Windows.Controls;

namespace WiinUSoft
{
    /// <summary>
    /// Interaction logic for PropWindow.xaml
    /// </summary>
    public partial class PropWindow : Window
    {
        public bool doSave = false;
        public bool customCalibrate = false;
        public Property props;

        PropWindow(Property org) : this(org, "Controller") { }

        public PropWindow(Property org, string defalutName)
        {
            InitializeComponent();

            props = new Property(org);
            nameInput.Text = string.IsNullOrWhiteSpace(props.name) ? defalutName : props.name;
            defaultInput.Text = props.profile;
            autoCheckbox.IsChecked = props.autoConnect;
            if (props.autoNum >= 0 && props.autoNum <= autoConnectNumber.Items.Count)
            {
                autoConnectNumber.SelectedIndex = props.autoNum;
            }
            switch (props.idleDisconnect)
            {
                case -1:        //disabled
                    idleDisconnectTime.SelectedIndex = 0;
                    break;
                case 30000:     //30 sec
                    idleDisconnectTime.SelectedIndex = 1;
                    break;
                case 60000:     //1 min
                    idleDisconnectTime.SelectedIndex = 2;
                    break;
                case 120000:    //2 min
                    idleDisconnectTime.SelectedIndex = 3;
                    break;
                case 180000:    //3 min
                    idleDisconnectTime.SelectedIndex = 4;
                    break;
                case 300000:    //5 min
                    idleDisconnectTime.SelectedIndex = 5;
                    break;
                case 600000:    //10 min
                    idleDisconnectTime.SelectedIndex = 6;
                    break;
                case 900000:    //15 min
                    idleDisconnectTime.SelectedIndex = 7;
                    break;
                default:
                    idleDisconnectTime.SelectedIndex = 4;
                    break;

            }
            if (props.rumbleIntensity >= 0 && props.rumbleIntensity <= rumbleSelection.Items.Count)
            {
                rumbleSelection.SelectedIndex = props.rumbleIntensity;
            }
            switch (props.calPref)
            {
                case Property.CalibrationPreference.Default:
                    calibrationSelection.SelectedIndex = 0;
                    break;
                case Property.CalibrationPreference.Minimal:
                    calibrationSelection.SelectedIndex = 1;
                    break;
                case Property.CalibrationPreference.More:
                    calibrationSelection.SelectedIndex = 2;
                    break;
                case Property.CalibrationPreference.Extra:
                    calibrationSelection.SelectedIndex = 3;
                    break;
                case Property.CalibrationPreference.Custom:
                    calibrationSelection.SelectedIndex = 4;
                    break;
            }
            pointerSelection.SelectedIndex = (int)org.pointerMode;
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            customCalibrate = false;
            Close();
        }

        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            customCalibrate = false;
            doSave = true;
            Close();
        }

        private void autoCheckbox_Click(object sender, RoutedEventArgs e)
        {
            props.autoConnect = autoCheckbox.IsChecked == true;
        }

        private void nameInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            props.name = nameInput.Text;
        }

        private void defaultInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            props.profile = defaultInput.Text;
        }

        private void defaultBtn_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.DefaultExt = ".wsp";
            dialog.Filter = App.PROFILE_FILTER;

            Nullable<bool> doLoad = dialog.ShowDialog();

            if (doLoad == true && dialog.CheckFileExists)
            {
                defaultInput.Text = dialog.FileName;
            }
        }

        private void AutoConnect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (props != null)
            {
                props.autoConnect = autoConnectNumber.SelectedIndex > 0;
                props.autoNum = autoConnectNumber.SelectedIndex;
            }
        }

        private void IdleDisconnect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (props != null)
            {
                switch (idleDisconnectTime.SelectedIndex)
                {
                    case 0:
                        props.idleDisconnect = -1;
                        break;
                    case 1:
                        props.idleDisconnect = 30000;
                        break;
                    case 2:
                        props.idleDisconnect = 60000;
                        break;
                    case 3:
                        props.idleDisconnect = 120000;
                        break;
                    case 4:
                        props.idleDisconnect = 180000;
                        break;
                    case 5:
                        props.idleDisconnect = 300000;
                        break;
                    case 6:
                        props.idleDisconnect = 600000;
                        break;
                    case 7:
                        props.idleDisconnect = 900000;
                        break;
                }
            }
        }
        

        private void Rumble_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (props != null)
            {
                props.useRumble = rumbleSelection.SelectedIndex > 0;
                props.rumbleIntensity = rumbleSelection.SelectedIndex;
            }
        }

        private void Calibration_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (props != null)
            {
                switch (calibrationSelection.SelectedIndex)
                {
                    case 0:
                        props.calPref = Property.CalibrationPreference.Default;
                        customCalibrate = false;
                        break;

                    case 1:
                        props.calPref = Property.CalibrationPreference.Minimal;
                        customCalibrate = false;
                        break;

                    case 2:
                        props.calPref = Property.CalibrationPreference.More;
                        customCalibrate = false;
                        break;

                    case 3:
                        props.calPref = Property.CalibrationPreference.Extra;
                        customCalibrate = false;
                        break;

                    case 4:
                        //props.calPref = Property.CalibrationPreference.Custom;
                        //customCalibrate = true;
                        //Hide();
                        break;
                }
            }
        }

        private void calibrationSelection_DropDownClosed(object sender, EventArgs e)
        {
            if (props != null && calibrationSelection.SelectedIndex == 4)
            {
                props.calPref = Property.CalibrationPreference.Custom;
                customCalibrate = true;
                Hide();
            }
        }

        private void pointerSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (props != null)
            {
                props.pointerMode = (Property.PointerOffScreenMode)pointerSelection.SelectedIndex;
            }
        }
    }
}
