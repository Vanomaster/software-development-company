using System.Windows;
using System.Windows.Controls;
using Queries.Base;

namespace Gui.Views;

public static class DataGridExtensions
{
    public static QueryResult<DataGridColumn> GetColumnByHeaderContent(
        this DataGrid dataGrid,
        string wantedColumnHeaderContent)
    {
        foreach (var column in dataGrid.Columns)
        {
            var columnHeaderContent = column.Header.ToString();
            if (columnHeaderContent == wantedColumnHeaderContent)
            {
                return new QueryResult<DataGridColumn>(column);
            }
        }

        return new QueryResult<DataGridColumn>("Column not found");
    }

    public static void ChangeRowVisible(this DataGrid dataGrid, object item, bool isNeedToShow)
    {
        var itemContainer = dataGrid.ItemContainerGenerator.ContainerFromItem(item);
        if (itemContainer is not DataGridRow row)
        {
            return;
        }

        row.Visibility = isNeedToShow switch
        {
            true => Visibility.Visible,
            false => Visibility.Collapsed,
        };
    }
}