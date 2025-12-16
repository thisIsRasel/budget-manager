using BudgetManager.ViewModels;

namespace BudgetManager.Views;

public partial class EditTransactionPage : ContentPage
{
	public EditTransactionPage(EditTransactionViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}
