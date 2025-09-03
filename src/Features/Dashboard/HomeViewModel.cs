namespace ExamEngine.Features.Dashboard;

using ExamEngine.Common;

public sealed class HomeViewModel : ViewModelBase
{
    string _greeting = "Loading...";
    public string Greeting
    {
        get => _greeting;
        set => SetProperty(ref _greeting, value);
    }

    public async Task LoadAsync()
    {
        try
        {
            IsBusy = true;
            // Simulate a tiny load; in real app, query progress/attempts
            await Task.Delay(50);
            Greeting = "Recent Attempt";
        }
        finally
        {
            IsBusy = false;
        }
    }
}

