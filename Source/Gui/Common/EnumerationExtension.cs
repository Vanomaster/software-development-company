using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Markup;
using CleanModels;

namespace Gui.Views;

public class EnumerationExtension : MarkupExtension
{
    private Type enumType;

    public EnumerationExtension(Type enumType)
    {
        EnumType = enumType ?? throw new ArgumentNullException(nameof(enumType));
    }

    public Type EnumType
    {
        get => enumType;

        private set
        {
            if (enumType == value)
            {
                return;
            }

            var underlyingType = Nullable.GetUnderlyingType(value) ?? value;
            if (underlyingType.IsEnum == false)
            {
                throw new ArgumentException("Type must be an Enum.");
            }

            enumType = value;
        }
    }

    public override object ProvideValue(IServiceProvider serviceProvider) // or IXamlServiceProvider for UWP and WinUI
    {
        var enumValues = Enum.GetValues(EnumType);

        return enumValues.Cast<object>()
            .Select(enumValue => new EnumerationMember
            {
                Value = enumValue,
                Description = GetDescription(enumValue),
            })
            .ToArray();
    }

    private string GetDescription(object enumValue)
    {
        return EnumType
            .GetField(enumValue.ToString())
            .GetCustomAttributes(typeof (DescriptionAttribute), false)
            .FirstOrDefault() is DescriptionAttribute descriptionAttribute
            ? descriptionAttribute.Description
            : enumValue.ToString();
    }
}