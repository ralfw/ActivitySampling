using System.IO;
using System.Web.Script.Serialization;

namespace ActivitySampling.adapters.providers
{
    class Preferences
    {
        public static Preferences Instance { get; private set; }

        public static void Initialize(string applicationDataFolderPath) {
            Instance = new Preferences(applicationDataFolderPath);    
        }


        string preferencesFilePath;

        private Preferences(string applicationDataFolderPath)
        {
            this.preferencesFilePath = Path.Combine(applicationDataFolderPath, "preferences.config");
            if (!File.Exists(this.preferencesFilePath)) {
                var pref = new PreferencesDto { 
                    ActivityLogPath = applicationDataFolderPath,
                    Soundname = "Tink", // or "Submarine" or "default", "" for none
                    DefaultIntervalMin = 20
                };
                Write(pref);
            }
        }


        public void Write(PreferencesDto pref) {
            var json = new JavaScriptSerializer();
            var prefText = json.Serialize(pref);
            File.WriteAllText(this.preferencesFilePath, prefText);
        }

        public PreferencesDto Read() {
            var json = new JavaScriptSerializer();
            var prefText = File.ReadAllText(this.preferencesFilePath);
            var pref = json.Deserialize<PreferencesDto>(prefText);
            return pref;
        }
    }
}
