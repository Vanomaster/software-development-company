namespace Commands.Base;

/// <summary>
/// Command result.
/// </summary>
/// <typeparam name="TResult">Type of command result.</typeparam>
public interface ICommandResult<out TResult>
{
    /// <summary>
    /// Data.
    /// </summary>
    public TResult Data { get; }

    /// <summary>
    /// Error message.
    /// </summary>
    public string ErrorMessage { get; }

    /// <summary>
    /// Whether executed successfully.
    /// </summary>
    public bool IsSuccessful { get; }
}