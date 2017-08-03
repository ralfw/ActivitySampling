using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Eto.Drawing;
using Eto.Forms;
using MonoMac.Foundation;

namespace ActivitySampling
{
    class MainDlg : Form
    {
        readonly RequestHandler reqHandler;

        ListBox lstActivityLog;


        void Setup_menu() {
            var aboutCommand = new Command { MenuText = "About..." };
            aboutCommand.Executed += (sender, e) => MessageBox.Show(this, "Activity Sampling v0.1");

            var quitCommand = new Command { MenuText = "Quit", Shortcut = Application.Instance.CommonModifier | Keys.Q };
            quitCommand.Executed += (sender, e) => Application.Instance.Quit();

            var preferencesCommand = new Command { MenuText = "&Preferences...", Shortcut = Application.Instance.CommonModifier | Keys.Comma };

            Menu = new MenuBar() {
                AboutItem = aboutCommand,
                ApplicationItems = {  preferencesCommand },
                QuitItem = quitCommand,
            };
        }



        void Setup_content() {
            Title = "Activity Log";
            ClientSize = new Size(250, 350);

            var txtActivity = new TextBox();
            this.lstActivityLog = new ListBox();
            var btnLogActivity = new Button { Text = "Log" };

            btnLogActivity.Click += (sender, e) => {
                this.reqHandler.Log_activity(txtActivity.Text);
                lstActivityLog.Items.Insert(0, new ListItem{Text = Format_log_entry(txtActivity.Text, DateTime.Now)});
            };


            var layout = new TableLayout
            {
                Padding = new Padding(5), // padding around cells
                Spacing = new Size(5, 5), // spacing between each cell

                Rows = {
                    new TableRow(new Label{Text="Activity:"}),
                    new TableRow(txtActivity),
                    new TableRow(btnLogActivity),
                    new TableRow(lstActivityLog)
                }
            };

            Content = layout;
        }


        public MainDlg(RequestHandler reqHandler)
        {
            this.reqHandler = reqHandler;

            Setup_menu();
            Setup_content();
        }


        public void Display(IEnumerable<ActivityDto> activities)
        {
            var groupedByDay = activities.GroupBy(a => a.Timestamp.ToString("yyyyMMdd"));
            foreach(var g in groupedByDay.Reverse()) {
                this.lstActivityLog.Items.Add(g.First().Timestamp.ToString("D"));
                foreach (var a in g.Reverse())
                    this.lstActivityLog.Items.Add(Format_log_entry(a.Description,a.Timestamp));
            }

        }

        string Format_log_entry(string description, DateTime timestamp) => $"{timestamp:t} - {description}";
    }
}
