namespace ExamEngine.Features.Practice;

public partial class PracticePage : ContentPage
{
    readonly PracticeViewModel _vm = new();

    public PracticePage()
    {
        InitializeComponent();
        BindingContext = _vm;
    }
}
