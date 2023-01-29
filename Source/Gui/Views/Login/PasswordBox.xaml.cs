using System.Windows.Controls;

namespace Gui.Views.L;

/// <summary>
/// Interaction logic for PasswordBox.xaml.
/// </summary>
public partial class PasswordBox : UserControl, IPasswordProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PasswordBox"/> class.
    /// </summary>
    public PasswordBox()
    {
        InitializeComponent();
    }

    /// <inheritdoc/>
    public string GetPassword()
    {
        return PassBox.Password;
    }
}