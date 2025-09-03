namespace ExamEngine;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        // Register practice routes to navigate to placeholders
        Routing.RegisterRoute("practice/adaptive", typeof(Features.Practice.Adaptive.AdaptivePage));
        Routing.RegisterRoute("practice/quick", typeof(Features.Practice.Quick.QuickPage));
        Routing.RegisterRoute("practice/full", typeof(Features.Practice.Full.FullExamPage));

        // Exam and Review routes (query parameter: attemptId)
        Routing.RegisterRoute("exam", typeof(Features.Exam.ExamPage));
        Routing.RegisterRoute("review", typeof(Features.Review.ReviewPage));
    }
}
