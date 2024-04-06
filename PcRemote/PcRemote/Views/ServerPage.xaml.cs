using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using PcRemote.Models;
using PcRemote.Views;
using PcRemote.ViewModels;

namespace PcRemote.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ServerPage : ContentPage
    {
        ServerViewModel viewModel;

        public ServerPage()
        {
            InitializeComponent();

            BindingContext = viewModel = new ServerViewModel();

            MessagingCenter.Subscribe<MainPage, ClientCommand>(this, "NewClientCommand", (obj, command) =>
            {
                viewModel.setStatus(command.Text);
            });
        }

        void Show_Clicked(object sender, EventArgs e)
        {
            MessagingCenter.Send(this, "NewCommand", new ServerCommand("show"));
        }

        void Hide_Clicked(object sender, EventArgs e)
        {
            MessagingCenter.Send(this, "NewCommand", new ServerCommand("hide"));
        }

        void KillActiveWindow_Clicked(object sender, EventArgs e)
        {
            MessagingCenter.Send(this, "NewCommand", new ServerCommand("KillActiveWindow"));
        }

        void Shutdown_Clicked(object sender, EventArgs e)
        {
            MessagingCenter.Send(this, "NewCommand", new ServerCommand("shutdown -s -f -t 1"));
        }

        void VolumeUp_Clicked(object sender, EventArgs e)
        {
            MessagingCenter.Send(this, "NewCommand", new ServerCommand("volume up"));
        }

        void VolumeDown_Clicked(object sender, EventArgs e)
        {
            MessagingCenter.Send(this, "NewCommand", new ServerCommand("volume down"));
        }

        private void ScreenOn_Clicked(object sender, EventArgs e)
        {
            MessagingCenter.Send(this, "NewCommand", new ServerCommand("screen on"));
        }

        private void ScreenOff_Clicked(object sender, EventArgs e)
        {
            MessagingCenter.Send(this, "NewCommand", new ServerCommand("screen off"));
        }
    }
}