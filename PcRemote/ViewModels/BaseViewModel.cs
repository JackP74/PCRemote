using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PcRemote.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        #region "varibales"
        string title = string.Empty;
        bool isBusy = false;
        string background = "#363636";
        string foreground = "#e0e0e0";
        string buttoncolor = "#288cc9";
        #endregion

        #region "proprieties"
        public bool IsBusy
        {
            get { return isBusy; }
            set { SetProperty(ref isBusy, value); }
        }

        public string Title
        {
            get { return title; }
            set { SetProperty(ref title, value); }
        }

        public string BackGround
        {
            get { return background; }
            set { SetProperty(ref background, value); }
        }

        public string ForeGround
        {
            get { return foreground; }
            set { SetProperty(ref foreground, value); }
        }

        public string ButtonColor
        {
            get { return buttoncolor; }
            set { SetProperty(ref buttoncolor, value); }
        }
        #endregion

        #region "events"
        public event PropertyChangedEventHandler? PropertyChanged;
        #endregion

        #region "functions"

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "", Action? onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
