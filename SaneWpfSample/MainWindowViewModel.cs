using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Windows.Input;
using SaneWpf.Framework;

namespace SaneWpfSample
{
    class MainWindowViewModel : ViewModelBase
    {
        private string _test;

        public MainWindowViewModel()
        {
            this.AddValidation(nameof(Test), (string t) => t == "nobody" 
                ? new ValidationIssue("Name should not be nobody", IssueSeverity.Warning) 
                : null);
        }

        [Required]
        [MinLength(4)]
        public string Test
        {
            get => _test;
            set => this.Set(ref _test, value);
        }


        public ICommand AsyncCommand => new AsyncCommand<object>((obj) => AsyncTestMethod());

        public async Task AsyncTestMethod()
        {
            await Task.Delay(100).ConfigureAwait(false);
            await Task.Delay(100).ConfigureAwait(false);
            await Task.Delay(100).ConfigureAwait(false);
            await Task.Delay(100).ConfigureAwait(false);
            await Task.Delay(100).ConfigureAwait(false);
        }
    }
}
