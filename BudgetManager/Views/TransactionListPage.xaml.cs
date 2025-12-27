using BudgetManager.ViewModels;

namespace BudgetManager.Views;

public partial class TransactionListPage : ContentPage
{
	public TransactionListPage(TransactionViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Shell.SetNavBarIsVisible(this, false);
        //if (BindingContext is TransactionViewModel vm)
        //{
        //    vm.LoadBudgetsAndCostEntries();
        //}
    }
}