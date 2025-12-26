using BudgetManager.ViewModels;

namespace BudgetManager.Views;

public partial class CategoryCreationPage : ContentPage
{
	public CategoryCreationPage(CategoryViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}
