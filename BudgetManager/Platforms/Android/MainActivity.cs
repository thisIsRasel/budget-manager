using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using BudgetManager.Platforms.Android;

namespace BudgetManager
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        const int RequestNotificationId = 1000;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
            {
                if (CheckSelfPermission(Manifest.Permission.PostNotifications) != Permission.Granted)
                {
                    RequestPermissions(
                        new[] { Manifest.Permission.PostNotifications },
                        RequestNotificationId);
                }
            }

            CreateNotificationChannel();
            ScheduleDailyNotification();
        }

        void ScheduleDailyNotification()
        {
            var alarmIntent = new Intent(this, typeof(DailyAlarmReceiver));
            alarmIntent.PutExtra("message", "Have you entered your daily costs today?");

            var pendingIntent = PendingIntent.GetBroadcast(
                this,
                0,
                alarmIntent,
                PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

            var alarmManager = (AlarmManager)GetSystemService(AlarmService);

            var calendar = Java.Util.Calendar.Instance;
            calendar.TimeInMillis = Java.Lang.JavaSystem.CurrentTimeMillis();
            calendar.Set(Java.Util.CalendarField.HourOfDay, 22);  // 10 PM
            calendar.Set(Java.Util.CalendarField.Minute, 0);
            calendar.Set(Java.Util.CalendarField.Second, 0);

            // If 10 PM has already passed today, schedule for tomorrow
            if (calendar.TimeInMillis < Java.Lang.JavaSystem.CurrentTimeMillis())
                calendar.Add(Java.Util.CalendarField.DayOfMonth, 1);

            alarmManager.SetExactAndAllowWhileIdle(
                AlarmType.RtcWakeup,
                calendar.TimeInMillis,
                pendingIntent
            );
        }

        void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channel = new NotificationChannel(
                    "daily_channel",
                    "Daily Notifications",
                    NotificationImportance.Default
                )
                {
                    Description = "Channel for daily 10 PM notifications"
                };

                var notificationManager = (NotificationManager)GetSystemService(NotificationService);
                notificationManager.CreateNotificationChannel(channel);
            }
        }

    }
}
