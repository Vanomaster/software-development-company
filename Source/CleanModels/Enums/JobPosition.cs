using System.ComponentModel;

namespace CleanModels;

public enum JobPosition : byte
{
    [Description("Администратор")]
    Administrator,

    [Description("Менеджер проекта")]
    ProjectManager = 1,

    [Description("Руководитель команды")]
    TeamLeader,

    [Description("Продуктовый аналитик")]
    ProductAnalyst,

    [Description("Системный аналитик")]
    SystemAnalyst,

    [Description("UX-дизайнер")]
    UxDesigner,

    [Description("UI-дизайнер")]
    UiDesigner,

    [Description("DevOps-инженер")]
    DevOpsEngineer,

    [Description("Инженер-программист")]
    SoftwareEngineer,

    [Description("Программист")]
    Programmer,

    [Description("Тестировщик")]
    SoftwareTester,

    [Description("Разработчик БД")]
    DbDesigner,
}