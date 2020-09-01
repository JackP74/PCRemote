using Android.Graphics;
using System;
using System.Windows.Input;

using Xamarin.Forms;

namespace PcRemote.ViewModels
{
    public class ImageViewModel : BaseViewModel
    {
        private Bitmap image;

        public ImageViewModel()
        {
            Title = "Image";
        }

        public Bitmap Image
        {
            get { return image; }
            set { SetProperty(ref image, value); }
        }
    }
}