namespace PcRemote.ViewModels
{
    public class ImageViewModel : BaseViewModel
    {
        private ImageSource? image;
        private int maxWidth;

        public ImageViewModel()
        {
            Title = "Image";
        }

        public ImageSource? Image
        {
            get { return image; }
            set { SetProperty(ref image, value); }
        }

        public int MaxWidth
        {
            get { return maxWidth; }
            set { SetProperty(ref maxWidth, value); }
        }
    }
}