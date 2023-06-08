using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DupeBuster.App.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private string greeting = "Welcome to Avalonia!";

    public ICommand DoTheThing { get; }

    public MainWindowViewModel()
    {
        DoTheThing = new RelayCommand(RunTheThing);
        Task.Run(async () =>
        {
            await Task.Delay(2000);
            Greeting = "Kek";
        });
    }

    public void RunTheThing()
    {
        Greeting = "Thank you for clicking the button!";
    }
}