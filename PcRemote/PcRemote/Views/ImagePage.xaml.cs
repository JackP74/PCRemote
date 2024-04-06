using System;
using System.IO;
using System.Threading;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using PcRemote.Models;
using PcRemote.ViewModels;
using Xamarin.Essentials;

namespace PcRemote.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ImagePage : ContentPage
    {
        private readonly ImageViewModel viewModel;
        private TouchTracking.TouchActionType lastAction = TouchTracking.TouchActionType.Cancelled;
        private Size currentSize;

        public ImagePage()
        {
            InitializeComponent();

            BindingContext = viewModel = new ImageViewModel();
            viewModel.MaxWidth = 640;
            viewModel.ButtonColor = "#992196F3";

            MessagingCenter.Subscribe<MainPage, byte[]>(this, "NewImage", (obj, rawImage) =>
            {
                StartThread(() =>
                {
                    var newStream = new MemoryStream(rawImage);
                    var newSource = ImageSource.FromStream(() => { return newStream; });

                    viewModel.Image = newSource;
                });
            });
        }

        private void StartThread(ThreadStart threadStart)
        {
            Thread newThread = new Thread(threadStart)
            { IsBackground = true };
            newThread.SetApartmentState(ApartmentState.STA);
            newThread.Start();
        }

        private void MainGrid_SizeChanged(object sender, EventArgs e)
        {
            currentSize = new Size(Convert.ToDouble(MainGrid.Width.ToString("0.##")), Convert.ToDouble(MainGrid.Height.ToString("0.##")));
            viewModel.MaxWidth = Convert.ToInt32(MainGrid.Width);
        }

        private void TouchEffect_TouchAction(object sender, TouchTracking.TouchActionEventArgs args)
        {
            try
            {
                if (args.Type == TouchTracking.TouchActionType.Released && lastAction == TouchTracking.TouchActionType.Pressed)
                    MessagingCenter.Send(this, "NewTap", $"1 0 0 0 0");

                else if (args.Type == TouchTracking.TouchActionType.Moved)
                    MessagingCenter.Send(this, "NewTap", $"0 {args.Location.X:0.##} {args.Location.Y:0.##} {currentSize.Width} {currentSize.Height}");

                lastAction = args.Type;

            } catch { }
        }

        private void BtnClick_Clicked(object sender, EventArgs e)
        {
            MessagingCenter.Send(this, "NewTap", $"1 0 0 0 0");
        }

        private void BtnDoubleClick_Clicked(object sender, EventArgs e)
        {
            MessagingCenter.Send(this, "NewTap", $"2 0 0 0 0");
        }

        private void BtnRightClick_Clicked(object sender, EventArgs e)
        {
            MessagingCenter.Send(this, "NewTap", $"3 0 0 0 0");
        }

        private void BtnKeyboard_Clicked(object sender, EventArgs e)
        {
            NormalOverlayBtns.IsVisible = false;
            KeyboardOverlayBtns.IsVisible = true;
        }

        private void BtnExitKeyboard_Clicked(object sender, EventArgs e)
        {
            KeyboardOverlayBtns.IsVisible = false;
            NormalOverlayBtns.IsVisible = true;
        }

        private void BtnKey_Clicked(object sender, EventArgs e)
        {
            if (sender is Button Btn)
            {
                string ToSend = Btn.Text.ToLower();
                MessagingCenter.Send(this, "NewKeys", ToSend);
            }
        }

        private void BtnBackSpace_Clicked(object sender, EventArgs e)
        {
            MessagingCenter.Send(this, "NewKeys", "{BACKSPACE}");
        }

        private void BtnEnter_Clicked(object sender, EventArgs e)
        {
            MessagingCenter.Send(this, "NewKeys", "{ENTER}");
        }

        private void BtnSpace_Clicked(object sender, EventArgs e)
        {
            MessagingCenter.Send(this, "NewKeys", " ");
        }
    }
}