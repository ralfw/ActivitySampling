using System;
using System.IO;
using ActivitySampling.adapters.portals;
using ActivitySampling.adapters.providers;
using Eto.Forms;


namespace ActivitySampling
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            // build
            var app = new Application();

            var applicationDataFolderPath = Ensure_application_data_folder();
            Logging.Initialize(applicationDataFolderPath);
            Preferences.Initialize(applicationDataFolderPath);
            var pref = Preferences.Instance.Read();

            var reqHandler = new RequestHandler(pref.ActivityLogPath);
            var notifier = new Notifier(pref.Soundname);

            // bind
            var mainDlg = new MainDlg();
            mainDlg.Activity_loggged += reqHandler.Log_activity;
            mainDlg.Notifications_requested += notifier.Start;
            mainDlg.Stop_notifications_requested += notifier.Stop;
            mainDlg.Activity_changed += description => notifier.Current_activity = description;
            mainDlg.Refresh_requested += Refresh;

            notifier.Notification_scheduled += mainDlg.Start_countdown;
            notifier.Countdown += mainDlg.Update_countdown;
            notifier.Notification_presented += () => { };
            notifier.Notification_acknowledged += mainDlg.Log_activity;

            Logging.Instance.Append("initialization complete");

            // run
            Refresh();
            notifier.Start(TimeSpan.FromMinutes(pref.DefaultIntervalMin));
            Logging.Instance.Append("running (almost)");

            app.Run(mainDlg);

            notifier.Dispose();


            void Refresh() {
                var activities = reqHandler.Select_recent_activities(150);
                mainDlg.Display(activities);
            }
        }


        static string Ensure_application_data_folder() {
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                      "ActivitySampling");
            Directory.CreateDirectory(folder);
            return folder;
        }
    }
}
