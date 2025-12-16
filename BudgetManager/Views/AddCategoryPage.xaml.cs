using BudgetManager.ViewModels;

namespace BudgetManager.Views;

public partial class AddCategoryPage : ContentPage
{
	public AddCategoryPage(CategoryViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}
