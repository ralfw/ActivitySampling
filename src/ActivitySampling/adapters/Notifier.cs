using System;
using Eto.Forms;
using MonoMac.Foundation;

namespace ActivitySampling
{
    class Notifier : IDisposable
    {
        public event Action<TimeSpan> Notification_scheduled;
        public event Action<TimeSpan> Countdown;
        public event Action Notification_presented;
        public event Action<string> Notification_acknowledged;


        NSUserNotification notification;
        NSUserNotificationCenter notificationCenter;

        UITimer timNotify;
        UITimer timProgress;
        TimeSpan countdown;


        public Notifier()
        {
            this.timNotify = new UITimer();
            this.timNotify.Elapsed += timNotify_elapsed;

            this.timProgress = new UITimer { Interval = 1 };
            this.timProgress.Elapsed += timProgress_elapsed;

            this.notification = new NSUserNotification {
                Title = "What are you working on?",
                Subtitle = "",
                InformativeText = "Click for same activity as before.\nOr open window to change it.",
                SoundName = NSUserNotification.NSUserNotificationDefaultSoundName
            };
            this.Current_activity = "";

            this.notificationCenter = NSUserNotificationCenter.DefaultUserNotificationCenter;
            this.notificationCenter.ShouldPresentNotification += (_c, _n) => true; // always notify!
            this.notificationCenter.DidDeliverNotification += (_s, _e) => this.Notification_presented();
            this.notificationCenter.DidActivateNotification += (_s, _e) => this.Notification_acknowledged(this.Current_activity);
        }


        public void Start(TimeSpan deliverIn) {
            this.timNotify.Stop();

            this.timNotify.Interval = deliverIn.TotalSeconds;
            this.timNotify.Start();
            this.Notification_scheduled(deliverIn);

            this.countdown = deliverIn;
            this.timProgress.Start();
        }

        public void Stop() {
            this.timNotify.Stop();
            this.timProgress.Stop();
        }


        public string Current_activity { get; set; }


        void timNotify_elapsed(object s, EventArgs e) {
            this.notification.Subtitle = this.Current_activity;
            this.notificationCenter.DeliverNotification(this.notification);
            this.notificationCenter.RemoveAllDeliveredNotifications();

            this.countdown = TimeSpan.FromSeconds(this.timNotify.Interval);

            this.Notification_scheduled(TimeSpan.FromSeconds(this.timNotify.Interval));
        }

        void timProgress_elapsed(object s, EventArgs e)
        {
            this.countdown = this.countdown.Subtract(TimeSpan.FromSeconds(1));
            this.Countdown(this.countdown);
        }


        public void Dispose()
        {
            this.timNotify.Stop();
            this.timProgress.Stop();
        }
    }
}