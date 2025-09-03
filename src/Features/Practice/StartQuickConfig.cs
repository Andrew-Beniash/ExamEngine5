namespace ExamEngine.Features.Practice;

public sealed class StartQuickConfig
{
    public int Count { get; set; } = 5;
    public string[] TopicIds { get; set; } = Array.Empty<string>();
    public bool Timed { get; set; } = false;
}

