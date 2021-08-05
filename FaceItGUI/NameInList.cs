using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace FaceItGUI
{
    public class NameInList : INotifyPropertyChanged
    {
        private string _feeling;
       // private string _recognized;

        public string Name { get; set; }
        public string Feeling
        {
            get
            {
                return _feeling;
            }
            set
            {
                _feeling = value;
                NotifyPropertyChanged("Feeling");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }
}
