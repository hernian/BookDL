using BookDL.Domain;
using System.Diagnostics;
using System.Text.Json;

namespace BookDL.Infrastructure
{
    public record CurrentState(BookInfo BookInfo, string OutputDirectory);

    public interface ISettingsService
    {
        string OutputDirectory { get; set; }
        OutputDataKind OutputDataKind { get; set; }

        CurrentState CurrentState { get; set; }
        void Save();
    }

    public class SettingsService : ISettingsService
    {
        public string OutputDirectory
        {
            get => Properties.Settings.Default.DefaultOutputDirectory;
            set => Properties.Settings.Default.DefaultOutputDirectory = value;
        }

        public OutputDataKind OutputDataKind
        {
            get => (OutputDataKind)Properties.Settings.Default.OutputDataKind;
            set => Properties.Settings.Default.OutputDataKind = (int)value;
        }

        public CurrentState CurrentState
        {
            get => LoadCurrentState();
            set => SetCurrentState(value);
        }
        public void Save()
        {
            Properties.Settings.Default.Save();
        }
        private CurrentState LoadCurrentState()
        {
            try
            {
                var jsonText = Properties.Settings.Default.CurrentState;
                var currentState = JsonSerializer.Deserialize<CurrentState>(jsonText);
                if (currentState != null)
                {
                    return currentState;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return new CurrentState(
                BookInfo: new BookInfo(
                    BookUrl: string.Empty,
                    Title: string.Empty,
                    TitleKatakana: string.Empty,
                    Author: string.Empty,
                    AuthorKatakana: string.Empty),
                OutputDirectory: Properties.Settings.Default.DefaultOutputDirectory);
        }

        void SetCurrentState(CurrentState currentState)
        {
            var jsonText = JsonSerializer.Serialize<CurrentState>(currentState);
            Properties.Settings.Default.CurrentState = jsonText;
        }

    }
}
