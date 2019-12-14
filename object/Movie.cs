using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieExplorer
{
    public class Movie : INotifyPropertyChanged, ICloneable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public int ID { get; set; }
        private string title;
        public string Title {
            get
            {
                return title;
            }
            set
            {
                title = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Title"));
                }
            }
        }
        private string originalTitle;
        public string OriginalTitle
        {
            get
            {
                return originalTitle;
            }
            set
            {
                originalTitle = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("OriginalTitle"));
                }
            }
        }
        private string rating;
        public string Rating
        {
            get
            {
                return rating;
            }
            set
            {
                rating = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Rating"));
                }
            }
        }
        private string resolution;
        public string Resolution
        {
            get
            {
                return resolution;
            }
            set
            {
                resolution = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Resolution"));
                }
            }
        }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string Format { get; set; }
        public string Year { get; set; }
        public string DoubanID { get; set; }
        private string photo;
        public string Photo
        {
            get
            {
                return photo;
            }
            set
            {
                photo = value;
                if (StringUtils.isBlank(photo))
                {
                    absolutePhoto = Environment.CurrentDirectory + "/images/movie-default.gif";
                }
                else
                {
                    if (!photo.StartsWith("http")) {
                        absolutePhoto = Environment.CurrentDirectory + photo;
                    }
                    else
                    {
                        absolutePhoto = photo;
                    }
                }
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Photo"));
                    PropertyChanged(this, new PropertyChangedEventArgs("AbsolutePhoto"));
                }
            }
        }
        private string absolutePhoto;
        public string AbsolutePhoto
        {
            get
            {
                return absolutePhoto;
            }
            set
            {
                absolutePhoto = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("AbsolutePhoto"));
                }
            }
        }
        public string CreateTime { get; set; }
        public string LastModified { get; set; }

        object ICloneable.Clone()
        {
            return this.Clone();
        }

        public Movie Clone()
        {
            return (Movie)this.MemberwiseClone();
        }
    }

}
