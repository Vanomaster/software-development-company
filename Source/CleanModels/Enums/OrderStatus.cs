using System.ComponentModel;

namespace CleanModels;

public enum OrderStatus : byte
{
    [Description("Не взят в работу")]
    NotAccepted = 1,

    [Description("Разработка ТЗ")]
    StatementOfWorkDevelopment,

    [Description("Проектирование")]
    Designing,

    [Description("Кодирование")]
    Coding,

    [Description("Тестирование")]
    Testing,

    [Description("Внедрение")]
    Integration,

    [Description("Выполнено")]
    Done,
}