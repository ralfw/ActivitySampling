using System;
using System.IO;
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

            var reqHandler = new RequestHandler(applicationDataFolderPath);
            var notifier = new Notifier();

            // bind
            var mainDlg = new MainDlg();
            mainDlg.Activity_loggged += reqHandler.Log_activity;
            mainDlg.Notifications_requested += notifier.Start;
            mainDlg.Stop_notifications_requested += notifier.Stop;
            mainDlg.Activity_changed += description => notifier.Current_activity = description;

            notifier.Notification_scheduled += mainDlg.Start_countdown;
            notifier.Countdown += mainDlg.Update_countdown;
            notifier.Notification_presented += () => { };
            notifier.Notification_acknowledged += mainDlg.Log_activity;

            Logging.Log.Append("initialization complete");

            // run
            var activities = reqHandler.Select_recent_activities(150);
            mainDlg.Display(activities);
            notifier.Start(TimeSpan.FromMinutes(5));
            Logging.Log.Append("running (almost)");

            app.Run(mainDlg);

            notifier.Dispose();
        }


        static string Ensure_application_data_folder() {
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                      "ActivitySampling");
            Directory.CreateDirectory(folder);
            return folder;
        }
    }
}
