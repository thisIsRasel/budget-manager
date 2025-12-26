namespace BudgetManager
{
    public partial class AppShell : Shell
    {
        private DateTime _lastBackPressed;

        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(Views.CategoryCreationPage), typeof(Views.CategoryCreationPage));
            Routing.RegisterRoute(nameof(Views.CategoryUpdatePage), typeof(Views.CategoryUpdatePage));
            Routing.RegisterRoute(nameof(Views.TransactionCreationPage), typeof(Views.TransactionCreationPage));
            Routing.RegisterRoute(nameof(Views.TransactionUpdatePage), typeof(Views.TransactionUpdatePage));
            Routing.RegisterRoute(nameof(Views.BudgetListPage), typeof(Views.BudgetListPage));
            Routing.RegisterRoute(nameof(Views.BudgetCreationPage), typeof(Views.BudgetCreationPage));
            Routing.RegisterRoute(nameof(Views.BudgetDetailsPage), typeof(Views.BudgetDetailsPage));
            Routing.RegisterRoute(nameof(Views.BudgetUpdatePage), typeof(Views.BudgetUpdatePage));
            Routing.RegisterRoute(nameof(Views.YearlyBudgetPage), typeof(Views.YearlyBudgetPage));
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
