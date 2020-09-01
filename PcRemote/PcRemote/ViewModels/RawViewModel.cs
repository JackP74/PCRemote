using System;
using System.Windows.Input;

using Xamarin.Forms;

namespace PcRemote.ViewModels
{
    public class RawViewModel : BaseViewModel
    {
        private string messages = "";

        public RawViewModel()
        {
            Title = "Raw";
        }

        public string Messages
        {
            get { return messages; }
            set { SetProperty(ref messages, value); }
        }

        public void CheckMessage(string newMessage)
        {
            if(newMessage == ":c") { Messages = string.Empty; return; }

            Messages += Environment.NewLine + Environment.NewLine + newMessage;
            Messages = Messages.Trim();
        }
    }
}