using BudgetManager.ViewModels;

namespace BudgetManager.Views;

public partial class TransactionUpdatePage : ContentPage
{
	public TransactionUpdatePage(TransactionUpdateViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}
