using System;
using System.IO;
using Eto.Drawing;
using Eto.Forms;
using MonoMac.Foundation;

namespace ActivitySampling
{
    class MainDlg : Form
    {
        readonly RequestHandler reqHandler;


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
            ClientSize = new Size(180, 250);

            var txtActivity = new TextBox();
            var lstActivityLog = new ListBox();
            var btnLogActivity = new Button { Text = "Log" };

            btnLogActivity.Click += (sender, e) => {
                this.reqHandler.Log_activity(txtActivity.Text);
                lstActivityLog.Items.Insert(0, new ListItem{Text = $"{DateTime.Now:t}: {txtActivity.Text}"});
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
    }
}
