using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MovieExplorer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Movie> movieList;
        private List<string> supportMovieFormats = new List<string> {"ISO", "MKV", "MP4", "TS"};
        private Setting setting;
        private int selectedIndex = -1;
        private int ROW_SIZE = 8;
        private MovieServer movieServer;
        private bool minimizeStart;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private IntPtr m_Hwnd;
        private int displaySreen;

        public MainWindow(bool minimize)
        {
            this.minimizeStart = minimize;
            InitializeComponent();
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //默认获取第一个屏幕显示
            displaySreen = System.Windows.Forms.Screen.AllScreens.Length - 1;
            System.Windows.Forms.Screen s = System.Windows.Forms.Screen.AllScreens[displaySreen];
            System.Drawing.Rectangle r = s.WorkingArea;
            this.Top = r.Top;
            this.Left = r.Left;
            this.WindowState = WindowState.Maximized;
            //创建托盘
            CreateWindowNotify();
            if (minimizeStart)
            {
                Hide();
            }
            setting = SettingHelper.getSetting();
            //启动远程服务
            movieServer = new MovieServer();
            movieServer.Start(setting.RemotePort);
            if(setting.MovieFolders == null || setting.MovieFolders.Count == 0)
            {
                MessageBox.Show("请先设置电影目录");
                openSettingForm();
                return;
            }
            //加载电影
            loadMovies();
        }


        private void CreateWindowNotify()
        {
            this.notifyIcon = new System.Windows.Forms.NotifyIcon();
            this.notifyIcon.BalloonTipText = "Movie Explorer";
            this.notifyIcon.ShowBalloonTip(2000);
            this.notifyIcon.Text = "Movie Explorer";
            this.notifyIcon.Icon = new System.Drawing.Icon(@"images/logo.ico");
            this.notifyIcon.Visible = true;
            //打开菜单项
            System.Windows.Forms.MenuItem open = new System.Windows.Forms.MenuItem("Open");
            open.Click += new EventHandler(Show);
            //退出菜单项
            System.Windows.Forms.MenuItem exit = new System.Windows.Forms.MenuItem("Exit");
            exit.Click += new EventHandler(Close);
            //关联托盘控件
            System.Windows.Forms.MenuItem[] childen = new System.Windows.Forms.MenuItem[] { open, exit };
            notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu(childen);

            this.notifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler((o, e) =>
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Left) this.Show(o, e);
            });
        }


        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            // 获取窗体句柄
            m_Hwnd = new WindowInteropHelper(this).Handle;
            HwndSource hWndSource = HwndSource.FromHwnd(m_Hwnd);
            // 添加处理程序
            if (hWndSource != null) hWndSource.AddHook(WndProc);
        }

        /// <summary>
        /// 所有控件初始化完成后调用
        /// </summary>
        /// <param name="e"></param>
        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            // 注册热键
            HotKey.RegisterHotKey(m_Hwnd, 100, HotKey.KeyModifiers.Ctrl, System.Windows.Forms.Keys.D0);
        }

        //快捷键事件处理
        private IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr longParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            //按快捷键 
            switch (msg)
            {
                case WM_HOTKEY:
                    switch (wParam.ToInt32())
                    {
                        case 100:
                            if (this.Visibility == Visibility.Visible)
                            {
                                Hide();
                            }
                            else
                            {
                                Show();
                            }
                            break;
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        private void Show(object sender, EventArgs e)
        {
            this.Visibility = System.Windows.Visibility.Visible;
            this.ShowInTaskbar = true;
            this.Activate();
            this.Focus();
        }

        private void Hide(object sender, EventArgs e)
        {
            this.ShowInTaskbar = false;
            this.Visibility = System.Windows.Visibility.Hidden;
        }

        private void Close(object sender, EventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void loadMovies()
        {
            try
            {
                //查询当前数据库中的电影
                movieList = DBHelper.getMovies();
                Dictionary<string, Movie> dbMovieDict = new Dictionary<string, Movie>();
                foreach (Movie movie in movieList)
                {
                    dbMovieDict.Add(movie.FileName, movie);
                }

                //遍历文件夹中的电影
                Dictionary<string, Movie> fileList = new Dictionary<string, Movie>();
                foreach (string movieFolder in setting.MovieFolders)
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(movieFolder);
                    foreach (DirectoryInfo movieDir in directoryInfo.GetDirectories())
                    {
                        //跳过系统和隐藏文件夹
                        if ((movieDir.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden 
                            || (movieDir.Attributes & FileAttributes.System) == FileAttributes.System)
                        {
                            continue;
                        }
                            bool isBdmv = false;
                        foreach (DirectoryInfo childDir in movieDir.GetDirectories())
                        {
                            if (StringUtils.equals(childDir.Name.ToUpper(), "BDMV"))
                            {
                                isBdmv = true;
                                break;
                            }
                        }
                        if (isBdmv)
                        {
                            string title = "";
                            int index = movieDir.Name.IndexOf(" ");
                            if (index < 0)
                            {
                                title = movieDir.Name;
                            }
                            else
                            {
                                title = movieDir.Name.Substring(0, index);
                            }
                            Movie m = new Movie();
                            m.Title = title;
                            m.FileName = movieDir.Name;
                            m.FilePath = movieDir.FullName;
                            m.Format = "BDMV";
                            m.Resolution = getMovieResolution(movieDir.Name);
                            m.CreateTime = movieDir.CreationTime.ToString("yyyy-MM-dd HH:mm:ss");
                            m.LastModified = movieDir.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss");
                            fileList.Add(m.FileName, m);
                        }
                        else
                        {
                            getFileMovies(fileList, movieDir);
                        }
                    }
                    getFileMovies(fileList, directoryInfo);
                }
                //文件夹中新增的电影
                foreach (KeyValuePair<string, Movie> dict in fileList)
                {
                    if (!dbMovieDict.ContainsKey(dict.Key))
                    {
                        DBHelper.saveMovie(dict.Value);
                        continue;
                    }
                    Movie dbMovie = dbMovieDict[dict.Key];
                    Movie fileMovie = dict.Value;
                    if(!StringUtils.equals(dbMovie.FilePath, fileMovie.FilePath))//电影目录发生变化时更新目录
                    {
                        dbMovie.FilePath = fileMovie.FilePath;
                        DBHelper.saveMovie(dbMovie);
                    }
                    if (!File.Exists(dbMovie.AbsolutePhoto))//本地图片不存在时删除数据库图片
                    {
                        dbMovie.Photo = "";
                        DBHelper.saveMovie(dbMovie);
                    }
                }
                //文件夹中删除的电影
                foreach (KeyValuePair<string, Movie> dict in dbMovieDict)
                {
                    if (!fileList.ContainsKey(dict.Key))
                    {
                        DBHelper.deleteMovie(dict.Value.ID);
                    }
                }
                movieList = DBHelper.getMovies();
                int i = 0;
                foreach (Movie movie in movieList)
                {
                    MovieInfo movieInfo = new MovieInfo();
                    movieInfo.Index = i++;
                    movieInfo.DataContext = movie;
                    movieInfo.MouseEnter += new MouseEventHandler(this.Image_MouseEnter);
                    movieInfo.MouseLeave += new MouseEventHandler(this.moviePhoto_MouseLeave);
                    movieInfo.MouseLeftButtonDown += new MouseButtonEventHandler(this.moviePhoto_MouseButtonDown);
                    movieInfo.MouseRightButtonDown += new MouseButtonEventHandler(this.moviePhoto_MouseButtonDown);
                    moviePanel.Children.Add(movieInfo);
                }

                Thread t1 = new Thread(new ThreadStart(threadLoadMovieImage));
                t1.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        private void getFileMovies(Dictionary<string, Movie> fileList, DirectoryInfo directory)
        {
            foreach (FileInfo movieFile in directory.GetFiles())
            {
                if (movieFile.Length < 1024 * 1024 * 1014) continue;//小于1G的文件跳过
                string format = movieFile.Name.Substring(movieFile.Name.LastIndexOf(".") + 1).ToUpper();
                if (!supportMovieFormats.Contains(format))
                {
                    continue;
                }
                MovieInfo movieInfo = new MovieInfo();
                string title = "";
                int index = directory.Name.IndexOf(" ");
                if (index < 0)
                {
                    index = directory.Name.IndexOf(".");
                }
                if (index < 0)
                {
                    title = directory.Name;
                }
                else
                {
                    title = directory.Name.Substring(0, index);
                }
                Movie m = new Movie();
                m.Title = title;
                m.FileName = movieFile.Name;
                m.FilePath = movieFile.FullName;
                m.Format = format;
                m.Resolution = getMovieResolution(movieFile.Name);
                m.CreateTime = movieFile.CreationTime.ToString("yyyy-MM-dd HH:mm:ss");
                m.LastModified = movieFile.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss");
                try
                {
                    fileList.Add(m.FileName, m);
                }catch(Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public string getMovieResolution(string fileName)
        {
            if(fileName.IndexOf("4K") >0 || fileName.IndexOf("2160") > 0)
            {
                return "4K";
            }else if (fileName.IndexOf("2K") > 0 || fileName.IndexOf("1080") > 0)
            {
                return "2K";
            }
            else if (fileName.IndexOf("720") > 0)
            {
                return "720P";
            }
            else
            {
                return "2K";
            }

        }

        private void threadLoadMovieImage()
        {
            foreach (Movie movie in movieList)
            {
                if (StringUtils.isBlank(movie.Photo))
                {
                    MovieAPI.searchMovie(movie);
                }
            }
        }

        private void Image_MouseEnter(object sender, MouseEventArgs e)
        {
            MovieInfo movieInfo = sender as MovieInfo;
            if (!movieInfo.Index.Equals(this.selectedIndex))
            {
                movieInfo.moviePhotoBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(144, 214, 234));
            }
        }

        private void moviePhoto_MouseLeave(object sender, MouseEventArgs e)
        {
            MovieInfo movieInfo = sender as MovieInfo;
            if (!movieInfo.Index.Equals(this.selectedIndex))
            {
                movieInfo.moviePhotoBorder.BorderBrush = null;
            }         
        }

        private void moviePhoto_MouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            MovieInfo movieInfo = sender as MovieInfo;
            if (e.ChangedButton == MouseButton.Left)
            {
                selectedMovie(movieInfo);
                if (e.ClickCount == 2)
                {
                    if (StringUtils.isBlank(setting.Player))
                    {
                        MessageBox.Show("你还没有设置播放器！");
                        return;
                    }
                    playMovie();
                }
            } else if (e.ChangedButton == MouseButton.Right)
            {
                MovieEdit movieEdit = new MovieEdit();
                movieEdit.movie = movieList[movieInfo.Index];
                movieEdit.DataContext = movieEdit.movie;
                movieEdit.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                movieEdit.ShowDialog();
            }
        }

        private void selectedMovie(MovieInfo movieInfo)
        {
            if (!movieInfo.Index.Equals(this.selectedIndex))
            {
                if (this.selectedIndex != -1)
                {
                    MovieInfo previous = getMovieInfo(this.selectedIndex);
                    previous.moviePhotoBorder.BorderBrush = null;
                }
                movieInfo.moviePhotoBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(64, 140, 254));
                this.selectedIndex = movieInfo.Index;
            }
        }


        private void Setting_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Image image = sender as Image;
            switch (image.Name)
            {
                case "close":
                    Close();
                    break;
                case "minimize":
                    Hide();
                    break;
                case "settingBtn":
                    openSettingForm();
                    break;
            }
        }

        private void Setting_MouseEnter(object sender, MouseEventArgs e)
        {
            Image image = sender as Image;
            switch (image.Name)
            {
                case "close":
                    image.Source = new BitmapImage(new Uri("images/close_havor.png", UriKind.Relative));
                    break;
                case "minimize":
                    image.Source = new BitmapImage(new Uri("images/minimize_havor.png", UriKind.Relative));
                    break;
                case "settingBtn":
                    image.Source = new BitmapImage(new Uri("images/setting_havor.png", UriKind.Relative));
                    break;
            }
            
        }

        private void Setting_MouseLeave(object sender, MouseEventArgs e)
        {
            Image image = sender as Image;
            switch (image.Name)
            {
                case "close":
                    image.Source = new BitmapImage(new Uri("images/close.png", UriKind.Relative));
                    break;
                case "minimize":
                    image.Source = new BitmapImage(new Uri("images/minimize.png", UriKind.Relative));
                    break;
                case "settingBtn":
                    image.Source = new BitmapImage(new Uri("images/setting.png", UriKind.Relative));
                    break;
            }
        }

        private void openSettingForm()
        {
            SettingForm settingForm = new SettingForm();
            settingForm.DataContext = setting;
            settingForm.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            settingForm.Closed += this.SettingFormClosed;
            settingForm.ShowDialog();
            if (settingForm.FolderChanged)
            {
                moviePanel.Children.Clear();
                loadMovies();
            }
        }

        private void SettingFormClosed(object sender, System.EventArgs e)
        {
            if (!StringUtils.equals(setting.RemotePort, movieServer.ServerPort))
            {
                movieServer.Stop();
                movieServer.Start(setting.RemotePort);
            }
        }

            private void playMovie()
        {
            Movie movie = movieList[selectedIndex];
            Process cmd = new Process();
            cmd.StartInfo.FileName = setting.Player;
            cmd.StartInfo.Arguments = @movie.FilePath;
            cmd.Start();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            int nextIndex = -1;
            switch (e.Key)
            {
                case Key.Up:
                    if (selectedIndex == -1)
                    {
                        nextIndex = 0;
                    }
                    else
                    {
                        if(selectedIndex - ROW_SIZE >= 0)
                        {
                            nextIndex = selectedIndex - 8;
                        }
                    }
                    break;
                case Key.Down:
                    if (selectedIndex == -1)
                    {
                        nextIndex = 0;
                    }
                    else
                    {
                        if (selectedIndex + ROW_SIZE < movieList.Count)
                        {
                            nextIndex = selectedIndex + 8;
                        }
                    }
                    break;
                case Key.Left:
                    if (selectedIndex == -1)
                    {
                        nextIndex = 0;
                    }
                    else
                    {
                        if (selectedIndex - 1 >= 0)
                        {
                            nextIndex = selectedIndex - 1;
                        }
                    }
                    break;
                case Key.Right:
                    if (selectedIndex == -1)
                    {
                        nextIndex = 0;
                    }
                    else
                    {
                        if (selectedIndex + 1 < movieList.Count)
                        {
                            nextIndex = selectedIndex + 1;
                        }
                    }
                    break;
                case Key.Enter:
                    playMovie();
                    break;
                case Key.PageDown:
                    //切换屏幕
                    if (displaySreen == 0) displaySreen = 1;
                    else displaySreen = 0;
                    System.Windows.Forms.Screen s = System.Windows.Forms.Screen.AllScreens[displaySreen];
                    System.Drawing.Rectangle r = s.WorkingArea;
                    this.WindowState = WindowState.Normal;
                    this.Top = r.Top;
                    this.Left = r.Left;
                    this.WindowState = WindowState.Maximized;
                    break;
            }
            if(nextIndex != -1)
            {
                int beforeRows = selectedIndex / 8;
                int afterRows = nextIndex / 8;
                if (beforeRows < afterRows)
                {
                    if(afterRows > 2)
                    {
                        scrollBar.ScrollToVerticalOffset(scrollBar.VerticalOffset + 350);
                    }
                }
                else if(beforeRows > afterRows)
                {
                    scrollBar.ScrollToVerticalOffset(scrollBar.VerticalOffset - 350);
                }
                
                selectedMovie(getMovieInfo(nextIndex));
            }
        }

        private MovieInfo getMovieInfo(int index)
        {
            int i = 0;
            foreach (UIElement element in moviePanel.Children)
            {
                if(i == index)
                {
                    return element as MovieInfo;
                }
                i++;
            }
            return null;
        }
    }
}
