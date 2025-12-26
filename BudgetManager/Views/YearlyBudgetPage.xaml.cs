using BudgetManager.ViewModels;

namespace BudgetManager.Views;

public partial class YearlyBudgetPage : ContentPage
{
	public YearlyBudgetPage(YearlyBudgetViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}
