using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Validation = SaneWpf.Framework.Validation;

namespace SaneWpf.Controls
{
    /// <summary>
    /// Interaction logic for IssuesControl.xaml
    /// </summary>
    public partial class IssuesControl : UserControl
    {
        public IssuesControl()
        {
            InitializeComponent();

            DataContextChanged += OnDataContextChanged;
            Warnings = new ObservableCollection<Validation>();
            Errors = new ObservableCollection<Validation>();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is INotifyDataErrorInfo oldVm) 
                oldVm.ErrorsChanged -= VmOnErrorsChanged;

            if (e.NewValue is INotifyDataErrorInfo newVm)
            {
                newVm.ErrorsChanged += VmOnErrorsChanged;
                UpdateErrors(newVm);
            }
            else
                ClearErrors();
        }

        public static readonly DependencyProperty ErrorsProperty = DependencyProperty.Register(
            "Errors", typeof(ObservableCollection<Framework.Validation>), typeof(IssuesControl), new PropertyMetadata(default(ObservableCollection<Validation>)));

        public ObservableCollection<Validation> Errors
        {
            get { return (ObservableCollection<Validation>) GetValue(ErrorsProperty); }
            set { SetValue(ErrorsProperty, value); }
        }

        public static readonly DependencyProperty WarningsProperty = DependencyProperty.Register(
            "Warnings", typeof(ObservableCollection<Validation>), typeof(IssuesControl), new PropertyMetadata(default(ObservableCollection<Validation>)));

        public ObservableCollection<Validation> Warnings
        {
            get { return (ObservableCollection<Validation>) GetValue(WarningsProperty); }
            set { SetValue(WarningsProperty, value); }
        }

        private void ClearErrors()
        {
            Errors.Clear();
            Warnings.Clear();
        }

        private void UpdateErrors(INotifyDataErrorInfo viewModel)
        {
            ClearErrors();

            var validations = GetErrors(viewModel).ToList();
            foreach (var validation in validations.Where(x => x.Severity == Validation.IssueSeverity.Error)) 
                Errors.Add(validation);

            foreach (var validation in validations.Where(x => x.Severity == Validation.IssueSeverity.Warning)) 
                Warnings.Add(validation);
        }

        private IEnumerable<Validation> GetErrors(INotifyDataErrorInfo viewModel)
        {
            var enumerable = viewModel.GetErrors(null);
            foreach (var item in enumerable)
            {
                if (item is Validation validation)
                    yield return validation;
                else
                    yield return Validation.Error(item.ToString());
            }
        }

        private void VmOnErrorsChanged(object sender, DataErrorsChangedEventArgs e)
        {
            UpdateErrors((INotifyDataErrorInfo) sender);
        }
    }
}
