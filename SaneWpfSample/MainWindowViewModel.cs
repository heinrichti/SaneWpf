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
            AddValidation(() => Test, s => s == "nobody", Validation.Error("Name should not be nobody"));
            AddValidation(() => Test, s => s == "Tim", Validation.Error("Nööö"));
            AddValidation(() => Test, s => s.Length < 4, Validation.Error("Muss länger als 3 sein"));
            AddValidation(() => NumberTest, i => i < 25 || i > 50, Validation.Error("NumberTest has to be between 25 and 50"));

            InitializeCommand = new AsyncCommand(async _ =>
            {
                NumberTest = 25;
                await Task.CompletedTask;
            });
        }

        public ICommand OpenDialogCommand => new Command<object>(_ =>
        {
            var myDialog = new TodoDialog();
            myDialog.ViewModel<TodoDialogViewModel>().TodoId = new Random().Next(1, 200);
            myDialog.ShowDialog();
        });

        [Required]
        //[MinLength(4)]
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
