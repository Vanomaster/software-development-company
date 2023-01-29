using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Commands.Base;
using Dal.Entities;

namespace Gui.Views;

public class DataValidator
{
    private readonly Regex loginNormalizer = new (
        pattern: "^[\\wа-яА-ЯёЁ]+$",
        options: RegexOptions.Compiled | RegexOptions.Singleline);

    private readonly Regex nameNormalizer = new (
        pattern: "^[a-zA-Zа-яА-ЯёЁ]+$",
        options: RegexOptions.Compiled | RegexOptions.Singleline);

    private readonly Regex emailNormalizer = new (
        pattern: "^[a-zA-Z0-9.!#$%&’*+/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\\.[a-zA-Z0-9-]+)*$",
        options: RegexOptions.Compiled | RegexOptions.Singleline);

    public CommandResult IsValidLogin(string? login)
    {
        if (IsNullOrWhiteSpace(login))
        {
            return new CommandResult("Не заполнен логин");
        }

        if (login.Length > 200)
        {
            return new CommandResult("Слишком длинный логин");
        }

        var match = loginNormalizer.Match(login);
        if (!match.Success)
        {
            return new CommandResult("Недопустимый логин");
        }

        return new CommandResult();
    }

    public CommandResult IsValidFirstName(string? name)
    {
        if (IsNullOrWhiteSpace(name))
        {
            return new CommandResult("Не заполнено имя");
        }

        if (name.Length > 100)
        {
            return new CommandResult("Слишком длинное имя");
        }

        var match = nameNormalizer.Match(name);
        if (!match.Success)
        {
            return new CommandResult("Недопустимое имя");
        }

        return new CommandResult();
    }

    public CommandResult IsValidLastName(string? name)
    {
        if (IsNullOrWhiteSpace(name))
        {
            return new CommandResult("Не заполнена фамилия");
        }

        if (name.Length > 100)
        {
            return new CommandResult("Слишком длинная фамилия");
        }

        var match = nameNormalizer.Match(name);
        if (!match.Success)
        {
            return new CommandResult("Недопустимая фамилия");
        }

        return new CommandResult();
    }

    public CommandResult IsValidPatronymic(string? patronymic)
    {
        if (IsNullOrWhiteSpace(patronymic))
        {
            return new CommandResult();
        }

        if (patronymic.Length > 100)
        {
            return new CommandResult("Слишком длинное отчество");
        }

        var match = nameNormalizer.Match(patronymic);
        if (!match.Success)
        {
            return new CommandResult("Недопустимое отчество");
        }

        return new CommandResult();
    }

    public CommandResult IsValidEmail(string? email)
    {
        if (IsNullOrWhiteSpace(email))
        {
            return new CommandResult("Не заполнена электронная почта");
        }

        if (email.Length > 200)
        {
            return new CommandResult("Слишком длинная электронная почта");
        }

        var match = emailNormalizer.Match(email);
        if (!match.Success)
        {
            return new CommandResult("Недопустимая электронная почта");
        }

        return new CommandResult();
    }

    public CommandResult IsValidTitle(string? title, string titleOwner)
    {
        if (IsNullOrWhiteSpace(title))
        {
            return new CommandResult("Не заполнен заголовок " + titleOwner);
        }

        if (title.Length > 200)
        {
            return new CommandResult("Слишком длинный заголовок " + titleOwner);
        }

        return new CommandResult();
    }

    public CommandResult IsValidText(string? text, string titleOwner)
    {
        if (IsNullOrWhiteSpace(text))
        {
            return new CommandResult("Не заполнен текст " + titleOwner);
        }

        return new CommandResult();
    }

    public CommandResult IsValidCost(int? cost)
    {
        if (cost == null)
        {
            return new CommandResult();
        }

        if (cost < 0)
        {
            return new CommandResult("Недопустимая стоимость");
        }

        return new CommandResult();
    }

    public CommandResult IsValidPassportSeries(int? series)
    {
        if (series == null)
        {
            return new CommandResult("Не заполнена серия паспорта");
        }

        if (series < 1000)
        {
            return new CommandResult("Серия паспорта содержит менее 4-х цифр");
        }

        if (series > 9999)
        {
            return new CommandResult("Серия паспорта содержит более 4-х цифр");
        }

        return new CommandResult();
    }

    public CommandResult IsValidPassportNumber(int? number)
    {
        if (number == null)
        {
            return new CommandResult("Не заполнен номер паспорта");
        }

        if (number < 100000)
        {
            return new CommandResult("Номер паспорта содержит менее 6-ти цифр");
        }

        if (number > 999999)
        {
            return new CommandResult("Номер паспорта содержит более 6-ти цифр");
        }

        return new CommandResult();
    }

    public CommandResult IsValidBirthDate(DateTime? date)
    {
        if (date == null)
        {
            return new CommandResult("Не заполнена дата рождения");
        }

        if (date > DateTime.Today)
        {
            return new CommandResult("Некорректная дата рождения");
        }

        return new CommandResult();
    }

    public CommandResult IsValidResidence(string? residence)
    {
        if (IsNullOrWhiteSpace(residence))
        {
            return new CommandResult("Не заполнено место жительства");
        }

        if (residence.Length > 250)
        {
            return new CommandResult("Слишком длинное место жительства");
        }

        return new CommandResult();
    }

    private static bool IsNullOrWhiteSpace(string? text)
    {
        return string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(text?.Trim());
    }
}