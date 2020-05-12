using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace TabletopMtgImporter.Wpf
{
    // follows pattern from https://codemag.com/article/1505101
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public MainWindowViewModel()
        {
            this.SelectFileCommand = new DelegateCommand(_ => this.SelectFile());
            this.ImportCommand = new DelegateCommand(_ => this.Import(), _ => this.CanImport(), this);
        }

        public ICommand SelectFileCommand { get; }
        public ICommand ImportCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string name = "") => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        #region ---- Binding Properties ----
        private string? _selectedFile;

        public string? SelectedFile
        {
            get => this._selectedFile;
            set
            {
                if (value != this._selectedFile)
                {
                    this._selectedFile = value;
                    this.OnPropertyChanged();
                    this.OnPropertyChanged(nameof(this.SelectedFileDisplay));
                }
            }
        }

        public string SelectedFileDisplay => this.SelectedFile ?? "no file selected";

        private int _selectedTabIndex;

        public int SelectedTabIndex
        {
            get => this._selectedTabIndex;
            set
            {
                if (value != this._selectedTabIndex)
                {
                    this._selectedTabIndex = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _pastedDeckName = string.Empty;

        public string PastedDeckName
        {
            get => this._pastedDeckName;
            set
            {
                if (value != this._pastedDeckName)
                {
                    this._pastedDeckName = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _pastedText = string.Empty;

        public string PastedText
        {
            get => this._pastedText;
            set
            {
                if (value != this._pastedText)
                {
                    this._pastedText = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _outputPaneText = string.Empty;

        public string OutputPaneText
        {
            get => this._outputPaneText;
            set
            {
                if (value != this._outputPaneText)
                {
                    this._outputPaneText = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private bool _isImporting;

        public bool IsImporting
        {
            get => this._isImporting;
            set
            {
                if (value != this._isImporting)
                {
                    this._isImporting = value;
                    this.OnPropertyChanged();
                }
            }
        }
        #endregion

        private void SelectFile()
        {
            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                this.SelectedFile = openFileDialog.FileName;
            }
        }

        private void Import()
        {
            if (!this.TryGetDeckInput(out var deckInput)) { return; }

            this.OutputPaneText = string.Empty;
            var logger = new WpfLogger(s => Application.Current.Dispatcher.Invoke(() => this.OutputPaneText += s + Environment.NewLine));
            var importer = new Importer(logger, new Configuration());
            this.IsImporting = true;
            var importTask = Task.Run(() => importer.TryImportAsync(deckInput!));
            importTask.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    logger.Error($"Import failed with unhandled exception");
                    logger.Debug($"Unhandled import exception: {t.Exception}");
                }

                Application.Current.Dispatcher.Invoke(() => {
                    this.IsImporting = false;
                    
                    if (logger.ErrorCount > 0)
                    {
                        MessageBox.Show(
                            $"Failed to import '{deckInput!.Name}'. See output pane for details",
                            "Import finished",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error
                        );
                    }
                    else if (logger.WarningCount > 0)
                    {
                        MessageBox.Show(
                            $"Import of '{deckInput!.Name}' completed, but with warnings. See output pane for details",
                            "Import finished",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning
                        );
                    }
                    else
                    {
                        MessageBox.Show($"Successfully imported '{deckInput!.Name}'!", "Import finished", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                });
            });
        }

        private bool CanImport() => this.TryGetDeckInput(out _);

        private bool TryGetDeckInput(out IDeckInput? deckInput)
        {
            if (!this.IsImporting)
            {
                switch (this.SelectedTabIndex)
                {
                    case 0:
                        if (!string.IsNullOrWhiteSpace(this.PastedDeckName)
                            && !string.IsNullOrWhiteSpace(this.PastedText))
                        {
                            deckInput = new StringDeckInput(this.PastedDeckName, this.PastedText);
                            return true;
                        }
                        break;
                    case 1:
                        if (!string.IsNullOrEmpty(this.SelectedFile))
                        {
                            deckInput = new DeckFileInput(this.SelectedFile!);
                            return true;
                        }
                        break;
                    default:
                        break;
                }
            }

            deckInput = null;
            return false;
        }
    }
}
