namespace ExamEngine.DesignSystem.Controls;

public partial class PrimaryButton : Button
{
    public PrimaryButton()
    {
        InitializeComponent();

        // Ensure minimum accessible touch target
        MinimumHeightRequest = 44;
        MinimumWidthRequest = 44;

        // Allow consumers to bind Command/CommandParameter as usual on Button
        // No extra code needed; default Button properties are inherited.
    }
}

