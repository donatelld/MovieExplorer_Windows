using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MovieExplorer
{
    /// <summary>
    /// SettingForm.xaml 的交互逻辑
    /// </summary>
    public partial class SettingForm : Window
    {
        private Setting setting;
        public bool FolderChanged;

        public SettingForm()
        {
            InitializeComponent();
            setting = SettingHelper.getSetting();
            initMovieFolderPanel();
        }

        private void initMovieFolderPanel()
        {
            if(setting.MovieFolders != null)
            {
                foreach(string movieFolder in setting.MovieFolders)
                {
                    WrapPanel panel = createMovieFolderPanel(movieFolder);
                    MovieFolderPanel.Children.Add(panel);
                }
            }
        }

        private WrapPanel createMovieFolderPanel(string folderName)
        {
            WrapPanel wrapPanel = new WrapPanel();
            Thickness margin = new Thickness();
            margin.Top = 3;
            wrapPanel.Margin = margin;
            TextBlock folderText = new TextBlock();
            folderText.Text = folderName;
            folderText.FontSize = 12;
            Color color = (Color)ColorConverter.ConvertFromString("#13227a");
            folderText.Foreground = new SolidColorBrush(color);
            wrapPanel.Children.Add(folderText);

            Image image = new Image();
            image.Source = new BitmapImage(new Uri("images/delete.png", UriKind.Relative));
            image.Width = 12;
            Thickness margin1 = new Thickness();
            margin1.Left = 5;
            image.Margin = margin1;
            image.MouseLeftButtonDown += new MouseButtonEventHandler(this.deleteFolder_MouseButtonDown);
            wrapPanel.Children.Add(image);
            return wrapPanel;
        }

        private void deleteFolder_MouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            FolderChanged = true;
            Image image = sender as Image;
            WrapPanel wrapPanel = (WrapPanel)image.Parent;
            TextBlock textBlock = null;
            foreach (UIElement element in wrapPanel.Children)
            {
                if(element.GetType() == typeof(TextBlock)){
                    textBlock = element as TextBlock;
                    break;
                }
            }
            setting.MovieFolders.Remove(textBlock.Text);
            MovieFolderPanel.Children.Remove(wrapPanel);
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "(*.exe) | *.exe";
            DialogResult result = openFileDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }
            PlayerTextBox.Text = openFileDialog.FileName;
            setting.Player = openFileDialog.FileName;
        }

        private void Image_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            FolderBrowserDialog openFolderDialog = new FolderBrowserDialog();
            DialogResult result = openFolderDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }
            FolderChanged = true;
            setting.MovieFolders.Add(openFolderDialog.SelectedPath);
            WrapPanel panel = createMovieFolderPanel(openFolderDialog.SelectedPath);
            MovieFolderPanel.Children.Add(panel);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            SettingHelper.saveSetting();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (this.AutoStartBox.IsChecked == true)
            {
                CreateShortcutToStartUp();
            }
            else
            {
                string StartupPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonStartup);
                if (File.Exists(StartupPath + "\\MovieExplorer.lnk"))
                {
                    File.Delete(StartupPath + "\\MovieExplorer.lnk");
                }
            }
        }

        /// <summary>  
        /// 创建快捷方式。  
        /// </summary>  
        /// <param name="shortcutPath">快捷方式路径。</param>  
        /// <param name="targetPath">目标路径。</param>  
        /// <param name="workingDirectory">工作路径。</param>  
        /// <param name="description">快捷键描述。</param>  
        public static bool CreateShortcutToStartUp()
        {
            try
            {
                string StartupPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonStartup);
                if (File.Exists(StartupPath + "\\MovieExplorer.lnk")) return true;
                string CurrentPath = Environment.CurrentDirectory;
                IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();

                IWshRuntimeLibrary.IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(StartupPath + "\\MovieExplorer.lnk");
                shortcut.TargetPath = CurrentPath + "\\MovieExplorer.exe";
                shortcut.Arguments = "-s";// 参数
                shortcut.Description = "MovieExplorer";
                shortcut.WorkingDirectory = CurrentPath;//程序所在文件夹，在快捷方式图标点击右键可以看到此属性
                shortcut.IconLocation = @"" + shortcut.TargetPath + ",0";//图标
                //shortcut.Hotkey = "CTRL+SHIFT+Z";//热键
                shortcut.WindowStyle = 1;
                shortcut.Save();
                return true;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }
    }
}
