using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace PZ18.ViewModels.dialogs; 

public partial class ConfirmationDialog : Window {
    private readonly Action<ConfirmationDialog> _positiveClick;
    private readonly Action<ConfirmationDialog> _negativeClick;

    public ConfirmationDialog(
        string headerText,
        string descriptionText,
        Action<ConfirmationDialog> positiveClick,
        Action<ConfirmationDialog> negativeClick
        ) {
        _positiveClick = positiveClick;
        _negativeClick = negativeClick;
        InitializeComponent();
        Header.Text = headerText;
        Description.Text = descriptionText;
    }

    private void ConfirmClick(object sender, RoutedEventArgs e) {
        _positiveClick.Invoke(this);
        Close();
    }
    
    private void CancelClick(object sender, RoutedEventArgs e) {
        _negativeClick.Invoke(this);
        Close();
    }
}