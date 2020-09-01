using PcRemote.Models;
using PcRemote.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PcRemote.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PmpPage : ContentPage
    {
        PmpViewModel viewModel;

        public PmpPage()
        {
            InitializeComponent();

            BindingContext = viewModel = new PmpViewModel();
        }

        void Play_Clicked(object sender, EventArgs e)
        {
            MessagingCenter.Send(this, "NewCommand", new ServerCommand("PMP play"));
        }

        void Pause_Clicked(object sender, EventArgs e)
        {
            MessagingCenter.Send(this, "NewCommand", new ServerCommand("PMP pause"));
        }

        void Forward_Clicked(object sender, EventArgs e)
        {
            MessagingCenter.Send(this, "NewCommand", new ServerCommand("PMP forward"));
        }

        void Backward_Clicked(object sender, EventArgs e)
        {
            MessagingCenter.Send(this, "NewCommand", new ServerCommand("PMP backward"));
        }

        void Next_Clicked(object sender, EventArgs e)
        {
            MessagingCenter.Send(this, "NewCommand", new ServerCommand("PMP next"));
        }

        void Previous_Clicked(object sender, EventArgs e)
        {
            MessagingCenter.Send(this, "NewCommand", new ServerCommand("PMP previous"));
        }

        void VolumeUp_Clicked(object sender, EventArgs e)
        {
            MessagingCenter.Send(this, "NewCommand", new ServerCommand("PMP volumeup 5"));
        }

        void VolumeDown_Clicked(object sender, EventArgs e)
        {
            MessagingCenter.Send(this, "NewCommand", new ServerCommand("PMP volumedown 5"));
        }

        void Mute_Clicked(object sender, EventArgs e)
        {
            MessagingCenter.Send(this, "NewCommand", new ServerCommand("PMP mute"));
        }

        private void btnAutoPlay_Clicked(object sender, EventArgs e)
        {
            MessagingCenter.Send(this, "NewCommand", new ServerCommand("PMP autplay"));
        }
    }
}