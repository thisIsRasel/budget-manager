using BudgetManager.ViewModels;

namespace BudgetManager.Views;

public partial class NotesPage : ContentPage
{
    private readonly NotesViewModel _viewModel;

	public NotesPage(NotesViewModel viewModel)
	{
		InitializeComponent();
        _viewModel = viewModel;
		BindingContext = _viewModel;
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadNotes();
    }
}
