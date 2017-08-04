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
        const int INTERVAL_LENGTH_SECONDS = 30 * 60;
        readonly RequestHandler reqHandler;


        TextBox txtActivity;
        ListBox lstActivityLog;


        Command cmdStart;
        Command cmdStop;

        void cmdStart_clicked(object sender, EventArgs e) {
            Schedule_notification();
        }
        void cmdStop_clicked(object sender, EventArgs e) {
            this.notificationCenter.RemoveScheduledNotification(this.notification);
            this.cmdStart.Enabled = true;
            this.cmdStop.Enabled = !this.cmdStart.Enabled;
        }


        NSUserNotification notification;
        NSUserNotificationCenter notificationCenter;

        void Schedule_notification() {
            this.notificationCenter.RemoveAllDeliveredNotifications();
            this.notification.DeliveryDate = DateTime.Now.Add(TimeSpan.FromSeconds(INTERVAL_LENGTH_SECONDS));
            this.notificationCenter.ScheduleNotification(this.notification);

            this.cmdStart.Enabled = false;
            this.cmdStop.Enabled = !this.cmdStart.Enabled;
        }
        
        void notification_delivered(object sender, EventArgs e) {
            Schedule_notification();
        }

        void notification_clicked(object sender, EventArgs e) {
            if (this.txtActivity.Text != "")
                Log_activity(this.txtActivity.Text);
            else {
                this.WindowState = WindowState.Normal;
                this.txtActivity.Focus();
            }
        }


        void Setup_menu() {
            var aboutCommand = new Command { MenuText = "About..." };
            aboutCommand.Executed += (sender, e) => MessageBox.Show(this, "Activity Sampling v0.1");

            var quitCommand = new Command { MenuText = "Quit", Shortcut = Application.Instance.CommonModifier | Keys.Q };
            quitCommand.Executed += (sender, e) => Application.Instance.Quit();

            var preferencesCommand = new Command { MenuText = "&Preferences...", Shortcut = Application.Instance.CommonModifier | Keys.Comma };

            this.cmdStart = new Command { MenuText = "Start", Shortcut = Application.Instance.CommonModifier | Keys.S, Enabled = false};
            this.cmdStart.Executed += cmdStart_clicked;
            this.cmdStop = new Command { MenuText = "Stop", Shortcut = Application.Instance.CommonModifier | Keys.T };
            this.cmdStop.Executed += cmdStop_clicked;

            Menu = new MenuBar() {
                AboutItem = aboutCommand,
                ApplicationItems = {  preferencesCommand },
                QuitItem = quitCommand
            };

            Menu.Items.Insert(3, new ButtonMenuItem { Text = "Notifications", Items = { this.cmdStart, this.cmdStop } });
        }


        void Setup_content()
        {
            Title = "Activity Log";
            ClientSize = new Size(250, 350);

            txtActivity = new TextBox();
            this.lstActivityLog = new ListBox();
            var btnLogActivity = new Button { Text = "Log" };

            btnLogActivity.Click += (sender, e) => {
                Log_activity(txtActivity.Text);
                Logging.Log.Append("Activity changed to: " + txtActivity.Text);
            };

            this.notification = new NSUserNotification { 
                Title = "What are you working on?",
                Subtitle = "",
                InformativeText = "Click for same activity as before. Open window to change it.",
                SoundName = NSUserNotification.NSUserNotificationDefaultSoundName
            };
            this.notificationCenter = NSUserNotificationCenter.DefaultUserNotificationCenter;
            this.notificationCenter.DidDeliverNotification += notification_delivered;
            this.notificationCenter.DidActivateNotification += notification_clicked;
            this.notificationCenter.ShouldPresentNotification += (_c, _n) => true; // always notify!

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

            this.UnLoad += (sender, e) => {
                this.notificationCenter.RemoveScheduledNotification(this.notification);
            };
        }


        public MainDlg(RequestHandler reqHandler)
        {
            this.reqHandler = reqHandler;

            Setup_menu();
            Setup_content();

            Schedule_notification();
        }


        public void Log_activity(string description) {
            this.reqHandler.Log_activity(description);

            this.txtActivity.Text = description;
            this.lstActivityLog.Items.Insert(0, new ListItem { Text = Format_log_entry(txtActivity.Text, DateTime.Now) });
        }


        public void Display(IEnumerable<ActivityDto> activities)
        {
            var groupedByDay = activities.GroupBy(a => a.Timestamp.ToString("yyyyMMdd")).Reverse();
            foreach(var g in groupedByDay) {
                this.lstActivityLog.Items.Add(g.First().Timestamp.ToString("D"));
                foreach (var a in g.Reverse())
                    this.lstActivityLog.Items.Add(Format_log_entry(a.Description,a.Timestamp));
            }
        }

        string Format_log_entry(string description, DateTime timestamp) => $"{timestamp:t} - {description}";
    }
}
