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
    public partial class CommandPage : ContentPage
    {
        CommandViewModel viewModel;

        public CommandPage(string commandTitle)
        {
            InitializeComponent();

            BindingContext = viewModel = new CommandViewModel(commandTitle);
        }
    }
}