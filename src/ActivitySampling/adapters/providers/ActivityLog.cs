﻿using System;
using System.IO;
using System.Linq;

namespace ActivitySampling.adapters.providers
{
    class ActivityLog
    {
        const string LOG_FILENAME = "activities.log";

        readonly string filepath;

        public ActivityLog(string folderPath) {
            this.filepath = Path.Combine(folderPath, LOG_FILENAME);
        }

        public void Append(ActivityDto activity) {
            var entry = $"{activity.Timestamp:s}\t{activity.Description}";
            File.AppendAllLines(this.filepath, new[] { entry });
        }

        public ActivityDto[] Activities {
            get {
                var entries = File.ReadAllLines(this.filepath);
                return entries.Where(e => !string.IsNullOrWhiteSpace(e)).Select(Parse_entry).ToArray();

                ActivityDto Parse_entry(string entry) {
                    var parts = entry.Split('\t');
                    return new ActivityDto {
                        Timestamp = DateTime.Parse(parts[0]),
                        Description = parts[1]
                    };
                }
            }
        }
    }
}
