namespace CCMaui.Views.Popups;

public abstract class CCPopupPage : ContentPage
{
    private readonly Grid _popupPageLayout;
    protected readonly VerticalStackLayout PopupBackground;

    public static readonly BindableProperty AppearingDurationProperty =
        BindableProperty.Create(
            nameof(CCPopupPage),
            typeof(uint),
            typeof(CCPopupPage),
            250
        );

    public static readonly BindableProperty DisappearingDurationProperty =
        BindableProperty.Create(
            nameof(DisappearingDuration),
            typeof(uint),
            typeof(CCPopupPage),
            250
        );

    public static readonly BindableProperty BackgroundDimTargetColorProperty =
        BindableProperty.Create(
            nameof(BackgroundDimTargetColor),
            typeof(Color),
            typeof(CCPopupPage),
            Color.FromArgb("#A6000000")
        );

    public static readonly BindableProperty PopupContentProperty =
        BindableProperty.Create(
            nameof(PopupContent),
            typeof(View),
            typeof(CCPopupPage),
            defaultValue: null,
            propertyChanged: PopupContentChanged
        );

    public uint AppearingDuration
    {
        get => (uint)GetValue(AppearingDurationProperty);
        set => SetValue(AppearingDurationProperty, value);
    }

    public uint DisappearingDuration
    {
        get => (uint)GetValue(DisappearingDurationProperty);
        set => SetValue(DisappearingDurationProperty, value);
    }

    public Color BackgroundDimTargetColor
    {
        get => (Color)GetValue(BackgroundDimTargetColorProperty);
        set => SetValue(BackgroundDimTargetColorProperty, value);
    }

    public View PopupContent
    {
        get => (View)GetValue(PopupContentProperty);
        set => SetValue(PopupContentProperty, value);
    }

    public new View Content
    {
        get => (View)GetValue(ContentProperty);
        private init => SetValue(ContentProperty, value);
    }

    protected CCPopupPage()
    {
        BackgroundColor = Colors.Transparent;
        _popupPageLayout = new Grid
        {
            RowDefinitions = [new RowDefinition(GridLength.Star)],
            ColumnDefinitions = [new ColumnDefinition(GridLength.Star)],
            IgnoreSafeArea = true,
            BackgroundColor = Colors.Transparent,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
        };

        PopupBackground = new VerticalStackLayout
        {
            BackgroundColor = Colors.Transparent,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill
        };

        PopupBackground.SetValue(Grid.RowProperty, 0);
        PopupBackground.SetValue(Grid.ColumnProperty, 0);

        _popupPageLayout.Children.Add(PopupBackground);

        Content = _popupPageLayout;
    }

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        switch (propertyName)
        {
            case nameof(Content) when Content != _popupPageLayout:
                throw new InvalidOperationException(
                    $"Incorrect use of {nameof(CCPopupPage)}. Modify {nameof(PopupContent)} instead of {nameof(Content)}.");
            case nameof(Window) when Window is null:
                OnDispose();
                break;
        }
    }

    private static void PopupContentChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not CCPopupPage popup || newValue is not View newPopupContent) return;

        if (oldValue is View)
            throw new InvalidOperationException(
                $"Incorrect use of {nameof(CCPopupPage)}. Cannot modify {nameof(PopupContent)} once it has been set.");

        newPopupContent.SetValue(Grid.RowProperty, 0);
        newPopupContent.SetValue(Grid.ColumnProperty, 0);

        popup._popupPageLayout.Children.Add(newPopupContent);
        popup.SetPreAnimationState();
    }

    public virtual void OnDispose()
    {
    }

    public abstract void SetPreAnimationState();
    public abstract Task AnimateAppearingAsync();
    public abstract Task AnimateDisappearingAsync();
}