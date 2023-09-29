using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace PZ18.ViewModels.dialogs;

public partial class GenericDialog : Window {
    public GenericDialog() {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
    }

    public void Open<T>(T obj, Window owner) {
        this.ShowDialog(owner);
        var fields = typeof(T).GetProperties()
            .Where(it => it.GetCustomAttribute<ColumnAttribute>() is not null
                         && it.GetCustomAttribute<DisplayNameAttribute>() is not null)
            .Select(it => new EditableFieldInfo(
                it.GetCustomAttribute<ColumnAttribute>()!,
                it.GetCustomAttribute<DisplayNameAttribute>()!,
                it
            ))
            .ToList();
        foreach (var field in fields) {
            var row = new StackPanel() {
                Spacing = 8
            };
            row.Children.Add(new TextBlock() {
                Text = field.DisplayNameAttribute.DisplayName
            });
            row.Children.Add(new TextBox() {
                Text = field.PropertyInfo.GetValue(obj)!.ToString()
            });
            Panel.Children.Add(
                row
            );
        }
    }

    public record EditableFieldInfo(ColumnAttribute ColumnAttribute, DisplayNameAttribute DisplayNameAttribute, PropertyInfo PropertyInfo);
}