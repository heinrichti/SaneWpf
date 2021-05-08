using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaneWpf.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class AutoNotifyAttribute : Attribute
    {
        public AutoNotifyAttribute(params string[] propertyList)
        {

        }
    }
}
