using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using Dal.Entities;

namespace Gui.Views;

public class DataGridFilter
{
    public void FilterByPrhase<TEntity>(
        TEntity item,
        DataGrid dataGrid,
        Func<PropertyInfo, bool> predicate,
        string text)
    {
        
    }

    /*public static void Filter<TEntity>(
        TEntity item,
        DataGrid dataGrid,
        Func<PropertyInfo, bool> predicate,
        string text)
    {
        string itemText = GetItemText(predicate, item);
        bool itemIsContainsText = text.Split(" ").All(word => itemText.Split(" ").Any(itemTxt => itemTxt.Contains(word)));
        dataGrid.ChangeRowVisible(item, isNeedToShow: itemIsContainsText);
    }*/

    public static void FilterWithAdditional<TEntity, TAdditionalEntity>(
        TEntity item,
        TAdditionalEntity additionalItem,
        DataGrid dataGrid,
        Func<PropertyInfo, bool> predicate,
        Func<PropertyInfo, bool> additionalPredicate,
        string text)
        where TEntity : IEntity
    {
        string itemText = GetItemText(predicate, item);
        string additionalItemText = GetItemText(additionalPredicate, additionalItem);
        bool itemIsContainsText = itemText.Contains(text);
        bool additionalItemIsContainsText = additionalItemText.Contains(text);
        bool isNeedToShow = itemIsContainsText ? itemIsContainsText : additionalItemIsContainsText;
        dataGrid.ChangeRowVisible(item, isNeedToShow: isNeedToShow);
    }

    public static void FilterWithEnum<TEntity>(
        TEntity item,
        DataGrid dataGrid,
        Func<PropertyInfo, bool> predicate,
        string text)
        where TEntity : IEntity
    {
        string itemText = GetItemTextWithEnum(predicate, item);
        bool itemIsContainsText = text.Split(" ")
            .All(word => itemText.Split(" ").Any(itemWord => itemWord.Contains(word)));

        dataGrid.ChangeRowVisible(item, isNeedToShow: itemIsContainsText);
    }

    public static void FilterWithEnumAndAdditional<TEntity, TAdditionalEntity>(
        TEntity item,
        TAdditionalEntity additionalItem,
        DataGrid dataGrid,
        Func<PropertyInfo, bool> predicate,
        Func<PropertyInfo, bool> additionalPredicate,
        string text)
        where TEntity : IEntity
    {
        string itemText = GetItemTextWithEnum(predicate, item);
        string additionalItemText = GetItemTextWithEnum(additionalPredicate, additionalItem);
        bool itemIsContainsText = text.Split(" ")
            .All(word => (itemText + additionalItemText).Split(" ").Any(itemWord => itemWord.Contains(word)));

        dataGrid.ChangeRowVisible(item, isNeedToShow: itemIsContainsText);
    }

    public static void FilterWithEnumAndAdditional<TEntity, TAdditionalEntity, TAdditionEntity>(
        TEntity item,
        TAdditionalEntity additionalItem,
        TAdditionEntity additionItem,
        DataGrid dataGrid,
        Func<PropertyInfo, bool> predicate,
        Func<PropertyInfo, bool> additionalPredicate,
        Func<PropertyInfo, bool> additionPredicate,
        string text)
        where TEntity : IEntity
    {
        string itemText = GetItemTextWithEnum(predicate, item);
        string additionalItemText = GetItemTextWithEnum(additionalPredicate, additionalItem);
        string additionItemText = GetItemTextWithEnum(additionPredicate, additionItem);
        bool itemIsContainsText = text.Split(" ")
            .All(word => (itemText + additionalItemText + additionItemText).Split(" ")
                .Any(itemWord => itemWord.Contains(word)));

        dataGrid.ChangeRowVisible(item, isNeedToShow: itemIsContainsText);
    }

    private static string GetItemText<TEntity>(Func<PropertyInfo, bool> predicate, TEntity item)
    {
        string itemText = typeof(TEntity).GetProperties()
            .Where(predicate) // property => property.GetType().IsPrimitive
            .Select(property => property.GetValue(item)?.ToString())
            .Aggregate(string.Empty, (current, next) => current + " " + next)
            .ToLower();

        return itemText;
    }

    private static string GetItemTextWithEnum<TEntity>(Func<PropertyInfo, bool> predicate, TEntity item)
    {
        if (item is null)
        {
            return string.Empty;
        }

        var itemValues = typeof(TEntity).GetProperties()
            .Where(predicate)
            .Select(property => property.GetValue(item))
            .ToList();

        var description = string.Empty;
        foreach (object? value in itemValues)
        {
            if (value == null)
            {
                continue;
            }

            var underlyingType = Nullable.GetUnderlyingType(value.GetType()) ?? value.GetType();
            if (!underlyingType.IsEnum)
            {
                continue;
            }

            description = value.GetType()
                .GetField(value.ToString())?
                .GetCustomAttributes(typeof(DescriptionAttribute), false)
                .FirstOrDefault() is DescriptionAttribute descriptionAttribute
                ? descriptionAttribute.Description
                : value.ToString();
        }

        description ??= string.Empty;
        string itemText = itemValues
            .Select(property => property?.ToString())
            .Aggregate(description, (current, next) => current + " " + next)
            .ToLower();

        return itemText;
    }
}