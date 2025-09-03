namespace ExamEngine.Features.Review;

[QueryProperty(nameof(AttemptId), "attemptId")]
public partial class ReviewPage : ContentPage
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

    public ReviewPage()
    {
        InitializeComponent();
    }
}

