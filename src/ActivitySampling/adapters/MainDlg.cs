using System;
using System.Collections.Generic;
using System.Linq;
using Eto.Drawing;
using Eto.Forms;


namespace ActivitySampling
{
    class MainDlg : Form
    {
        public event Action<string> Activity_loggged;
        public event Action<TimeSpan> Notifications_requested;
        public event Action Stop_notifications_requested;
        public event Action<string> Activity_changed;


        const int DEFAULT_INTERVAL_LENGTH_SEC = 5 * 60;


        int interval_length_sec = DEFAULT_INTERVAL_LENGTH_SEC;


        TextBox txtActivity;
        ListBox lstActivityLog;

        Label lblProgress;
        ProgressBar progressbar;


        ButtonMenuItem mnuStart;
        Command cmdStop;

        void cmdStart_clicked(object sender, EventArgs e) {
            var mnu = (MenuItem)sender;
            this.interval_length_sec = (int)((Command)mnu.Command).CommandParameter;

            this.Notifications_requested(TimeSpan.FromSeconds(this.interval_length_sec));

            this.mnuStart.Enabled = false;
            this.cmdStop.Enabled = !this.mnuStart.Enabled;
        }

        void cmdStop_clicked(object sender, EventArgs e) {
            this.Stop_notifications_requested();

            this.mnuStart.Enabled = true;
            this.cmdStop.Enabled = !this.mnuStart.Enabled;
        }


        void Setup_menu() {
            var aboutCommand = new Command { MenuText = "About..." };
            aboutCommand.Executed += (sender, e) => MessageBox.Show(this, "Activity Sampling v0.2");

            var quitCommand = new Command { MenuText = "Quit", Shortcut = Application.Instance.CommonModifier | Keys.Q };
            quitCommand.Executed += (sender, e) => Application.Instance.Quit();

            var preferencesCommand = new Command { MenuText = "&Preferences...", Shortcut = Application.Instance.CommonModifier | Keys.Comma };

            this.cmdStop = new Command { MenuText = "Stop", Shortcut = Application.Instance.CommonModifier | Keys.T, Enabled = false };
            this.cmdStop.Executed += cmdStop_clicked;

            this.mnuStart = new ButtonMenuItem { Text = "Start", Items = {
                    new Command{MenuText = "10 sec", CommandParameter = 10 },
                    new Command{MenuText = "5 min", CommandParameter = 5*60 },
                    new Command{MenuText = "15 min", CommandParameter = 15*60 },
                    new Command{MenuText = "25 min", CommandParameter = 25*60 },
                    new Command{MenuText = "30 min", CommandParameter = 30*60 },
                    new Command{MenuText = "60 min", CommandParameter = 60*60 },
                    new Command{MenuText = "90 min", CommandParameter = 90*60 },
                    new Command{MenuText = "120 min", CommandParameter = 120*60 }
                } };
            foreach(var item in this.mnuStart.Items) {
                item.Click += cmdStart_clicked;
            }


            Menu = new MenuBar() {
                AboutItem = aboutCommand,
                //ApplicationItems = {  preferencesCommand },
                QuitItem = quitCommand
            };

            Menu.Items.Insert(3, new ButtonMenuItem { Text = "Notifications", Items = { this.mnuStart, this.cmdStop } });
        }


        void Setup_content()
        {
            Title = "Activity Log";
            ClientSize = new Size(250, 350);

            txtActivity = new TextBox();
            this.lstActivityLog = new ListBox();
            var btnLogActivity = new Button { Text = "Log" };

            btnLogActivity.Click += (sender, e) => {
                var description = this.txtActivity.Text;
                Log_activity(description);
                this.Activity_changed(description);
                Logging.Log.Append("Activity changed to: " + description);
            };

            this.lblProgress = new Label { Text = "00:00:00", TextAlignment = TextAlignment.Center, Height = 13 };
            this.progressbar = new ProgressBar { Height = 10 };


            var layout = new TableLayout
            {
                Padding = new Padding(10), // padding around cells
                Spacing = new Size(5, 5), // spacing between each cell

                Rows = {
                    new TableRow(new Label{Text="Activity:"}),
                    new TableRow(this.txtActivity),
                    new TableRow(btnLogActivity),
                    new TableRow(this.lblProgress),
                    new TableRow(this.progressbar),
                    new TableRow(this.lstActivityLog)
                }
            };
            Content = layout;
        }


        public MainDlg() {
            Setup_menu();
            Setup_content();
        }


        public void Start_countdown(TimeSpan countdown)
        {
            this.progressbar.MaxValue = countdown.Seconds;
            Update_countdown(countdown);
        }

        public void Update_countdown(TimeSpan countdown)
        {
            this.progressbar.Value = countdown.Seconds;
            this.lblProgress.Text = countdown.ToString();
        }


        public void Log_activity(string description)
        {
            if (description == "") return;
            this.txtActivity.Text = description;
            this.lstActivityLog.Items.Insert(0, new ListItem { Text = Format_log_entry(description, DateTime.Now) });
            this.Activity_loggged(description);

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
