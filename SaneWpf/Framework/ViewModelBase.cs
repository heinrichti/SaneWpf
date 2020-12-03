using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
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
            ValidateProperty(value, propertyName);
            RaisePropertyChanged(propertyName);
            return true;
        }

        private readonly Dictionary<string, List<Validation>> _validationIssues = new Dictionary<string, List<Validation>>();

        private void ValidateProperty<T>(T value, [CallerMemberName] string propertyName = null)
        {
            if (propertyName == null) return;

            var issues = new List<Validation>();
            
            var validationContext = new ValidationContext(this);
            validationContext.MemberName = propertyName;
            var validationResult = new Collection<ValidationResult>();
            if (!Validator.TryValidateProperty(value, validationContext, validationResult))
                issues.AddRange(validationResult.Select(result => Validation.Error(result.ErrorMessage)));

            if (_validationRules.TryGetValue(propertyName, out var validationRules))
            {
                issues.AddRange(validationRules
                    .Cast<Func<T, Validation>>()
                    .Select(validationRule => validationRule(value))
                    .Where(validationIssue => validationIssue != null));
            }

            if (_validationIssues.Remove(propertyName))
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));

            if (issues.Any())
                _validationIssues[propertyName] = issues;
        }

        protected void AddValidation<T>(
            Expression<Func<T>> property, 
            Func<T, bool> errorCondition,
            Validation validation)
        {
            if (!(property.Body is MemberExpression memberExpression))
                throw new ArgumentException(nameof(property) + " must be a MemberExpression");
            
            var propertyName = memberExpression.Member.Name;

            Validation ValidateFunc(T arg) => errorCondition(arg) ? validation : null;

            if (_validationRules.TryGetValue(propertyName, out var rules))
                rules.Add((Func<T, Validation>) ValidateFunc);
            else
                _validationRules.Add(propertyName, new List<object> {(Func<T, Validation>) ValidateFunc});
        }

        public IEnumerable GetErrors(string propertyName) => _validationIssues.TryGetValue(propertyName, out var issues)
            ? issues
            : null;

        public bool HasErrors => _validationIssues.Any();

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
    }
}
