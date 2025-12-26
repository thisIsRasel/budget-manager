using BudgetManager.ViewModels;

namespace BudgetManager.Views
{
    public partial class BudgetDetailsPage : ContentPage
    {
        public BudgetDetailsPage(BudgetDetailsViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Shell.SetNavBarIsVisible(this, false);
        }
    }
}