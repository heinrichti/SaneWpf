using SaneWpf.Framework;

namespace SaneWpfSample
{
    class MyDialogViewModel : ViewModelBase
    {
        private string _name;

        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }
    }
}
