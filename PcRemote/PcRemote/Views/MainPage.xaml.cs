using System;
using System.ComponentModel;
using Xamarin.Forms;
using PcRemote.classes;
using PcRemote.Models;

using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;

using FFImageLoading;
using Xamarin.Essentials;

namespace PcRemote.Views
{
    [DesignTimeVisible(false)]
    public partial class MainPage : TabbedPage
    {
        private readonly Server MainServer;
        public Size ScreenSize;
        
        public MainPage()
        {
            InitializeComponent();

            var config = new FFImageLoading.Config.Configuration()
            {
                FadeAnimationEnabled = false,
                DownsampleInterpolationMode = FFImageLoading.Work.InterpolationMode.Low,
                DecodingMaxParallelTasks = Environment.ProcessorCount,
                SchedulerMaxParallelTasks = Math.Max(32, 2 + Environment.ProcessorCount * 2),
                DelayInMs = 0,
                InvalidateLayout = false
            };

            ImageService.Instance.Initialize(config);

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