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
        public event Action Refresh_requested;


        ButtonMenuItem mnuStart;
        Command cmdStop;

        TextBox txtActivity;
        Button btnLogActivity;
        Label lblProgress;
        ProgressBar progressbar;
        ListBox lstActivityLog;


        void Setup_menu()
        {
            var aboutCommand = new Command { MenuText = "About..." };
            aboutCommand.Executed += (sender, e) => MessageBox.Show(this, "Activity Sampling v1.0.1.0");

            var quitCommand = new Command { MenuText = "Quit", Shortcut = Application.Instance.CommonModifier | Keys.Q };
            quitCommand.Executed += (sender, e) => Application.Instance.Quit();

            var preferencesCommand = new Command { MenuText = "&Preferences...", Shortcut = Application.Instance.CommonModifier | Keys.Comma };

            this.cmdStop = new Command { MenuText = "Stop", Shortcut = Application.Instance.CommonModifier | Keys.T };
            this.cmdStop.Executed += cmdStop_clicked;

            this.mnuStart = new ButtonMenuItem
            {
                Text = "Start",
                Items = {
                    new Command{MenuText = "5 min",   CommandParameter = 5*60 },
                    new Command{MenuText = "10 min",  CommandParameter = 10*60 },
                    new Command{MenuText = "15 min",  CommandParameter = 15*60 },
                    new Command{MenuText = "20 min",  CommandParameter = 20*60 },
                    new Command{MenuText = "25 min",  CommandParameter = 25*60 },
                    new Command{MenuText = "30 min",  CommandParameter = 30*60 },
                    new Command{MenuText = "45 min",  CommandParameter = 45*60 },
                    new Command{MenuText = "60 min",  CommandParameter = 60*60 },
                    new Command{MenuText = "90 min",  CommandParameter = 90*60 },
                    new Command{MenuText = "120 min", CommandParameter = 120*60 },
                    new Command{MenuText = "10 sec",  CommandParameter = 10 }
                }
            };
            foreach (var item in this.mnuStart.Items) {
                item.Click += cmdStart_clicked;
            }


            var cmdRefresh = new Command { MenuText = "Refresh", Shortcut = Keys.F5 };
            cmdRefresh.Executed += (sender, e) => this.Refresh_requested();

            Menu = new MenuBar() {
                AboutItem = aboutCommand,
                //ApplicationItems = {  preferencesCommand },
                QuitItem = quitCommand,
            };
            Menu.Items.Insert(3, new ButtonMenuItem { Text = "Notifications", Items = { this.mnuStart, this.cmdStop } });
            Menu.Items.Insert(4, new ButtonMenuItem { Text = "View", Items = { cmdRefresh } });
        }


        void Setup_content()
        {
            Title = "Activity Log";
            ClientSize = new Size(250, 350);

            this.KeyDown += perform_ENTER_default_action;

            txtActivity = new TextBox();
            txtActivity.KeyDown += perform_ENTER_default_action;

            this.lstActivityLog = new ListBox();
            this.lstActivityLog.MouseDoubleClick += (sender, e) =>
            {
                if (this.lstActivityLog.SelectedIndex >= 0)
                {
                    var li = (ListItem)this.lstActivityLog.Items[this.lstActivityLog.SelectedIndex];
                    if (li.Tag != null)
                        this.txtActivity.Text = (string)li.Tag;
                }
            };

            this.btnLogActivity = new Button { Text = "Log" };
            this.btnLogActivity.Click += (sender, e) => {
                var description = this.txtActivity.Text;

                Log_activity(description);

                this.Activity_changed(description);
                Logging.Instance.Append("Activity changed to: " + description);
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


        void cmdStart_clicked(object sender, EventArgs e) {
            this.Stop_notifications_requested();

            var mnu = (MenuItem)sender;
            var interval_length_sec = (int)((Command)mnu.Command).CommandParameter;
            this.Notifications_requested(TimeSpan.FromSeconds(interval_length_sec));
        }

        void cmdStop_clicked(object sender, EventArgs e) {
            this.Stop_notifications_requested();
        }

        void perform_ENTER_default_action(object s, KeyEventArgs e) {
            if (e.Key == Keys.Enter) {
                btnLogActivity.PerformClick();

                if (e.Shift) {
                    var timMin = new UITimer { Interval = 1 };
                    timMin.Elapsed += (_s, _e) => {
                        this.WindowState = WindowState.Minimized;
                        timMin.Stop();
                    };
                    timMin.Start();
                }
            }
        }


        public MainDlg() {
            Setup_menu();
            Setup_content();
        }


        public void Start_countdown(TimeSpan countdown) {
            this.progressbar.MaxValue = (int)countdown.TotalSeconds;
            Update_countdown(countdown);
        }

        public void Update_countdown(TimeSpan countdown) {
            this.progressbar.Value = (int)countdown.TotalSeconds;
            this.lblProgress.Text = countdown.ToString();
        }


        public void Log_activity(string description) {
            if (description == "") {
                this.WindowState = WindowState.Normal;
                this.txtActivity.Focus();
            }
            else {
                this.txtActivity.Text = description;
                this.lstActivityLog.Items.Insert(0, new ListItem { Text = Format_log_entry(description, DateTime.Now), Tag = description });

                this.Activity_loggged(description);
            }
        }


        public void Display(IEnumerable<ActivityDto> activities) {
            var groupedByDay = activities.GroupBy(a => a.Timestamp.ToString("yyyyMMdd")).Reverse();

            this.lstActivityLog.Items.Clear();
            foreach (var g in groupedByDay) {
                this.lstActivityLog.Items.Add(g.First().Timestamp.ToString("D"));
                foreach (var a in g.Reverse()) {
                    var li = new ListItem { Text = Format_log_entry(a.Description, a.Timestamp), Tag = a.Description };
                    this.lstActivityLog.Items.Add(li);
                }
            }

            if (activities.Any()) {
                var mostRecentDescription = activities.Last().Description;
                this.txtActivity.Text = mostRecentDescription;

                this.Activity_changed(mostRecentDescription);
            }
        }


        string Format_log_entry(string description, DateTime timestamp) => $"{timestamp:t} - {description}";
    }
}
