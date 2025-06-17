namespace PcRemote.ViewModels
{
    public class ServerViewModel : BaseViewModel
    {
        private string labelStatusText = "";

        public ServerViewModel()
        {
            Title = "Server";
            labelStatusText = "Status: Not Connected";
        }

        public string LabelStatusText
        {
            get { return labelStatusText; }
            set { SetProperty(ref labelStatusText, value); }
        }

        public void setStatus(string newStatus)
        {
            if (string.IsNullOrWhiteSpace(newStatus))
            {
                LabelStatusText = "Status: Unkown";
            }
            else
            {
                LabelStatusText = "Status: " + newStatus;
            }
        }
    }
}