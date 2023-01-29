using System.ComponentModel;

namespace CleanModels;

public enum StatementOfWorkStatus : byte
{
    [Description("Не готово")]
    NotDone = 1,

    [Description("Готово")]
    Done,
}