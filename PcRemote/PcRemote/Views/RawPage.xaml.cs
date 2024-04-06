using PcRemote.Models;
using PcRemote.ViewModels;
using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PcRemote.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RawPage : ContentPage
    {
        RawViewModel viewModel;

        public RawPage()
        {
            InitializeComponent();

            BindingContext = viewModel = new RawViewModel();

            MessagingCenter.Subscribe<MainPage, ClientCommand>(this, "NewMessage", (obj, newMessage) =>
            {
                viewModel.CheckMessage(newMessage.Text);
            });
        }

        void RawCommand_Clicked(object sender, EventArgs e)
        {
            if(!string.IsNullOrWhiteSpace(txtRawInput.Text)) { 
                if(txtRawInput.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None).Count() > 1)
                {
                    foreach(string item in txtRawInput.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
                    {
                        MessagingCenter.Send(this, "NewCommand", new ServerCommand(item));
                    }
                }
                else
                {
                    MessagingCenter.Send(this, "NewCommand", new ServerCommand(txtRawInput.Text));
                }

                txtRawInput.Text = string.Empty;
            }
        }
    }
}