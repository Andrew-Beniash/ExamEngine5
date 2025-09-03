using System.Windows.Input;
using ExamEngine.Common;

namespace ExamEngine.Features.Practice;

public sealed class PracticeViewModel : ViewModelBase
{
    public StartQuickConfig QuickConfig { get; } = new();

    public ICommand StartAdaptiveCommand { get; }
    public ICommand StartQuickCommand { get; }
    public ICommand StartFullCommand { get; }

    public PracticeViewModel()
    {
        StartAdaptiveCommand = new Command(async () => await NavigateAsync("practice/adaptive"));
        StartQuickCommand = new Command<StartQuickConfig>(async cfg => await NavigateAsync("practice/quick"));
        StartFullCommand = new Command(async () => await NavigateAsync("practice/full"));
    }

    static Task NavigateAsync(string route)
        => Shell.Current.GoToAsync(route);
}

