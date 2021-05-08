using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Generators
{
    internal class GenerationInfos
    {
        internal class PartialClass
        {
            internal class Property
            {
                public string FieldName { get; set; }
                public string Name { get; set; }
                public string Type { get; set; }
                public string TypeNamespace { get; set; }
                public readonly List<string> Attributes = new List<string>();
                public readonly List<string> AttributeNamespaces = new List<string>();
                public readonly List<string> PropertiesToNotify = new List<string>();
            }

            public string Namespace { get; set; }
            public readonly List<Property> Properties = new List<Property>();
            public string Name { get; set; }

            public override string ToString()
            {
                if (!Properties.Any())
                    return "";


                var builder = new StringBuilder();

                builder.Append($@"namespace {Namespace}
{{
    partial class {Name} : INotifyPropertyChanged
    {{
        public event PropertyChangedEventHandler PropertyChanged;
");                
                
                foreach (var property in Properties)
                {
                    foreach (var attribute in property.Attributes)
                    {
                        builder.Append("        ");
                        builder.AppendLine(attribute);
                    }

                    builder.AppendLine($@"        public {property.Type} {property.Name}
        {{
            get => {property.FieldName};
            set
            {{
                if (!EqualityComparer<{property.Type}>.Default.Equals({property.FieldName}, value))
                {{
                    {property.FieldName} = value;");

                    foreach (var notifyProp in property.PropertiesToNotify)
                    {
                        builder.AppendLine(
$"                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(\"{notifyProp}\"));");
                    }

                    builder.AppendLine(
@"                }
            }
        }");
                }

                builder.Append(@"    }
}");

                return builder.ToString();
            }
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            foreach (var ns in GetNamespacesToInclude())
                builder.AppendLine("using " + ns + ";");

            builder.AppendLine();

            foreach (var c in ClassesToCreate)
            {
                builder.AppendLine(c.ToString());
            }

            return builder.ToString();
        }

        private IEnumerable<string> GetNamespacesToInclude() => ClassesToCreate
            .SelectMany(x => x.Properties)
            .SelectMany(x => new[] { x.TypeNamespace }.Union(x.AttributeNamespaces))
            .Union(new[] 
            {
                "System.Collections.Generic",
                "System.ComponentModel"
            })
            .Distinct();

        public readonly List<PartialClass> ClassesToCreate = new List<PartialClass>();
    }
}
