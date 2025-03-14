namespace CCMaui.Views.Popups;

public static class CCPopupPageUtils
{
    private const string ColorAnimationName = "ColorTo";

    public static Task<bool> ColorTo(this VisualElement self, Color fromColor, Color toColor,
        Action<Color> callback,
        uint length = 250, Easing? easing = null)
    {
        return ColorAnimation(self, ColorAnimationName, Transform, callback, length, easing);

        Color Transform(double t) => Color.FromRgba(fromColor.Red + t * (toColor.Red - fromColor.Red),
            fromColor.Green + t * (toColor.Green - fromColor.Green),
            fromColor.Blue + t * (toColor.Blue - fromColor.Blue),
            fromColor.Alpha + t * (toColor.Alpha - fromColor.Alpha));
    }

    private static Task<bool> ColorAnimation(IAnimatable element, string name, Func<double, Color> transform,
        Action<Color> callback, uint length, Easing? easing)
    {
        easing ??= Easing.Linear;
        var taskCompletionSource = new TaskCompletionSource<bool>();

        element.Animate(name, transform, callback, 16, length, easing,
            (_, c) => taskCompletionSource.SetResult(c));
        return taskCompletionSource.Task;
    }
}