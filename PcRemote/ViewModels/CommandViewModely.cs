namespace PcRemote.ViewModels
{
    public class CommandViewModel : BaseViewModel
    {
        private string commandtitle = "";
        private string commandfinal = "";

        public CommandViewModel(string commandTitle)
        {
            Title = "Command";
            CommandTitle = commandTitle;
        }

        public string CommandTitle
        {
            get { return commandtitle; }
            set { SetProperty(ref commandtitle, value); }
        }

        public string CommandFinal
        {
            get { return commandfinal; }
            set { SetProperty(ref commandfinal, value); }
        }
    }
}