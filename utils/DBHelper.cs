using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MovieExplorer
{
    class DBHelper
    {
        public static string TABLE_MOVIE = "movie";

        public static bool saveMovie(Movie movie)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data["title"] = movie.Title;
            data["original_title"] = movie.OriginalTitle;
            data["file_name"] = movie.FileName;
            data["file_path"] = movie.FilePath;
            data["resolution"] = movie.Resolution;
            data["rating"] = movie.Rating;
            data["year"] = movie.Year;
            data["douban_id"] = movie.DoubanID;
            data["photo"] = movie.Photo;
            data["format"] = movie.Format;
            data["create_time"] = movie.CreateTime;
            data["last_modified"] = movie.LastModified;
            bool result = false;
            try
            {
                if(movie.ID == 0)
                {
                    result = InsertRow(data, TABLE_MOVIE);
                }
                else
                {
                    Dictionary<string, string> condition = new Dictionary<string, string>();
                    condition["ID"] = movie.ID.ToString();
                    result = UpdateRow(data, TABLE_MOVIE, condition);
                }
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                result = false;
            }

            return result;
        }

        public static List<Movie> getMovies()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("select * from movie order by last_modified desc");

            try
            {
                List<Object> list = ExecuteSQL(builder.ToString());
                List<Movie> movieList = new List<Movie>();
                foreach (Dictionary<string, string> dict in list)
                {
                    Movie movie = new Movie();
                    movie.ID = Int32.Parse(dict["id"]);
                    movie.Title= dict["title"];
                    movie.OriginalTitle = dict["original_title"];
                    movie.FileName = dict["file_name"];
                    movie.FilePath = dict["file_path"];
                    movie.Resolution = dict["resolution"];
                    movie.Rating = dict["rating"];
                    movie.Year = dict["year"];
                    movie.DoubanID = dict["douban_id"];
                    movie.Photo = dict["photo"];
                    movie.Format = dict["format"];
                    movie.CreateTime = dict["create_time"];
                    movie.LastModified = dict["last_modified"];
                    movieList.Add(movie);
                }
                return movieList;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                throw e;
            }
        }

        // 插入一行新数据
        protected static bool InsertRow(Dictionary<string, string> data, string tableName)
        {
            int nDataLen = data.Count;
            if (nDataLen <= 0) return false;

            string sql = "INSERT INTO [" + tableName + "]  (";
            string keys = "";
            string vals = "";
            foreach (System.Collections.Generic.KeyValuePair<string, string> kvp in data)
            {
                string key = kvp.Key;
                string val = StringUtils.filterSymbol(kvp.Value); 
                
                if (keys.Length > 0) keys += ",";
                keys += key;

                if (vals.Length > 0) vals += ",";
                vals += "'" + val + "'";

            }
            sql += keys + ") VALUES (" + vals + ")";
            try
            {
                return ExecuteSQLNonQuery(sql);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                throw e;
            }
        }

        // 更新一行新数据  
        protected static bool UpdateRow(Dictionary<string, string> data, string tableName, Dictionary<string, string> condition)
        {
            int nDataLen = data.Count;
            if (nDataLen <= 0) return false;

            string sql = "UPDATE [" + tableName + "]  SET ";
            string vals = "";

            foreach (System.Collections.Generic.KeyValuePair<string, string> kvp in data)
            {
                string key = kvp.Key;
                string val = StringUtils.filterSymbol(kvp.Value); 
                if (val == null)//对非空值进行更新, 空值不更新.
                    continue;
                if (vals.Length > 0) vals += ",";
                vals += key + "='" + val + "' ";
            }
            sql += vals;

            StringBuilder conditionBuilder = new StringBuilder();
            foreach (System.Collections.Generic.KeyValuePair<string, string> kvp in condition)
            {
                string key = kvp.Key;
                string val = StringUtils.filterSymbol(kvp.Value);
                if (StringUtils.isBlank(val) || StringUtils.isBlank(key))
                    continue;
                if (conditionBuilder.Length > 1)
                    conditionBuilder.Append(" AND ");
                conditionBuilder.Append(key).Append("='").Append(val).Append("' ");
            }
            if (conditionBuilder.Length > 1)
                sql += " WHERE " + conditionBuilder.ToString();
            try
            {
                return ExecuteSQLNonQuery(sql);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                throw e;
            }
        }

        public static void deleteMovie(int ID)
        {
            string sql = "DELETE FROM " + TABLE_MOVIE + " WHERE ID = " + ID;
            try
            {
                ExecuteSQLNonQuery(sql);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                throw e;
            }
        }

        // 适用于类似del update等命令， 无需返回Query行
        protected static bool ExecuteSQLNonQuery(string sql)
        {
            if (!File.Exists(Environment.CurrentDirectory + "/db/movie.db"))//检查数据库文件是否存在
            {
                MessageBox.Show("本地数据库文件丢失, 请重新安装客户端。");
                return false;
            }

            try
            {   //注意， 真正的运行时数据库是在 Debug/db/movie.db  而不是工程目录下的 /db/movie.db
                using (SQLiteConnection conn = new SQLiteConnection("Data Source=" + Environment.CurrentDirectory + "/db/movie.db"))
                {
                    conn.Open();
                    SQLiteCommand cmd = conn.CreateCommand();
                    cmd.CommandText = sql; //"SELECT * FROM CouponScanRcd";
                    int ret = cmd.ExecuteNonQuery();
                    conn.Close();
                    return (ret >= 1);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                throw e;
            }
        }

        // 执行需要返回值的SQL 语句
        // 抛出异常时返回null
        protected static List<Object> ExecuteSQL(string sql)
        {
            if (!File.Exists(Environment.CurrentDirectory + "\\db\\movie.db"))//检查数据库文件是否存在
            {
                MessageBox.Show("本地数据库文件丢失, 请重新安装客户端。");
                return null;
            }
            List<Object> vRet = new System.Collections.Generic.List<Object>();
            try
            {
                string path = Environment.CurrentDirectory + "/db/movie.db";
                SQLiteConnection conn = new SQLiteConnection("Data Source=" + Environment.CurrentDirectory + "/db/movie.db");
                conn.Open();

                SQLiteCommand cmd = conn.CreateCommand();
                cmd.CommandText = sql; //"SELECT * FROM CouponScanRcd";
                using (SQLiteDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    while (dr.Read())
                    {
                        Dictionary<string, string> map = new Dictionary<string, string>();
                        for (int k = 0; k < dr.FieldCount; k++)
                        {
                            string name = dr.GetName(k);
                            string val = null;
                            try
                            {
                                Type type = dr.GetFieldType(k);
                                if (dr.IsDBNull(k))
                                    val = "";
                                else if (StringUtils.equals("String", type.Name))
                                    val = dr.GetString(k);
                                else if (StringUtils.equals("Double", type.Name))
                                    val = dr.GetDouble(k) + "";
                                else if (StringUtils.equals("Int64", type.Name))
                                    val = dr.GetInt64(k) + "";
                                else
                                    val = dr.GetString(k);

                            }
                            catch (System.Exception ex)
                            {
                                Console.WriteLine(ex.StackTrace);
                                throw ex;
                            }
                            if (val == null) val = "";
                            if (name == null) continue;
                            map[name] = val;
                        }
                        vRet.Add(map);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                throw e;
            }
            return vRet;
        }
    }
}
