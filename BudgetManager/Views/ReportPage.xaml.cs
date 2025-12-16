using BudgetManager.ViewModels;

namespace BudgetManager.Views;

public partial class ReportPage : ContentPage
{
    public ReportPage(ReportViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}