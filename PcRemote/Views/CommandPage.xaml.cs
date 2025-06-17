using PcRemote.ViewModels;

namespace PcRemote.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CommandPage : ContentPage
    {
        readonly CommandViewModel viewModel;

        public CommandPage(string commandTitle)
        {
            InitializeComponent();

            BindingContext = viewModel = new CommandViewModel(commandTitle);
        }
    }
}