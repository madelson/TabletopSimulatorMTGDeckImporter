using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TabletopMtgImporter.Wpf
{
    // follows pattern from https://codemag.com/article/1505101
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public MainWindowViewModel()
        {
            this.SelectFileCommand = new DelegateCommand(_ => this.SelectFile());
        }

        public ICommand SelectFileCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string name = "") => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

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

        private void SelectFile()
        {
            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                this.SelectedFile = openFileDialog.FileName;
            }
        }
    }
}
