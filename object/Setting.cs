using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieExplorer
{
    public class Setting : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private List<string> movieFolders;
        public List<string> MovieFolders
        {
            get
            {
                if(movieFolders == null)
                {
                    movieFolders = new List<string>();
                }
                return movieFolders;
            }
            set
            {
                movieFolders = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("MovieFolders"));
                }
            }
        }


        private string player;
        public string Player
        {
            get
            {
                return player;
            }
            set
            {
                player = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Player"));
                }
            }
        }

        private string remotePort;
        public string RemotePort
        {
            get
            {
                return remotePort;
            }
            set
            {
                remotePort = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("RemotePort"));
                }
            }
        }

        private bool autoStart;
        public bool AutoStart
        {
            get
            {
                return autoStart;
            }
            set
            {
                autoStart = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("AutoStart"));
                }
            }
        }
    }
}
