using BudgetManager.ViewModels;

namespace BudgetManager.Views;

public partial class ReportPage : ContentPage
{
    public ReportPage(ReportViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Shell.SetNavBarIsVisible(this, false);
        if (BindingContext is ReportViewModel vm)
        {
            vm.LoadTotals();
        }
    }
}