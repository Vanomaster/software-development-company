using System;
using Commands.Base;
using Cryptographer = BCrypt.Net.BCrypt;

namespace AuthorizationService;

public class HashMachine
{
    private const string LocalSalt = @"JVjjf38u$tgj%Fj^8j+OJ#*Ev";

    public string GenerateSalt()
    {
        string salt = Cryptographer.GenerateSalt();

        return salt;
    }

    public CommandResult<string> VerifyAndReplacePassword(
        string password,
        string salt,
        string passwordHash,
        string newPassword)
    {
        var verifyingPasswordResult = VerifyPassword(password, salt, passwordHash);
        if (!verifyingPasswordResult.IsSuccessful)
        {
            return new CommandResult<string>(errorMessage: verifyingPasswordResult.ErrorMessage);
        }

        bool isVerifiedPassword = verifyingPasswordResult.Data;
        if (!isVerifiedPassword)
        {
            return new CommandResult<string>(errorMessage: @"Неверный пароль");
        }

        var passwordHashGeneratingResult = GeneratePasswordHash(newPassword, salt);
        if (!passwordHashGeneratingResult.IsSuccessful)
        {
            return new CommandResult<string>(errorMessage: passwordHashGeneratingResult.ErrorMessage);
        }

        string newPasswordHash = passwordHashGeneratingResult.Data;

        return new CommandResult<string>(data: newPasswordHash);
    }

    public CommandResult<bool> VerifyPassword(string password, string salt, string passwordHash)
    {
        var passwordHashGeneratingResult = GeneratePasswordHash(password, salt);
        if (!passwordHashGeneratingResult.IsSuccessful)
        {
            return new CommandResult<bool>(errorMessage: passwordHashGeneratingResult.ErrorMessage);
        }

        string newPasswordHash = passwordHashGeneratingResult.Data;
        bool isVerifiedPassword = newPasswordHash == passwordHash;

        return new CommandResult<bool>(data: isVerifiedPassword);
    }

    public CommandResult<string> GeneratePasswordHash(string password, string salt)
    {
        string salts = salt + LocalSalt;
        string passwordHash;
        try
        {
            passwordHash = Cryptographer.HashPassword(password, salts);
        }
        catch (Exception exception)
        {
            return new CommandResult<string>(errorMessage: exception.ToString());
        }

        return new CommandResult<string>(data: passwordHash);
    }
}