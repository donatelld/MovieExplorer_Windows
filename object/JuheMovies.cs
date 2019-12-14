using System.Collections.Generic;

namespace MovieExplorer
{
    public class JuheMovies
    {
        public string resultcode;
        public string reason;
        public List<Subject> result;

        public class Subject
        {
            public string movieid;
            public string title;
            public string year;
            public string poster;
            public string rating;
        }
    }
}