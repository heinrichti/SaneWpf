using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Windows.Input;
using SaneWpf.Attributes;
using SaneWpf.ExtensionMethods;
using SaneWpf.Framework;

namespace SaneWpfSample
{
    [ViewModel]
    public partial class MainWindowViewModel
    {
        [Required]
        [MinLength(4)]
        [AutoNotify(nameof(_numberTest))]
        private string _test;

        [AutoNotify]
        private int _numberTest;

        public MainWindowViewModel()
        {
            Validations.Add(this, x => x._test, s => s == "nobody", x => Validation.Error("name should not be nobody"));
            Validations.Add(this, x => x._test, s => s == "nobody", x => Validation.Error("Name should not be nobody"));
            Validations.Add(this, x => x._test, s => s == "Tim", x => Validation.Error("Nööö"));
            Validations.Add(this, x => x._test, s => s.Length < 4, x => Validation.Error("Muss länger als 3 sein"));
            Validations.Add(this, x => x._numberTest, i => i < 25 || i > 50, x => Validation.Error("NumberTest has to be between 25 and 50"));

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

        public ICommand AsyncCommand => new AsyncCommand(_ => Task.Delay(500));

        public ICommand InitializeCommand { get; }
    }
}
