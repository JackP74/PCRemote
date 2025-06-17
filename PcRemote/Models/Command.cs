namespace PcRemote.Models
{
    class ServerCommand(string Text)
    {
        public string Text { get; set; } = Text;
    }

    class ClientCommand(string Text)
    {
        public string Text { get; set; } = Text;
    }

    class ScreenImage(byte[] Image)
    {
        public byte[] Image { get; set; } = Image;
    }
}