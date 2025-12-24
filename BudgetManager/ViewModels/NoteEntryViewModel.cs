using BudgetManager.Models;
using BudgetManager.Services;
using System.Windows.Input;

namespace BudgetManager.ViewModels
{
    [QueryProperty(nameof(Note), "Note")]
    public class NoteEntryViewModel : BaseViewModel
    {
        private readonly SQLiteService _sqlite;

        private Note _note;
        public Note Note
        {
            get => _note;
            set
            {
                _note = value;
                if (_note != null)
                {
                    Title = _note.Title;
                    Text = _note.Text;
                    Date = _note.Date;
                }
                OnPropertyChanged(nameof(Note));
                OnPropertyChanged(nameof(IsCreateMode));
                OnPropertyChanged(nameof(IsEditMode));
            }
        }

        private string _title;
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        private string _text;
        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                OnPropertyChanged(nameof(Text));
            }
        }

        private DateTime _date;
        public DateTime Date
        {
            get => _date;
            set
            {
                _date = value;
                OnPropertyChanged(nameof(Date));
            }
        }

        public bool IsCreateMode => Note == null;
        public bool IsEditMode => Note != null;

        public ICommand SaveCommand => new Command(async () =>
        {
            if (string.IsNullOrWhiteSpace(Title) && string.IsNullOrWhiteSpace(Text))
            {
                await Shell.Current.DisplayAlert("Error", "Please enter a title or text for the note.", "OK");
                return;
            }

            if (Note != null)
            {
                // Update
                Note.Title = Title;
                Note.Text = Text;
                Note.Date = Date;
                await _sqlite.UpdateNoteAsync(Note);
            }
            else
            {
                // Create
                var newNote = new Note
                {
                    Title = Title,
                    Text = Text,
                    Date = Date
                };
                await _sqlite.SaveNoteAsync(newNote);
            }

            await Shell.Current.GoToAsync("..");
        });

        public ICommand DeleteCommand => new Command(async () =>
        {
            await _sqlite.DeleteNoteAsync(Note);
            await Shell.Current.GoToAsync("..");
        });

        public NoteEntryViewModel(SQLiteService sqlite)
        {
            _sqlite = sqlite;
            Date = DateTime.Now;
        }
    }
}
