using PcRemote.ViewModels;

namespace PcRemote.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ShutDownPage : ContentView
    {
        readonly ShutDownViewModel viewModel;

        public ShutDownPage()
        {
            InitializeComponent();

            BindingContext = viewModel = new ShutDownViewModel();
        }
    }
}