using BudgetManager.ViewModels;

namespace BudgetManager.Views;

public partial class AddTransactionPage : ContentPage
{
	public AddTransactionPage(TransactionViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}
