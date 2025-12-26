namespace BudgetManager
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(Views.AddCategoryPage), typeof(Views.AddCategoryPage));
            Routing.RegisterRoute(nameof(Views.EditCategoryPage), typeof(Views.EditCategoryPage));
            Routing.RegisterRoute(nameof(Views.AddTransactionPage), typeof(Views.AddTransactionPage));
            Routing.RegisterRoute(nameof(Views.EditTransactionPage), typeof(Views.EditTransactionPage));
            Routing.RegisterRoute(nameof(Views.BudgetListPage), typeof(Views.BudgetListPage));
            Routing.RegisterRoute(nameof(Views.AddBudgetPage), typeof(Views.AddBudgetPage));
            Routing.RegisterRoute(nameof(Views.BudgetDetailsPage), typeof(Views.BudgetDetailsPage));
            Routing.RegisterRoute(nameof(Views.BudgetUpdatePage), typeof(Views.BudgetUpdatePage));
            Routing.RegisterRoute(nameof(Views.YearlyBudgetPage), typeof(Views.YearlyBudgetPage));
            Routing.RegisterRoute(nameof(Views.NotesPage), typeof(Views.NotesPage));
            Routing.RegisterRoute(nameof(Views.NoteEntryPage), typeof(Views.NoteEntryPage));
        }
    }
}
