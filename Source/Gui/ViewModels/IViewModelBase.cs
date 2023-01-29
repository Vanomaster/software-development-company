using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dal.Entities;
using Prism.Commands;

namespace Gui.ViewModels;

public interface IViewModelBase<TEntity>
    where TEntity : IEntity
{
    // public bool? IsSuccessfulCommandResult { get; set; }

    // public DelegateCommand ResetCommandResultCommand { get; }

    public DelegateCommand ResetNewItemCommand { get; }

    public DelegateCommand RefreshCommand { get; }

    public DelegateCommand AddCommand { get; }

    public DelegateCommand UpdateCommand { get; }

    public DelegateCommand<List<TEntity>> DeleteCommand { get; }

    public TEntity NewItem { get; set; }

    public TEntity SelectedItem { get; set; }

    public ObservableCollection<TEntity> Items { get; }
}