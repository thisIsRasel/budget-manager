namespace BudgetManager
{
    public partial class AppShell : Shell
    {
        private DateTime _lastBackPressed;

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

        protected override bool OnBackButtonPressed()
        {
            // If there is navigation history, go back normally
            if (Navigation.NavigationStack.Count > 1)
            {
                return base.OnBackButtonPressed();
            }

            var now = DateTime.UtcNow;

            if ((now - _lastBackPressed).TotalSeconds < 2)
            {
                // Allow app to close
                return base.OnBackButtonPressed();
            }

            _lastBackPressed = now;

            #if ANDROID
            Android.Widget.Toast.MakeText(
                Android.App.Application.Context,
                "Press back again to exit",
                Android.Widget.ToastLength.Short).Show();
            #endif

            // Block single back press
            return true;
        }
    }
}
