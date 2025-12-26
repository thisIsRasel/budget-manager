using BudgetManager.ViewModels;

namespace BudgetManager.Views;

public partial class TransactionCreationPage : ContentPage
{
	public TransactionCreationPage(TransactionViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}
