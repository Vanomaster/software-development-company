using System.Windows;

namespace Gui.Views;

public class PasswordBoxAttachedProperties
{
    public static string GetEncryptedPassword(DependencyObject obj)
    {
        return (string)obj.GetValue(EncryptedPasswordProperty);
    }

    public static void SetEncryptedPassword(DependencyObject obj, string value)
    {
        obj.SetValue(EncryptedPasswordProperty, value);
    }

    public static readonly DependencyProperty EncryptedPasswordProperty =
        DependencyProperty.RegisterAttached("EncryptedPassword", typeof(string), typeof(PasswordBoxAttachedProperties));
}