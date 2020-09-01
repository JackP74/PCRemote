using PcRemote.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using System.Drawing;
using System.IO;

namespace PcRemote.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ImagePage : ContentPage
    {
        ImageViewModel viewModel;

        public ImagePage()
        {
            InitializeComponent();

            BindingContext = viewModel = new ImageViewModel();
        }

        public byte[] ImageToByteArray(System.Drawing.Image imageIn)
        {
            byte[] result = null;
            using (MemoryStream ms = new MemoryStream())
            {
                imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                result = ms.ToArray();
            }
            return result;
        }

        public void SetImage(System.Drawing.Image NewImage)
        {
            //viewModel.Image = (Android.Graphics.Bitmap)NewImage;
            //viewModel.Image = ImageSourceConverter

            Byte[] byteArray = ImageToByteArray(NewImage);
            Stream stream = new MemoryStream(byteArray);

            ImgMain.Source = ImageSource.FromStream(() => { return stream; });

        }

    }
}