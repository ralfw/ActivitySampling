using System;
using System.Collections.Generic;
using System.Linq;
using ActivitySampling.adapters.providers;

namespace ActivitySampling
{
    class RequestHandler
    {
        readonly ActivityLog activityLog;


        public RequestHandler(string applicationDataFolderPath) {
            this.activityLog = new ActivityLog(applicationDataFolderPath);
        }


        public void Log_activity(string description)
        {
            var activity = new ActivityDto { Description = description, Timestamp = DateTime.Now };
            this.activityLog.Append(activity);
        }


        public ActivityDto[] Select_all_activities() {
            return this.activityLog.Activities.ToArray();
        }

        public ActivityDto[] Select_recent_activities(int n) {
            var toSkip = this.activityLog.Activities.Length - n;
            return this.activityLog.Activities.Skip(toSkip).Take(n).ToArray();
        }
    }
}
