using System;

namespace SaneWpf.Framework
{
    public class Validation : IEquatable<Validation>
    {
        public enum IssueSeverity
        {
            Error = 1,
            Warning = 2
        }

        public static Validation Warning(string message) => new Validation(message, IssueSeverity.Warning);

        public static Validation Error(string message) => new Validation(message, IssueSeverity.Error);

        private Validation(string message, IssueSeverity severity)
        {
            Message = message;
            Severity = severity;
        }

        public IssueSeverity Severity { get; }

        public string Message { get; }

        public bool Equals(Validation other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Severity == other.Severity && Message == other.Message;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Validation) obj);
        }

        public override int GetHashCode() => HashCode.Combine((int) Severity, Message);

        public override string ToString() => Message;
    }
}
