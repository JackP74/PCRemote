namespace PcRemote.Models
{
    class ServerCommand
    {
        public string Text { get; set; }

        public ServerCommand(string Text) {
            this.Text = Text;
        }
    }

    class ClientCommand
    {
        public string Text { get; set; }

        public ClientCommand(string Text)
        {
            this.Text = Text;
        }
    }
}