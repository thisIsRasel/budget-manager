using BudgetManager.ViewModels;

namespace BudgetManager.Views;

public partial class TransactionUpdatePage : ContentPage
{
    public TransactionUpdatePage(TransactionEntryViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
