using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Microsoft.Win32.TaskScheduler;       //not acutally made by microsoft "http://taskscheduler.codeplex.com/"

namespace WiinUSoft
{
    public class UserPrefs
    {
        private static UserPrefs _instance;
        
        //loads prefs in %AppData% if they exist or creates instance with default values
        public static UserPrefs Instance
        {
            get
            {
                if (_instance == null)
                {
                    if (File.Exists(DataPath))
                    {
                        LoadPrefs();
                    }
                    else
                    {
                        _instance = new UserPrefs();
                        _instance.devicePrefs = new List<Property>();
                        _instance.defaultProfile = new Profile();
                        // we could, but just in case lets not
                        //_instance.greedyMode = Environment.OSVersion.Version.Major < 10; 
                        //_instance.toshibaMode = !Shared.Windows.NativeImports.BluetoothEnableDiscovery(IntPtr.Zero, true);
                        SavePrefs();
                    }
                }
                return _instance;
            }
        }

        public static string DirectoryPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\WiinUSoft\";
        public static string DataPath = DirectoryPath + "prefs.config";
        
        //creates a task in task scheduler for autoStartup
        public static bool AutoStart
        {
            get { return Instance.autoStartup; }
            set
            {
                //note: created task is called WiinUSoft (USERNAME)
                //(USERNAME) is appended because windows cannot have duplicate task names
                //duplicate task names would occur if multiple users existed on a computer and would each enable auto startup
                string taskname = "WiinUSoft (" + Environment.UserName + ")" ;
                
                //connect to task service
                using ( TaskService ts = new TaskService() )
                {
                    //create task
                    if (value)
                    {
                        TaskDefinition td = ts.NewTask();
                            td.RegistrationInfo.Author = "WiinUSoft";
                            td.RegistrationInfo.Description = "Run WiinUSoft at Startup ";
                            td.Principal.LogonType = TaskLogonType.InteractiveToken;
                            td.Settings.ExecutionTimeLimit = System.TimeSpan.Zero; // needed so program does not default to 72hr time limit and will run forever
                            td.Settings.MultipleInstances = TaskInstancesPolicy.IgnoreNew; //do not start new instance if already running
                            td.Settings.DisallowStartIfOnBatteries = false;
                            td.Settings.StopIfGoingOnBatteries = false;
                            td.Settings.WakeToRun = false;
                            td.Settings.StartWhenAvailable = true;
                            LogonTrigger lTrigger = (LogonTrigger)td.Triggers.Add(new LogonTrigger());
                                lTrigger.UserId = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                            td.Actions.Add(new ExecAction(System.Reflection.Assembly.GetExecutingAssembly().Location, "-m", null));
                        //register task
                        ts.RootFolder.RegisterTaskDefinition(taskname, td, TaskCreation.CreateOrUpdate, null, null, TaskLogonType.InteractiveToken, null);
                    }
                    //delete task
                    else
                    {
                        if (ts.GetTask(taskname) != null) 
                        {
                            ts.RootFolder.DeleteTask(taskname);
                        }
                    }
                }
                Instance.autoStartup = value;
            }
        }

        public List<Property> devicePrefs;
        public Profile defaultProfile;
        public Property defaultProperty;
        public bool autoStartup;
        public bool startMinimized;
        public bool greedyMode;
        public bool toshibaMode;
        public bool autoRefresh = true;
        public bool autoXInput = true;
        public bool minimizeOnExit = true;
        public double WindowTop = 0;
        public double WindowLeft = 0;

        public UserPrefs()
        { }

        public static bool LoadPrefs()
        {
            bool successful = false;
            XmlSerializer X = new XmlSerializer(typeof(UserPrefs));
            
            try
            {
                if (File.Exists(DataPath))
                {
                    using (FileStream stream = File.OpenRead(DataPath))
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        _instance = X.Deserialize(reader) as UserPrefs;
                        reader.Close();
                        stream.Close();
                    }

                    successful = true;

                    if (_instance != null && _instance.devicePrefs != null)
                        _instance.defaultProperty = _instance.devicePrefs.Find((p) => p.hid.ToLower().Equals("all"));
                }
            }
            catch (Exception e) 
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }

            return successful;
        }

        public static bool SavePrefs()
        {
            bool successful = false;
            XmlSerializer X = new XmlSerializer(typeof(UserPrefs));

            try
            {
                if (File.Exists(DataPath))
                {
                    FileInfo prefs = new FileInfo(DataPath);
                    using (FileStream stream = File.Open(DataPath, FileMode.Create, FileAccess.ReadWrite))
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        X.Serialize(writer, _instance);
                        writer.Close();
                        stream.Close();
                    }
                }
                else
                {
                    if ( !Directory.Exists(DirectoryPath) )
                    {
                        Directory.CreateDirectory(DirectoryPath);
                    }
                    using (FileStream stream = File.Create(DataPath))
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        X.Serialize(writer, _instance);
                        writer.Close();
                        stream.Close();
                    }
                }

                successful = true;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }

            return successful;
        }

        public Property GetDevicePref(string hid)
        {
            foreach (var pref in devicePrefs)
            {
                if (pref.hid == hid)
                {
                    return pref;
                }
            }

            return defaultProperty;
        }

        public void AddDevicePref(Property property)
        {
            foreach (var pref in devicePrefs)
            {
                if (pref.hid == property.hid)
                {
                    pref.name            = property.name;
                    pref.autoConnect     = property.autoConnect;
                    pref.idleDisconnect  = property.idleDisconnect;
                    pref.profile         = property.profile;
                    pref.connType        = property.connType;
                    pref.autoNum         = property.autoNum;
                    pref.rumbleIntensity = property.rumbleIntensity;
                    pref.useRumble       = property.useRumble;
                    pref.calPref         = property.calPref;

                    return;
                }
            }

            devicePrefs.Add(property);
        }

        public void UpdateDeviceIcon(string path, string icon)
        {
            var prop = devicePrefs.FindIndex((p) => p.hid == path);

            if (prop >= 0)
            {
                devicePrefs[prop].lastIcon = icon;
                SavePrefs();
            }
        }

        public string GetDeviceIcon(string path)
        {
            var prop = devicePrefs.FindIndex((p) => p.hid == path);

            if (prop >= 0)
            {
                return devicePrefs[prop].lastIcon;
            }

            return "";
        }
    }

    public class Property
    {
        public enum ProfHolderType
        {
            XInput = 0,
            DInput = 1
        }

        public enum CalibrationPreference
        {
            Raw     = -2,
            Minimal = -1,
            Default = 0,
            Defalut = 0,
            More    = 1,
            Extra   = 2,
            Custom  = 3
        }

        public enum PointerOffScreenMode
        {
            Center = 0,
            SnapX  = 1,
            SnapY  = 2,
            SnapXY = 3
        }

        public string hid = "";
        public string name = "";
        public string lastIcon = "";
        public bool autoConnect = true;
        public int idleDisconnect = 180000;          //3 min converted into milliseconds
        public bool useRumble = true;
        public int autoNum = 5;
        public int rumbleIntensity = 2;
        public ProfHolderType connType;
        public string profile = "";
        public CalibrationPreference calPref;
        public string calString = ""; // not the best solution for saving the custom config but makes it easy
        public PointerOffScreenMode pointerMode = PointerOffScreenMode.Center;

        public Property()
        {
            hid = "";
            connType = ProfHolderType.XInput;
            calPref = CalibrationPreference.Default;
            pointerMode = PointerOffScreenMode.Center;
        }

        public Property(string ID)
        {
            hid = ID;
            connType = ProfHolderType.XInput;
            calPref = CalibrationPreference.Default;
            pointerMode = PointerOffScreenMode.Center;
        }

        public Property(Property copy)
        {
            hid = copy.hid;
            name = copy.name;
            autoConnect = copy.autoConnect;
            idleDisconnect = copy.idleDisconnect;
            autoNum = copy.autoNum;
            useRumble = copy.useRumble;
            rumbleIntensity = copy.rumbleIntensity;
            connType = copy.connType;
            profile = copy.profile;
            calPref = copy.calPref;
            calString = copy.calString;
            pointerMode = copy.pointerMode;
        }
    }

    public class Profile
    {
        public enum HolderType
        {
            XInput = 0,
            DInput = 1
        }

        public NintrollerLib.ControllerType profileType;
        public HolderType connType;
        public List<string> controllerMapKeys;
        public List<string> controllerMapValues;

        public Profile()
        {
            profileType = NintrollerLib.ControllerType.Wiimote;
            controllerMapKeys = new List<string>();
            controllerMapValues = new List<string>();
            connType = HolderType.XInput;
        }

        public Profile(NintrollerLib.ControllerType type)
        {
            profileType = type;
            controllerMapKeys = new List<string>();
            controllerMapValues = new List<string>();
            connType = HolderType.XInput;
        }
    }

}
