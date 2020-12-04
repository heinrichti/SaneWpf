using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Windows.Input;
using SaneWpf.ExtensionMethods;
using SaneWpf.Framework;

namespace SaneWpfSample
{
    class MainWindowViewModel : ViewModelBase
    {
        private string _test;
        private int _numberTest;

        public MainWindowViewModel()
        {
            AddValidation(() => Test, s => s == "nobody", Validation.Warning("Name should not be nobody"));
            AddValidation(() => NumberTest, i => i < 25 || i > 50, Validation.Error("NumberTest has to be between 25 and 50"));

            InitializeCommand = new AsyncCommand(async _ =>
            {
                //await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
                NumberTest = 25;
                await Task.CompletedTask;
            });
        }

        public ICommand OpenDialogCommand => new Command<object>(_ =>
        {
            var myDialog = new MyDialog();
            myDialog.ViewModel<MyDialogViewModel>().Name = "Test User";
            myDialog.ShowDialog();
        });

        [Required]
        [MinLength(4)]
        public string Test
        {
            get => _test;
            set => Set(ref _test, value);
        }

        public int NumberTest
        {
            get => _numberTest;
            set => Set(ref _numberTest, value);
        }


        public ICommand AsyncCommand => new AsyncCommand(_ => Task.Delay(500));

        public ICommand InitializeCommand { get; }
    }
}
