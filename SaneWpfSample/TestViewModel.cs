using SaneWpf.Attributes;
using SaneWpf.Framework;

namespace SaneWpfSample
{
    [ViewModel]
    internal partial class TestViewModel
    {
        [AutoNotify]
        private int _myProperty;

        public TestViewModel()
        {
            Validations.Add(this,
                x => x._myProperty,
                x => x != 25,
                Validation.Error("Hello world"));

            Validations.Add(this,
                x => x._myProperty,
                Test,
                Validation.Error("another validation"));
        }

        private bool Test(int t)
        {
            return true;
        }
    }
}
