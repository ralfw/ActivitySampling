using System;
using System.IO;
using Eto.Drawing;
using Eto.Forms;
using MonoMac.Foundation;

namespace ActivitySampling
{
    public class steinbruch : Form
    {
        UITimer timer = new UITimer();
        UITimer tick = new UITimer();

        public steinbruch()
        {
            Title = "Activity Tracker i1";
            ClientSize = new Size(180, 250);

            var progress = new ProgressBar();

            tick.Interval = 1;
            tick.Elapsed += (sender, e) =>
            {
                progress.Value += 1;
            };


            var cboIntervalllängen = new DropDown();
            cboIntervalllängen.Items.Add("10");
            cboIntervalllängen.Items.Add("30");
            cboIntervalllängen.Items.Add("60");
            cboIntervalllängen.SelectedIndex = 0;

            var btnStartStop = new Button { Text = "Start" };
            btnStartStop.Click += (sender, e) =>
            {
                if (timer.Started)
                {
                    tick.Stop();
                    timer.Stop();

                    btnStartStop.Text = "Start";
                }
                else
                {
                    timer.Interval = int.Parse(cboIntervalllängen.SelectedValue.ToString());
                    timer.Start();

                    progress.MaxValue = (int)timer.Interval;
                    progress.Value = 0;
                    tick.Start();

                    btnStartStop.Text = "Stop";
                }
            };

            var txtFocus = new TextBox();

            var lstFoci = new ListBox();
            lstFoci.Items.Add("a");
            lstFoci.Items.Add("b");

            var btnNotify = new Button();
            btnNotify.Text = "Notify!";
            btnNotify.Click += (sender, e) =>
            {
                var not = new NSUserNotification()
                {
                    Title = "Activity Sampling",
                    Subtitle = "What are you focused on right now?",
                    InformativeText = "Click to enter a short subject line",
                    DeliveryDate = DateTime.Now.AddSeconds(5),
                    SoundName = NSUserNotification.NSUserNotificationDefaultSoundName,
                    //HasActionButton = true
                };

                var center = NSUserNotificationCenter.DefaultUserNotificationCenter;

                center.ShouldPresentNotification += (_c, _n) => true; // always notify!
                center.RemoveAllDeliveredNotifications();

                // dont add this for ever click!!!!
                var logFilePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/activitysampling.log";
                center.DidActivateNotification += (object _sender, UNCDidActivateNotificationEventArgs _eargs) =>
                {
                    File.AppendAllText(logFilePath, $"activated @ {DateTime.Now:s}\n");
                    //this.WindowState = WindowState.Normal;
                };
                center.DidDeliverNotification += (object _sender, UNCDidDeliverNotificationEventArgs _eargs) =>
                {
                    File.AppendAllText(logFilePath, $"delivered @ {DateTime.Now:s}\n");
                };

                //center.ScheduleNotification(not);
                center.DeliverNotification(not); // sofort anzeigen
            };


            var layout = new TableLayout
            {
                Padding = new Padding(10), // padding around cells
                Spacing = new Size(5, 5), // spacing between each cell

                Rows = {
                    new TableRow(new Label{Text="Intervalllänge"}),
                    new TableRow(cboIntervalllängen),
                    new TableRow(btnStartStop),
                                        new TableRow(btnNotify),
                    new TableRow(progress),
                    new TableRow(new Label{Text="Intervallfokus"}),
                    new TableRow(txtFocus),
                    new TableRow(lstFoci)
                }
            };

            Content = layout;

            this.WindowStateChanged += (sender, e) =>
            {
                Console.WriteLine("window state changed");
            };

            //this.GotFocus += (sender, e) => WindowState = WindowState.Normal;



            var clickMe = new Command { MenuText = "Click Me!", ToolBarText = "Click Me!" };
            clickMe.Executed += (sender, e) => MessageBox.Show(this, "I was clicked!");

            var quitCommand = new Command { MenuText = "Quit", Shortcut = Application.Instance.CommonModifier | Keys.Q };
            quitCommand.Executed += (sender, e) => Application.Instance.Quit();

            var aboutCommand = new Command { MenuText = "About..." };
            aboutCommand.Executed += (sender, e) => MessageBox.Show(this, "About my app...");

            Menu = new MenuBar
            {
                Items = {
                    // File submenu
                    new ButtonMenuItem { Text = "&File", Items = { clickMe } },
                    // new ButtonMenuItem { Text = "&Edit", Items = { /* commands/items */ } },
                    // new ButtonMenuItem { Text = "&View", Items = { /* commands/items */ } },
                },
                ApplicationItems = {
                    // application (OS X) or file menu (others)
                    new ButtonMenuItem { Text = "&Preferences..." },
                },
                QuitItem = quitCommand,
                AboutItem = aboutCommand
            };
        }
    }
}
