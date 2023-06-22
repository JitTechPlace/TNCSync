using Haley.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TNCSync.Model
{
    public class SingletonData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propname = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }

        public IDialogService GlobalDialogSerivce { get; set; }

        public static SingletonData Instance = new SingletonData();

        private SingletonData()
        {
            
        }
    }
}
