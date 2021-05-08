using System;

namespace SaneWpf.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class AutoNotifyAttribute : Attribute
    {
        public AutoNotifyAttribute(params string[] additionalAttributes)
        {
        }
    }
}
