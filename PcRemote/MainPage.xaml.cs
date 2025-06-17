using PcRemote.classes;
using PcRemote.Models;
using PcRemote.Views;

namespace PcRemote
{
    public partial class MainPage : ContentPage
    {
        private readonly Server MainServer;
        public Size ScreenSize;

        public MainPage()
        {
            InitializeComponent();

            MainServer = new Server();
            MainServer.StateChanged += Server_StateChanged;
            MainServer.NewMessage += Server_NewMessage;
            MainServer.NewImage += Server_NewImage;

            MainServer.ConnectToServer();

            // Commands from Server page
            MessagingCenter.Subscribe<ServerPage, ServerCommand>(this, "NewCommand", (obj, command) =>
            {
                MainServer.AddCommand(command);
            });

            // Commands from PMP page
            MessagingCenter.Subscribe<PmpPage, ServerCommand>(this, "NewCommand", (obj, command) =>
            {
                MainServer.AddCommand(command);
            });

            // Commands from Raw page
            MessagingCenter.Subscribe<RawPage, ServerCommand>(this, "NewCommand", (obj, command) =>
            {
                MainServer.AddCommand(command);
            });

            // Commands from PC page
            MessagingCenter.Subscribe<ImagePage, string>(this, "NewTap", (obj, newPoint) =>
            {
                try { MainServer.SendChatterPacket("mouse " + newPoint); } catch { }
            });

            MessagingCenter.Subscribe<ImagePage, string>(this, "NewKeys", (obj, newKeys) =>
            {
                try { MainServer.SendChatterPacket("keyboard " + newKeys); } catch { }
            });
        }

        private void Server_StateChanged(bool Connected)
        {
            ClientCommand commandToSend = new ClientCommand(Connected ? "Connected" : "Not Connected");
            MessagingCenter.Send(this, "NewClientCommand", commandToSend);
        }

        private void Server_NewMessage(string Message)
        {
            MessagingCenter.Send(this, "NewMessage", new ClientCommand(Message));
        }

        private void Server_NewImage(byte[] NewImage)
        {
            MessagingCenter.Send(this, "NewImage", NewImage);
        }
    }

}
