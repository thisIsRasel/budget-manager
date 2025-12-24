using BudgetManager.Models;
using BudgetManager.Services;
using BudgetManager.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace BudgetManager.ViewModels
{
    public class NotesViewModel : BaseViewModel
    {
        private readonly SQLiteService _sqlite;

        public ObservableCollection<Note> Notes { get; } = new();

        public ICommand AddNoteCommand => new Command(async () =>
        {
            await Shell.Current.GoToAsync(nameof(NoteEntryPage));
        });

        public ICommand EditNoteCommand => new Command<Note>(async (note) =>
        {
            if (note == null) return;
            var navParam = new Dictionary<string, object>
            {
                { "Note", note }
            };
            await Shell.Current.GoToAsync(nameof(NoteEntryPage), navParam);
        });

        public ICommand DeleteNoteCommand => new Command<Note>(async (note) =>
        {
            if (note == null) return;
            bool answer = await Shell.Current.DisplayAlert("Delete Note", "Are you sure you want to delete this note?", "Yes", "No");
            if (answer)
            {
                await _sqlite.DeleteNoteAsync(note);
                LoadNotes();
            }
        });

        public NotesViewModel(SQLiteService sqlite)
        {
            _sqlite = sqlite;
        }

        public async void LoadNotes()
        {
            var notes = await _sqlite.GetNotesAsync();
            Notes.Clear();
            foreach (var note in notes)
            {
                Notes.Add(note);
            }
        }
    }
}
