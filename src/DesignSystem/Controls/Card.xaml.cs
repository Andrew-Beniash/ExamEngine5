namespace ExamEngine.DesignSystem.Controls;

public partial class Card : ContentView
{
    public static readonly BindableProperty CornerRadiusProperty = BindableProperty.Create(
        nameof(CornerRadius), typeof(CornerRadius), typeof(Card), new CornerRadius(12),
        propertyChanged: static (b, o, n) => ((Card)b).ApplyCornerRadius());

    public static readonly BindableProperty CardPaddingProperty = BindableProperty.Create(
        nameof(CardPadding), typeof(Thickness), typeof(Card), new Thickness(16),
        propertyChanged: static (b, o, n) => ((Card)b).ApplyPadding());

    public static readonly BindableProperty ElevationProperty = BindableProperty.Create(
        nameof(Elevation), typeof(double), typeof(Card), 8d,
        propertyChanged: static (b, o, n) => ((Card)b).ApplyElevation());

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public Thickness CardPadding
    {
        get => (Thickness)GetValue(CardPaddingProperty);
        set => SetValue(CardPaddingProperty, value);
    }

    public double Elevation
    {
        get => (double)GetValue(ElevationProperty);
        set => SetValue(ElevationProperty, value);
    }

    public Card()
    {
        InitializeComponent();
        ApplyCornerRadius();
        ApplyPadding();
        ApplyElevation();
    }

    void ApplyCornerRadius()
    {
        if (CardShape is not null)
        {
            CardShape.CornerRadius = CornerRadius;
        }
    }

    void ApplyPadding()
    {
        if (CardBorder is not null)
        {
            CardBorder.Padding = CardPadding;
        }
    }

    void ApplyElevation()
    {
        if (CardShadow is not null)
        {
            // Map elevation to shadow radius; simple heuristic
            CardShadow.Radius = Math.Max(0, Elevation);
            CardShadow.Opacity = Elevation <= 0 ? 0 : 0.25;
            CardShadow.Offset = Elevation <= 0 ? new Point(0, 0) : new Point(0, 2);
        }
    }
}

