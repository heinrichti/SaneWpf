using System.ComponentModel.DataAnnotations;
using SaneWpf.Framework;

namespace SaneWpfSample
{
    class MainWindowViewModel : ViewModelBase
    {
        private string _test;

        [Required]
        [MinLength(4)]
        public string Test
        {
            get => _test;
            set => this.Set(ref _test, value);
        }

    }
}
