using System;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using PcRemote.classes;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using PcRemote.Models;
using Android.Graphics;
using System.Drawing;

namespace PcRemote.Views
{
    [DesignTimeVisible(false)]
    public partial class MainPage : TabbedPage
    {
        Server MainServer;
        
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
                var newCommand = command as ServerCommand;
                MainServer.AddCommand(newCommand);
            });

            // Commands from PMP page
            MessagingCenter.Subscribe<PmpPage, ServerCommand>(this, "NewCommand", (obj, command) =>
            {
                var newCommand = command as ServerCommand;
                MainServer.AddCommand(newCommand);
            });

            // Commands from Raw page
            MessagingCenter.Subscribe<RawPage, ServerCommand>(this, "NewCommand", (obj, command) =>
            {
                var newCommand = command as ServerCommand;
                MainServer.AddCommand(newCommand);
            });
        }

        private void Server_StateChanged(bool Connected)
        {
            ClientCommand commandToSend;

            if(Connected)
            {
                commandToSend = new ClientCommand("Connected");
            } 
            else
            {
                commandToSend = new ClientCommand("Not Connected");
            }

            MessagingCenter.Send(this, "NewClientCommand", commandToSend);
        }

        private void Server_NewMessage(string Message)
        {
            MessagingCenter.Send(this, "NewMessage", new ClientCommand(Message));
        }

        private void Server_NewImage(System.Drawing.Image NewImage)
        {
            
        }
    }
}