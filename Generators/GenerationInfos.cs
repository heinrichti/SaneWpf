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
                internal class Validation
                {
                    public string ParameterName { get; set; }
                    public string MethodBody { get; set; }
                    public string ValidationErrorBody { get; set; }
                    public string ValidationErrorParameter { get; set; }
                    public string ParameterType { get; set; }
                    public string FieldName { get; set; }
                }

                public string FieldName { get; set; }
                public string Name { get; set; }
                public string Type { get; set; }
                public string TypeNamespace { get; set; }
                public string ClassName { get; set; }

                public readonly List<string> Attributes = new List<string>();
                public readonly List<string> AttributeNamespaces = new List<string>();
                public readonly List<string> PropertiesToNotify = new List<string>();
                public readonly List<Validation> Validations = new List<Validation>();

                public override string ToString()
                {
                    var builder = new StringBuilder();

                    foreach (var attribute in Attributes)
                    {
                        builder.Append("        ");
                        builder.AppendLine(attribute);
                    }

                    builder.AppendLine($@"        public {Type} {Name}
        {{
            get => {FieldName};
            set
            {{
                if (!EqualityComparer<{Type}>.Default.Equals({FieldName}, value))
                {{
                    {FieldName} = value;");

                    if (Validations.Any())
                    {
                        builder.AppendLine($@"                    _validationIssues.Remove(""{Name}"");
                    ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(""{Name}""));
");
                        builder.AppendLine("                    var issues = new List<Validation>();");

                        int validationMethod = 0;
                        foreach (var validation in Validations)
                        {
                            // local method for validation
                            builder.AppendLine(
$@"                    Validation error_{validationMethod}({ClassName} {validation.ValidationErrorParameter}) => {validation.ValidationErrorBody};
                    bool validate_{validationMethod}({validation.ParameterType} {validation.ParameterName}) => {validation.MethodBody};
                    if (validate_{validationMethod}({FieldName}))
                        issues.Add(error_{validationMethod}(this));");

                            ++validationMethod;
                        }

                        builder.AppendLine($@"                    if (issues.Any())
                    _validationIssues[""{Name}""] = issues;

                    ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(""{Name}""));");
                    }

                    foreach (var notifyProp in PropertiesToNotify)
                    {
                        builder.AppendLine(
$"                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(\"{notifyProp}\"));");
                    }

                    builder.AppendLine(
@"                }
            }
        }");

                    return builder.ToString();                    
                }
            }

            public string Namespace { get; set; }
            public readonly List<Property> Properties = new List<Property>();
            public string Name { get; set; }

            public override string ToString()
            {
                if (!Properties.Any())
                    return "";

                var hasValidations = Properties.Any(x => x.Validations.Any());
                var interfacesToImplement = "INotifyPropertyChanged";
                if (hasValidations)
                    interfacesToImplement += ", INotifyDataErrorInfo";

                var builder = new StringBuilder();

                builder.AppendLine($@"namespace {Namespace}
{{
    partial class {Name} : {interfacesToImplement}
    {{
        public event PropertyChangedEventHandler PropertyChanged;
");                
                
                if (hasValidations)
                {
                    builder.AppendLine(
@"        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        private readonly Dictionary<string, List<Validation>> _validationIssues = new Dictionary<string, List<Validation>>();

        public bool HasErrors => _validationIssues.Any();

        public IEnumerable GetErrors(string propertyName)
        {
            if (propertyName == null)
                return _validationIssues.SelectMany(x => x.Value);

            return _validationIssues.TryGetValue(propertyName, out var issues)
                ? issues
                : null;
        }
");
                }

                foreach (var property in Properties)
                {
                    builder.AppendLine(property.ToString());
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
                "SaneWpf.Framework",
                "System.Collections",
                "System.Collections.Generic",
                "System.ComponentModel",
                "System.Linq"
            })
            .Distinct();

        public readonly List<PartialClass> ClassesToCreate = new List<PartialClass>();
    }
}
