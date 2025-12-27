using BudgetManager.ViewModels;

namespace BudgetManager.Views;

public partial class TransactionCreationPage : ContentPage
{
	public TransactionCreationPage(TransactionEntryViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}
