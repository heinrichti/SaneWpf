﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Windows.Input;
using SaneWpf.Framework;

namespace SaneWpfSample
{
    class MainWindowViewModel : ViewModelBase
    {
        private string _test;
        private int _numberTest;

        public MainWindowViewModel()
        {
            AddValidation(() => Test, s => s == "nobody", Validation.Warning("Name should not be nobody"));
            AddValidation(() => NumberTest, i => i < 25 || i > 50, Validation.Error("NumberTest has to be between 25 and 50"));

            InitializeCommand = new AsyncCommand(async _ =>
            {
                await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
                NumberTest = 355;
            });
        }

        [Required]
        [MinLength(4)]
        public string Test
        {
            get => _test;
            set => Set(ref _test, value);
        }

        public int NumberTest
        {
            get => _numberTest;
            set => Set(ref _numberTest, value);
        }


        public ICommand AsyncCommand => new AsyncCommand(_ => AsyncTestMethod());

        public async Task AsyncTestMethod()
        {
            await Task.Delay(100).ConfigureAwait(false);
            await Task.Delay(100).ConfigureAwait(false);
            await Task.Delay(100).ConfigureAwait(false);
            await Task.Delay(100).ConfigureAwait(false);
            await Task.Delay(100).ConfigureAwait(false);
        }

        public ICommand InitializeCommand { get; }
    }
}
