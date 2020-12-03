using System;

namespace SaneWpf.Framework
{
    public class ValidationIssue : IEquatable<ValidationIssue>
    {
        public ValidationIssue(string text, IssueSeverity severity)
        {
            Text = text;
            Severity = severity;
        }

        public IssueSeverity Severity { get; }

        public string Text { get; }

        public bool Equals(ValidationIssue other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Severity == other.Severity && Text == other.Text;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ValidationIssue) obj);
        }

        public override int GetHashCode() => HashCode.Combine((int) Severity, Text);
    }
}
