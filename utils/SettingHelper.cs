using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieExplorer
{
    public class SettingHelper
    {
        private static Setting setting;
        private static string SETTING_FILE = System.Windows.Forms.Application.StartupPath + "\\config.json";

        public static Setting getSetting()
        {
            if(setting == null)
            {
                loadSetting();
            }
            return setting;
        }

        private static void loadSetting()
        {
            if (File.Exists(SETTING_FILE))
            {
                try
                {
                    setting = JsonConvert.DeserializeObject<Setting>(File.ReadAllText(SETTING_FILE));
                }catch(Exception) { }

            }
            else
            {
                setting = new Setting();
            }
        }

        public static void saveSetting()
        {
            if(setting != null)
            {
                File.WriteAllText(SETTING_FILE, JsonConvert.SerializeObject(setting));
            }
        }
    }
}
