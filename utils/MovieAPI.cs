using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MovieExplorer
{
    public class MovieAPI
    {
        public static List<Movie> searchMovie(Movie m)
        {
            List<Movie> movieList = new List<Movie>();
            try {
                //DoubanMovies doubanMovies = searchDoubanMovie(movie.Title);
                //if(doubanMovies.total > 0)
                //{
                //    DoubanMovies.Subject subject = doubanMovies.subjects[0];
                //    string imageUrl = subject.images.small;
                //    movie.Photo = downloadPhoto(imageUrl);
                //    movie.OriginalTitle = subject.original_title;
                //    movie.Rating = subject.rating.average;
                //    movie.Year = subject.year;
                //    movie.DoubanID = subject.id;
                //    DBHelper.saveMovie(movie);
                //}
                JuheMovies juheMovies = searchJuheMovie(m.Title);
                if (StringUtils.equals(juheMovies.resultcode, "200") && juheMovies.result.Count > 0)
                {
                    for(int i=0; i<juheMovies.result.Count; i++)
                    {
                        JuheMovies.Subject subject = juheMovies.result[i];
                        Movie movie = m.Clone();
                        movie.Photo = downloadPhoto(subject.poster);
                        movie.OriginalTitle = subject.title;
                        movie.Rating = subject.rating;
                        movie.Year = subject.year;
                        movie.DoubanID = subject.movieid;
                        if(i == 0)//默认保存第一条
                        {
                            m.Photo = movie.Photo;
                            m.AbsolutePhoto = movie.AbsolutePhoto;
                            m.OriginalTitle = movie.OriginalTitle;
                            m.Rating = movie.Rating;
                            m.Year = movie.Year;
                            m.DoubanID = movie.DoubanID;
                            DBHelper.saveMovie(movie);
                        }
                        movieList.Add(movie);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
            return movieList;
        }

        private static DoubanMovies searchDoubanMovie(string title)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.douban.com/v2/movie/search?q=" + title);
                request.Method = "GET";
                request.ContentType = "text/html;charset=UTF-8";

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream myResponseStream = response.GetResponseStream();
                StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
                string retString = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                myResponseStream.Close();

                Console.WriteLine(retString);
                DoubanMovies doubanMovies = JsonConvert.DeserializeObject<DoubanMovies>(retString);
                return doubanMovies;
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }

        private static JuheMovies searchJuheMovie(string title)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://v.juhe.cn/movie/index?key=434cb74781f91803cc083b6cc40cab3b&smode=0&title=" + title);
                request.Method = "GET";
                request.ContentType = "text/html;charset=UTF-8";

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream myResponseStream = response.GetResponseStream();
                StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
                string retString = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                myResponseStream.Close();

                Console.WriteLine(retString);
                JuheMovies juheMovies = JsonConvert.DeserializeObject<JuheMovies>(retString);
                return juheMovies;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }

        private static string downloadPhoto(string url)
        {
            try
            {
                WebRequest request = WebRequest.Create(url);
                WebResponse response = request.GetResponse();
                Stream reader = response.GetResponseStream();
                string suffix = url.Substring(url.LastIndexOf("."));
                string photoFolder = Environment.CurrentDirectory + "/photos/";
                if (!Directory.Exists(photoFolder))
                {
                    Directory.CreateDirectory(photoFolder);
                }
                string fileName = "/photos/" + StringUtils.GenRnd20LenStr() + suffix;
                FileStream writer = new FileStream(Environment.CurrentDirectory + fileName, FileMode.OpenOrCreate, FileAccess.Write);
                byte[] buff = new byte[512];
                int c = 0; //实际读取的字节数
                while ((c = reader.Read(buff, 0, buff.Length)) > 0)
                {
                    writer.Write(buff, 0, c);
                }
                writer.Close();
                writer.Dispose();
                reader.Close();
                reader.Dispose();
                response.Close();
                return fileName;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return "";
            }
        }
    }
}
