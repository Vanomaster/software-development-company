using System.ComponentModel;

namespace CleanModels;

public enum Gender : byte
{
    [Description("Мужской")]
    Male = 1,

    [Description("Женский")]
    Female,
}