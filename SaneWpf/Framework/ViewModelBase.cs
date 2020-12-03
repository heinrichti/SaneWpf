using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace SaneWpf.Framework
{
    public class ViewModelBase : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        private readonly Dictionary<string, List<object>> _validationRules = new Dictionary<string, List<object>>();

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected bool Set<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            RaisePropertyChanged(propertyName);
            ValidateProperty(value, propertyName);
            return true;
        }

        private readonly Dictionary<string, List<ValidationIssue>> _validationIssues = new Dictionary<string, List<ValidationIssue>>();

        private void ValidateProperty<T>(T value, [CallerMemberName] string propertyName = null)
        {
            if (propertyName == null) return;

            var hasOldIssues = _validationIssues.TryGetValue(propertyName, out var oldPropertyIssues);

            var issues = new List<ValidationIssue>();
            
            var validationContext = new ValidationContext(this);
            validationContext.MemberName = propertyName;
            var validationResult = new Collection<ValidationResult>();
            if (!Validator.TryValidateProperty(value, validationContext, validationResult))
                issues.AddRange(validationResult.Select(result => new ValidationIssue(result.ErrorMessage, IssueSeverity.Error)));

            if (_validationRules.TryGetValue(propertyName, out var validationRules))
            {
                issues.AddRange(validationRules
                    .Cast<Func<T, ValidationIssue>>()
                    .Select(validationRule => validationRule(value))
                    .Where(validationIssue => validationIssue != null));
            }

            var hasErrors = HasErrors;

            _validationIssues.Remove(propertyName);
            if (issues.Any())
                _validationIssues[propertyName] = issues;

            if (!hasErrors && _validationIssues.Any() || hasErrors && !_validationIssues.Any()) 
                RaisePropertyChanged(nameof(HasErrors));

            if (hasOldIssues && !issues.Any() || !hasOldIssues && issues.Any() || hasOldIssues && issues.SequenceEqual(oldPropertyIssues))
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        protected void AddValidation<T>(string propertyName, Func<T, ValidationIssue> validationFunc)
        {
            if (_validationRules.TryGetValue(propertyName, out var rules))
                rules.Add(validationFunc);
            else
                _validationRules.Add(propertyName, new List<object> {validationFunc});
        }

        public IEnumerable GetErrors(string propertyName) => _validationIssues.TryGetValue(propertyName, out var issues)
            ? issues.Select(x => x.Text)
            : null;

        public bool HasErrors => _validationIssues.Any();

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
    }
}
