using BudgetManager.ViewModels;

namespace BudgetManager.Views;

public partial class CategoryUpdatePage : ContentPage
{
	public CategoryUpdatePage(CategoryViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}
