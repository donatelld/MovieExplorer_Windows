using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieExplorer
{
    public class DoubanMovies
    {
        public int total;
        public List<Subject> subjects;



        public class Subject
        {
            public string id;
            public string title;
            public string original_title;
            public Rating rating;
            public string year;
            public Image images;
        }

        public class Image
        {
            public string small;
            public string large;
            public string medium;
        }

        public class Rating
        {
            public string average;
        }
    }

}
