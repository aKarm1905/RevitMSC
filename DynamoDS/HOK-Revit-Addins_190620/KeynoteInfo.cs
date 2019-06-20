using HOK.Keynote.REST;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.Keynote.ClassModels
{
    public class KeynoteInfo : INotifyPropertyChanged
    {
        private string idValue = "";
        private string keyValue = "";
        private string parentKeyValue = "";
        private string descriptionValue = "";
        private string keynoteSetIdValue = "";

        public string _id { get { return idValue; } set { idValue = value; NotifyPropertyChanged("_id"); } }
        public string key { get { return keyValue; } set { keyValue = value; NotifyPropertyChanged("key"); } }
        public string parentKey { get { return parentKeyValue; } set { parentKeyValue = value; NotifyPropertyChanged("parentKey"); } }
        public string description { get { return descriptionValue; } set { descriptionValue = value; NotifyPropertyChanged("description"); } }
        public string keynoteSet_id { get { return keynoteSetIdValue; } set { keynoteSetIdValue = value; NotifyPropertyChanged("keynoteSet_id"); } }

        public KeynoteInfo()
        {
        }

        public KeynoteInfo(string idStr, string keyStr, string parentKeyStr, string descriptionStr, string keynoteSetIdStr)
        {
            _id = idStr;
            key = keyStr;
            parentKey = parentKeyStr;
            description = descriptionStr;
            keynoteSet_id = keynoteSetIdStr;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}
