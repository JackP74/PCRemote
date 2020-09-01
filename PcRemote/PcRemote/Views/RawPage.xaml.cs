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

        private async Task SendFileAsync()
        {
            try
            {
                FileData fileData = await CrossFilePicker.Current.PickFile();

                string fileName = fileData.FileName;
                string contents = System.Text.Encoding.UTF8.GetString(fileData.DataArray);

                System.Console.WriteLine("File name chosen: " + fileName);
                System.Console.WriteLine("File data: " + contents);

                Debug.WriteLine(fileName);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Exception choosing file: " + ex.ToString());
            }
        }

        void RawCommand_Clicked(object sender, EventArgs e)
        {
            if(!string.IsNullOrWhiteSpace(txtRawInput.Text)) { 
                if(txtRawInput.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None).Count() > 1)
                {
                    foreach(string item in txtRawInput.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
                    {
                        if (string.IsNullOrWhiteSpace(item)) continue;

                        if (item.ToLower().Trim() == "sendfile")
                        {
                            Task task = new Task(() => { _ = SendFileAsync(); });
                            task.Start();
                        }

                        MessagingCenter.Send(this, "NewCommand", new ServerCommand(item));
                    }
                }
                else
                {
                    if (txtRawInput.Text.ToLower().Trim() == "sendfile")
                    {
                        Task task = new Task(() => { _ = SendFileAsync(); });
                        task.Start();
                    }
                    MessagingCenter.Send(this, "NewCommand", new ServerCommand(txtRawInput.Text));
                }

                txtRawInput.Text = string.Empty;
            }
        }
    }
}