// Matheus Marques Stefani
using System.Windows.Input;
using AcademiaDoZe.Presentation.AppMaui.ViewModels;
namespace AcademiaDoZe.Presentation.AppMaui.Views;

internal static class Ui
{
    public static Label Title(string text) => new() { Text = text, FontSize = 30, FontAttributes = FontAttributes.Bold };
    public static Label Caption(string text) => new() { Text = text, FontSize = 14 };
    public static Button Button(string text, string icon, ICommand? command = null) => new() { Text = text, ImageSource = icon, Command = command, ContentLayout = new Microsoft.Maui.Controls.Button.ButtonContentLayout(Microsoft.Maui.Controls.Button.ButtonContentLayout.ImagePosition.Left, 10) };
    public static View Field(string label, string path, bool password = false)
    {
        var entry = new Entry { Placeholder = label, IsPassword = password };
        entry.SetBinding(Entry.TextProperty, new Binding(path, BindingMode.TwoWay));
        return new VerticalStackLayout { Spacing = 4, Children = { new Label { Text = label, FontSize = 13 }, entry } };
    }
    public static VerticalStackLayout Body(params View[] views)
    {
        var layout = new VerticalStackLayout { Padding = 28, Spacing = 18, MaximumWidthRequest = 1050, HorizontalOptions = LayoutOptions.Fill };
        foreach (var view in views) layout.Children.Add(view);
        return layout;
    }
    public static Label Status()
    {
        var label = new Label { FontSize = 14 }; label.SetBinding(Label.TextProperty, nameof(BaseViewModel.Status)); return label;
    }
    public static ActivityIndicator Busy()
    {
        var indicator = new ActivityIndicator { Color = Color.FromArgb("#007E80") };
        indicator.SetBinding(ActivityIndicator.IsRunningProperty, nameof(BaseViewModel.IsBusy)); return indicator;
    }
}
