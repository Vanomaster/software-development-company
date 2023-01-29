namespace Commands.Base;

/// <summary>
/// Result of command.
/// </summary>
/// <typeparam name="TResult">Type of command result.</typeparam>
public class CommandResult<TResult> : ICommandResult<TResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CommandResult{TResult}"/> class.
    /// </summary>
    /// <param name="data">Data.</param>
    public CommandResult(TResult data)
    {
        Data = data;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandResult"/> class.
    /// </summary>
    /// <param name="errorMessage">Error message.</param>
    public CommandResult(string errorMessage)
    {
        ErrorMessage = errorMessage;
    }

    /// <inheritdoc/>
    public TResult Data { get; } = default!;

    /// <inheritdoc/>
    public string ErrorMessage { get; } = null!;

    /// <inheritdoc/>
    public bool IsSuccessful => ErrorMessage == null;
}