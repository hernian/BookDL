using BookDL.Domain;
using BookDL.Infrastructure;
using BookDL.Presentation;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Markup;

namespace BookDL.ViewModels
{
    public record OutputDataKindItem(string Name, OutputDataKind Kind)
    {
        public override string ToString() => Name;
    }


    public partial class ConfigViewModel : ObservableObject
    {
        private static readonly OutputDataKindItem[] OUTPUT_DATA_KIND_ITEMS = [
            new OutputDataKindItem("EPub", OutputDataKind.EPub),
            new OutputDataKindItem("Single-Html", OutputDataKind.SingleHtml)
            ];


        public event EventHandler<DialogResultEventArgs>? CloseDialogRequired;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(OKCommand))]
        private string outputDirectory;

        [ObservableProperty]
        private OutputDataKindItem? selectedItem;

        [ObservableProperty]
        private ObservableCollection<OutputDataKindItem> items = new ObservableCollection<OutputDataKindItem>(OUTPUT_DATA_KIND_ITEMS);

        private ISettingsService _settingsService;

        public ConfigViewModel(ISettingsService settingsService)
        {
            _settingsService = settingsService;
            OutputDirectory = _settingsService.OutputDirectory;
            SelectedItem = this.items.FirstOrDefault(i => i.Kind == _settingsService.OutputDataKind);
        }

        [RelayCommand(CanExecute = nameof(CanOK))]
        private void OK()
        {
            _settingsService.OutputDataKind = SelectedItem?.Kind ?? throw new InvalidOperationException("No outputDataKind selected.");
            _settingsService.OutputDirectory = this.OutputDirectory;
            _settingsService.Save();
            this.CloseDialogRequired?.Invoke(this, new DialogResultEventArgs(dialogResult: true));
        }

        private bool CanOK()
        {
            return !string.IsNullOrWhiteSpace(this.OutputDirectory);
        }

        [RelayCommand]
        private void Cancel()
        {
            this.CloseDialogRequired?.Invoke(this, new DialogResultEventArgs(dialogResult: false));
        }
    }
}
