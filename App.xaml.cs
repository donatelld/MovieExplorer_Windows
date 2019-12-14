using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MovieExplorer
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {

        private void ApplicationStartup(object sender, StartupEventArgs e)
        {
            string s = string.Empty;
            for (int i = 0; i < e.Args.Length; i++)
            {
                s += e.Args[i] + "";
            }
            if(StringUtils.equals(s, "-s"))
            {
                new MainWindow(true).Show();
            }
            else
            {
                new MainWindow().Show();
            }
        }
    }
}
