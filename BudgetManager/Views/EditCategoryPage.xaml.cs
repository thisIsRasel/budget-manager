using BudgetManager.ViewModels;

namespace BudgetManager.Views;

public partial class EditCategoryPage : ContentPage
{
	public EditCategoryPage(CategoryViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}
