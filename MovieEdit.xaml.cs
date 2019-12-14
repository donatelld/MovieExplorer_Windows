using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MovieExplorer
{
    /// <summary>
    /// MovieEdit.xaml 的交互逻辑
    /// </summary>
    public partial class MovieEdit : Window
    {
        public Movie movie { get; set; }
        private List<Movie> movieList;
        private int currentIndex = 0;

        public MovieEdit()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void moviePhotoFile_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            var result = openFileDialog.ShowDialog();
            if (result == true)
            {
                movie.Photo = copyPhotos(openFileDialog.FileName);
            }
        }

        public string copyPhotos(string srcFilePath)
        {
            try
            {
                string descFilePath = "\\photos\\" + StringUtils.GenRnd20LenStr();
                System.IO.File.Copy(srcFilePath, Environment.CurrentDirectory + descFilePath, true);
                return descFilePath;
            }catch(Exception e)
            {
                Console.WriteLine(e.StackTrace);
                throw e;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DBHelper.saveMovie(movie);
            this.Close();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                movieList = MovieAPI.searchMovie(movie);
                if(movieList.Count > 1)
                {
                    chooseMoviePanel.Visibility = Visibility.Visible;
                    preBtn.IsEnabled = false;
                    pageLabel.Content = "1/" + movieList.Count;
                }
                else
                {
                    chooseMoviePanel.Visibility = Visibility.Hidden;
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            DBHelper.saveMovie(movie);
        }

        private void preBtn_Click(object sender, RoutedEventArgs e)
        {
            copyMovie(movieList[--currentIndex]);
            if (currentIndex == 0)
            {
                preBtn.IsEnabled = false;
            }
            if (!nextBtn.IsEnabled)
            {
                nextBtn.IsEnabled = true;
            }
            pageLabel.Content = (currentIndex + 1) + "/" + movieList.Count;
        }

        private void nextBtn_Click(object sender, RoutedEventArgs e)
        {
            copyMovie(movieList[++currentIndex]);
            
            if (currentIndex == movieList.Count - 1)
            {
                nextBtn.IsEnabled = false;
            }
            if (!preBtn.IsEnabled)
            {
                preBtn.IsEnabled = true;
            }
            pageLabel.Content = (currentIndex + 1) + "/" + movieList.Count;
        }

        private void copyMovie(Movie m)
        {
            movie.Photo = m.Photo;
            movie.AbsolutePhoto = m.AbsolutePhoto;
            movie.OriginalTitle = m.OriginalTitle;
            movie.Rating = m.Rating;
            movie.Year = m.Year;
            movie.DoubanID = m.DoubanID;
            DBHelper.saveMovie(movie);
        }

    }
}
