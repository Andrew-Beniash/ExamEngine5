namespace ExamEngine.Features.Exam;

[QueryProperty(nameof(AttemptId), "attemptId")]
public partial class ExamPage : ContentPage
{
    string? _attemptId;
    public string? AttemptId
    {
        get => _attemptId;
        set
        {
            _attemptId = value;
            ParamLabel.Text = $"attemptId: {value ?? "(none)"}";
        }
    }

    public ExamPage()
    {
        InitializeComponent();
    }
}

