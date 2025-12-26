using BudgetManager.ViewModels;

namespace BudgetManager.Views;

public partial class BudgetUpdatePage : ContentPage
{
	public BudgetUpdatePage(BudgetUpdateViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
    }
}