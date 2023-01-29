namespace Commands.Base;

/// <summary>
/// Command result.
/// </summary>
public interface ICommandResult
{
    /// <summary>
    /// Error message.
    /// </summary>
    public string ErrorMessage { get; }

    /// <summary>
    /// Whether executed successfully.
    /// </summary>
    public bool IsSuccessful { get; }
}