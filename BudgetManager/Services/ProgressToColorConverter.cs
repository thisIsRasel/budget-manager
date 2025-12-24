using System.Globalization;

namespace BudgetManager.Services
{
    public class ProgressToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double progress)
            {
                if (progress >= 1)
                {
                    return Colors.Red;
                }
                else if (progress >= 0.75)
                {
                    return Color.FromArgb("#ffa8a8");
                }
            }

            return Color.FromArgb("#00c0ff");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}


