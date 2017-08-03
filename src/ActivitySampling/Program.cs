using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Eto.Forms;


namespace ActivitySampling
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            var app = new Application();

            var applicationDataFolderPath = Ensure_application_data_folder();
            Logging.Initialize(applicationDataFolderPath);
            var reqHandler = new RequestHandler(applicationDataFolderPath);
            var mainDlg = new MainDlg(reqHandler);

            var activities = reqHandler.Select_activities();
            mainDlg.Display(activities);

            app.Run(mainDlg);
        }


        static string Ensure_application_data_folder() {
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                      "ActivitySampling");
            Directory.CreateDirectory(folder);
            return folder;
        }
    }
}
