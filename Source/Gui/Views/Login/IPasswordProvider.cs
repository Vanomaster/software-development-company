namespace Gui.Views.L;

/// <summary>
/// Interface of password provider.
/// </summary>
public interface IPasswordProvider
{
    /// <summary>
    /// Get password from PasswordBox.
    /// </summary>
    /// <returns>Password.</returns>
    string GetPassword();
}