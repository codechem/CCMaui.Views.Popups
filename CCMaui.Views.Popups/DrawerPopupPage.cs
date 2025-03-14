namespace CCMaui.Views.Popups;

public class DrawerPopupPage : CCPopupPage
{
    private bool _isFirstAnimation = true;

    public static readonly BindableProperty IsPanEnabledProperty =
        BindableProperty.Create(
            nameof(IsPanEnabled),
            typeof(bool),
            typeof(DrawerPopupPage),
            true
        );

    public bool IsPanEnabled
    {
        get => (bool)GetValue(IsPanEnabledProperty);
        set => SetValue(IsPanEnabledProperty, value);
    }
    
    public override void SetPreAnimationState()
    {
        PopupContent.TranslationY = DeviceDisplay.MainDisplayInfo.Height * DeviceDisplay.MainDisplayInfo.Density;
        PopupBackground.BackgroundColor = Colors.Transparent;
        var panGesture = new PanGestureRecognizer();
        panGesture.PanUpdated += OnPanUpdate;
        PopupContent.GestureRecognizers.Add(panGesture);
    }

    public override async Task AnimateAppearingAsync()
    {
        if (_isFirstAnimation)
        {
            PopupContent.TranslationY = PopupContent.Height > 0
                ? PopupContent.Height
                : PopupContent.Measure(double.PositiveInfinity,
                        double.PositiveInfinity, MeasureFlags.IncludeMargins)
                    .Request.Height;

            _isFirstAnimation = false;
        }

        var easing = Easing.SinOut;

        await Task.WhenAll(
            PopupContent.TranslateTo(0, 0, AppearingDuration, easing),
            PopupBackground.ColorTo(PopupBackground.BackgroundColor, BackgroundDimTargetColor,
                color => { PopupBackground.BackgroundColor = color; }, AppearingDuration, easing));
    }

    public override async Task AnimateDisappearingAsync()
    {
        var hideTranslation = PopupContent.Height > 0
            ? PopupContent.Height
            : PopupContent.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.IncludeMargins)
                .Request.Height;

        var easing = Easing.SinIn;

        await Task.WhenAll(
            PopupContent.TranslateTo(0, hideTranslation, DisappearingDuration, easing),
            PopupBackground.ColorTo(PopupBackground.BackgroundColor, Colors.Transparent,
                color => { PopupBackground.BackgroundColor = color; }, DisappearingDuration, easing));
    }

    public override void OnDispose()
    {
        base.OnDispose();
        PopupContent.GestureRecognizers.Clear();
    }

    private void OnPanUpdate(object? sender, PanUpdatedEventArgs args)
    {
        if (!IsPanEnabled) return;
        if (sender is not View) return;

        if (args.StatusType is GestureStatus.Canceled or GestureStatus.Completed)
        {
            if (PopupContent.TranslationY > PopupContent.Height / 2)
                SafeClose();
            else
                Task.Run(AnimateAppearingAsync);
        }
        else
        {
            var drawerTranslationY = DeviceInfo.Platform == DevicePlatform.Android
                ? PopupContent.TranslationY + args.TotalY
                : args.TotalY;

            if (drawerTranslationY <= 0)
            {
                PopupContent.TranslationY = 0;
                return;
            }

            PopupContent.TranslationY = drawerTranslationY;

            SetBackgroundDim(drawerTranslationY);
        }
    }

    private void SetBackgroundDim(double drawerTranslationY)
    {
        var panHeightFraction = drawerTranslationY / PopupContent.Height;

        var currentColor = BackgroundDimTargetColor;
        currentColor = Color.FromRgba(currentColor.Red, currentColor.Green, currentColor.Blue,
            currentColor.Alpha - currentColor.Alpha * panHeightFraction);

        PopupBackground.BackgroundColor = currentColor;
    }

    private void SafeClose()
    {
        MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await AnimateDisappearingAsync();
            await Application.Current!.MainPage!.Navigation.PopModalAsync(false);
        });
    }
}