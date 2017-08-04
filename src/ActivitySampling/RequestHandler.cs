using System;
using System.Collections.Generic;
using System.Linq;

namespace ActivitySampling
{
    class RequestHandler
    {
        readonly ActivityLog activityLog;


        public RequestHandler(string applicationDataFolderPath) {
            this.activityLog = new ActivityLog(applicationDataFolderPath);
            Logging.Log.Append("Request handler started");
        }


        public void Log_activity(string description)
        {
            var activity = new ActivityDto { Description = description, Timestamp = DateTime.Now };
            this.activityLog.Append(activity);
        }


        public IEnumerable<ActivityDto> Select_activities()
        {
            return this.activityLog.Activities.ToArray();
        }
    }
}
