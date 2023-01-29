namespace Commands.Base;

/// <summary>
/// Result of command.
/// </summary>
public class CommandResult : ICommandResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CommandResult"/> class.
    /// </summary>
    public CommandResult()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandResult"/> class.
    /// </summary>
    /// <param name="errorMessage">Error message.</param>
    public CommandResult(string errorMessage)
    {
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// Error message.
    /// </summary>
    public string ErrorMessage { get; } = null!;

    /// <summary>
    /// Whether executed successfully.
    /// </summary>
    public bool IsSuccessful => ErrorMessage == null;
}