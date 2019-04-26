using System;
using System.Collections.Generic;
using FluentAssertions;
using Smartive.Core.Wpf.ViewModels;
using Xunit;

namespace Smartive.Core.Wpf.Test.ViewModels
{
    public class BaseViewModelTest
    {
        private readonly TestViewModel _model;

        public BaseViewModelTest()
        {
            _model = new TestViewModel();
        }

        [Fact]
        public void Test_Event_Called_With_Property()
        {
            var called = false;
            _model.PropertyChanged += delegate { called = true; };

            _model.Name = "Foobar";
            called.Should().BeTrue();
        }

        [Fact]
        public void Test_Event_Called_With_Additional_Properties()
        {
            var properties = new List<string>();
            _model.PropertyChanged += (sender, args) => properties.Add(args.PropertyName);

            _model.AdditionalProp = "Foobar";
            properties.Should().Contain("AdditionalProp");
            properties.Should().Contain("Name");
        }

        [Fact]
        public void Test_Should_Not_Throw()
        {
            var fn = new Action(() => { _model.Name = "Foo"; });

            fn.Should().NotThrow();
        }

        private class TestViewModel : BaseViewModel
        {
            private string _name;

            public string Name
            {
                get => _name;
                set => SetField(ref _name, value);
            }

            private string _additionalProp;

            public string AdditionalProp
            {
                get => _additionalProp;
                set
                {
                    SetField(ref _additionalProp, value);
                    OnPropertyChanged(nameof(Name));
                }
            }
        }
    }
}
