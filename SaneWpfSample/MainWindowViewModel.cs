using System.ComponentModel.DataAnnotations;
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

    }
}
