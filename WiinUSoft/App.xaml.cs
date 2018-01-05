using Microsoft.Shell;
using System;
using System.Collections.Generic;
using System.Windows;

namespace WiinUSoft
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, ISingleInstanceApp
    {
        internal const string PROFILE_FILTER = "WiinUSoft Profile|*.wsp";
        private const string Unique = "wiinupro-or-wiinusoft-instance";

        [STAThread]
        public static void Main()
        {
            //first code executed upon application start
            string[] args = Environment.GetCommandLineArgs();
            bool rflag = false;
            foreach (string arg in args)
            {
                if ( arg == "-r" )
                {
                    rflag = true;
                }
            }
            
            //starts MainWindow only if no instance already exists
            if (SingleInstance<App>.InitializeAsFirstInstance(Unique))
            {
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

                var application = new App();
                application.InitializeComponent();
                application.Run( );
                SingleInstance<App>.Cleanup();
            }
            //if -r (restart) flag is used it will simultanoesuly send shutdown command to first instance and
            //start waiting for first instance to close, upto 5sec, before starting MainWindow
            else if (rflag == true)
            {
                SingleInstance<App>.Cleanup();
                for (int i = 0; i <= 20; i++)
                {
                    try
                    {      
                        if (SingleInstance<App>.InitializeAsFirstInstance(Unique))
                        {
                            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

                            var application = new App();
                            application.InitializeComponent();
                            application.Run( );
                            break;
                        }
                    }
                    catch
                    {
                        SingleInstance<App>.Cleanup();
                        if (i >= 20)
                        {
                            MessageBox.Show("Timeout waiting for original WiinUSoft instance to close");
                        }
                    }
                    System.Threading.Thread.Sleep(250);
                }
                SingleInstance<App>.Cleanup();
            }

            //code below will be exeucted after MainWindow closes OR
            //immedietly in the case when MainWindow is be created
            //and is executed before termination

        }


        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            //code below will be executed, ON 1RST INSTANCE OF APPLICATION,
            //when the 2nd instance InitializeAsFirstInstance throws an error
            //this situation is used to to notify the 1rst instance of the flags/args attemped to be used with the 2nd instance

           // MessageBox.Show("App1 Here");
            //process args
            bool mflag = false;         //start minimized flag            
            bool rflag = false;         //restart instance
            foreach (string arg in args)
            {
                if ( arg == "-m" )
                {
                    mflag = true;
                }
                
                if ( arg == "-r" )
                {
                    rflag = true;
                }

            }

            //if 2nd instance was started with restart instance flag, shut down 1rst instance
            if (rflag == true)
            {
                Application.Current.Shutdown();
            }

            //if 2nd instance was not called with start minimized flag, pull up original instance and give it focus
            if (mflag != true)
            {
                ((MainWindow)this.MainWindow).ShowWindow();
                this.MainWindow.Activate();
            }

            return true;
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            
            SingleInstance<App>.Cleanup();
            Current.Dispatcher.Invoke(new Action(() => 
            {
                var box = new ErrorWindow(e);
                box.ShowDialog();
            }));
        }
    }
}
