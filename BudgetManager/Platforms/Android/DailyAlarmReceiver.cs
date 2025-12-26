using Android.App;
using Android.Content;
using AndroidX.Core.App;

namespace BudgetManager.Platforms.Android;

[BroadcastReceiver(Enabled = true, Exported = true)]
public class DailyAlarmReceiver : BroadcastReceiver
{
    public override void OnReceive(Context context, Intent intent)
    {
        string message = intent.GetStringExtra("message") ?? "Have you entered your daily costs today?";

        var notificationManager = NotificationManagerCompat.From(context);

        var notification = new NotificationCompat.Builder(context, "daily_channel")
            .SetContentText(message)
            .SetSmallIcon(Resource.Drawable.ic_notification)
            .SetAutoCancel(true)
            .Build();

        notificationManager.Notify(1001, notification);

        // Reschedule the next alarm for tomorrow
        ScheduleNextAlarm(context);
    }

    private void ScheduleNextAlarm(Context context)
    {
        var alarmIntent = new Intent(context, typeof(DailyAlarmReceiver));
        alarmIntent.PutExtra("message", "Have you entered your daily costs today?");

        var pendingIntent = PendingIntent.GetBroadcast(
            context, 0, alarmIntent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable
        );

        var alarmManager = (AlarmManager)context.GetSystemService(Context.AlarmService);

        var calendar = Java.Util.Calendar.Instance;
        calendar.TimeInMillis = Java.Lang.JavaSystem.CurrentTimeMillis();
        calendar.Set(Java.Util.CalendarField.HourOfDay, 22);  // 10 PM
        calendar.Set(Java.Util.CalendarField.Minute, 0);
        calendar.Set(Java.Util.CalendarField.Second, 0);

        // If time has already passed today, schedule for tomorrow
        if (calendar.TimeInMillis < Java.Lang.JavaSystem.CurrentTimeMillis())
            calendar.Add(Java.Util.CalendarField.DayOfMonth, 1);

        alarmManager.SetExactAndAllowWhileIdle(
            AlarmType.RtcWakeup,
            calendar.TimeInMillis,
            pendingIntent
        );
    }
}
