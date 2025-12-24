using BudgetManager.ViewModels;

namespace BudgetManager.Views;

public partial class NoteEntryPage : ContentPage
{
	public NoteEntryPage(NoteEntryViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
