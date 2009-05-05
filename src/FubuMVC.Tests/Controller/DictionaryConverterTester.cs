using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using FubuMVC.Core.Controller;

namespace FubuMVC.Tests.Controller
{
    [TestFixture]
    public class DictionaryConverterTester
    {
        private ICollection<ConvertProblem> _problems;

        [SetUp]
        public void SetUp()
        {
            _problems = null;
        }

        [Test]
        public void can_create_type_should_return_false_for_type_with_no_public_parameterless_ctor()
        {
            DictionaryConverter.CanCreateType(typeof(NoCtor)).ShouldBeFalse();
        }

        [Test]
        public void populate_should_ignore_a_null_values_dictionary()
        {
            var item = new Turkey();
            DictionaryConverter.Populate(null, item, out _problems);
            item.Name.ShouldBeNull();
            _problems.Count().ShouldEqual(0);
        }

        [Test]
        public void populate_should_set_all_property_values_present_in_dictionary()
        {
            var item = new Turkey();

            var dict = new Dictionary<string, object>{{"Name","Boris"}, {"Age","2"}};

            DictionaryConverter.Populate(dict, item, out _problems);
            item.Name.ShouldEqual("Boris");
            item.Age.ShouldEqual(2);
        }

        [Test, Ignore("Removed requirement for case-insensitivity. May add back later")]
        public void populate_should_set_all_property_values_present_in_dictionary_regardless_of_key_casing()
        {
            var item = new Turkey();

            var dict = new Dictionary<string, object> { { "nAme", "Smith" }, { "AGE", 9 } };

            DictionaryConverter.Populate(dict, item, out _problems);
            item.Name.ShouldEqual("Smith");
            item.Age.ShouldEqual(9);
        }

        [Test]
        public void populate_should_not_change_property_values_not_found_in_the_dictionary()
        {
            var item = new Turkey();
            item.Name = "Smith";

            var dict = new Dictionary<string, object> { { "Age", 9 } };

            DictionaryConverter.Populate(dict, item, out _problems);
            item.Name.ShouldEqual("Smith");
            item.Age.ShouldEqual(9);
        }

        [Test]
        public void populate_extra_values_in_dictionary_are_ignored()
        {
            var item = new Turkey();

            var dict = new Dictionary<string, object> { { "xyzzy", "foo" } };

            DictionaryConverter.Populate(dict, item, out _problems);
            item.Name.ShouldBeNull();
            item.Age.ShouldEqual(0);
            _problems.Count().ShouldEqual(0);
        }

        [Test, Ignore("Removed requirement for case-insensitivity. May add back later")]
        public void create_and_populate_should_create_new_object_and_set_all_property_values_present_in_dictionary_regardless_of_key_casing()
        {
            var dict = new Dictionary<string, object> { { "nAme", "Sally" }, { "AGE", 12 } };

            var item = DictionaryConverter.CreateAndPopulate<Turkey>(dict, out _problems);
            item.Name.ShouldEqual("Sally");
            item.Age.ShouldEqual(12);
        }

        [Test]
        public void create_and_populate_should_convert_between_types()
        {
            var dict = new Dictionary<string, object> { { "Age", "12" }, {"Alive", "True"}, {"BirthDate", "01-JUN-2008"} };

            var item = DictionaryConverter.CreateAndPopulate<Turkey>(dict, out _problems);
            item.Age.ShouldEqual(12);
            item.Alive.ShouldBeTrue();
            item.BirthDate.ShouldEqual(new DateTime(2008, 06, 01));
        }

        [Test]
        public void create_and_populate_should_not_throw_exception_during_type_conversion()
        {
            var dict = new Dictionary<string, object> { { "Age", "abc" } };

            var item = DictionaryConverter.CreateAndPopulate<Turkey>(dict, out _problems);
            item.Age.ShouldEqual(default(int));
            _problems.Count().ShouldEqual(1);
        }

        [Test]
        public void safe_create_and_populate_should_throw_exception_if_there_are_problems_converting()
        {
            var dict = new Dictionary<string, object> { { "Age", "abc" } };

            typeof (InvalidOperationException).ShouldBeThrownBy(
                () => DictionaryConverter.SafeCreateAndPopulate<Turkey>(dict));
        }

        [Test]
        public void create_and_populate_should_set_a_notification_when_one_a_value_was_found_but_could_not_be_set()
        {
            var dict = new Dictionary<string, object> { { "Age", "abc" } };

            var item = DictionaryConverter.CreateAndPopulate<Turkey>(dict, out _problems);

            _problems.Count().ShouldEqual(1);

            ConvertProblem problem = _problems.First();
            problem.Item.ShouldBeTheSameAs(item);
            problem.Property.Name.ShouldEqual("Age");
            problem.Value.ShouldEqual("abc");
        }

        [Test]
        public void Read_a_boolean_type_that_is_true()
        {
            var dict = new Dictionary<string, object> { { "Alive", "true" } };

            var item = DictionaryConverter.CreateAndPopulate<Turkey>(dict, out _problems);
            item.Alive.ShouldBeTrue();
        }

        [Test]
        public void Read_a_boolean_type_that_is_false()
        {
            var dict = new Dictionary<string, object> { { "Alive", "" } };

            var item = DictionaryConverter.CreateAndPopulate<Turkey>(dict, out _problems);
            item.Alive.ShouldBeFalse();
            _problems.Count().ShouldEqual(0);
        }
        
        [Test]
        public void Read_a_Nullable_value_type_empty_string_as_null()
        {
            var dict = new Dictionary<string, object> { { "NullableInt", "" } };

            var item = DictionaryConverter.CreateAndPopulate<Turkey>(dict, out _problems);
            item.NullableInt.ShouldBeNull();
            _problems.Count().ShouldEqual(0);
        }

        [Test]
        public void Read_a_Nullable_value_type()
        {
            var dict = new Dictionary<string, object> { { "NullableInt", "8" } };

            DictionaryConverter.CreateAndPopulate<Turkey>(dict, out _problems).NullableInt.ShouldEqual(8);
        }

        [Test]
        public void Checkbox_handling__if_the_property_type_is_boolean_and_the_value_equals_the_name_then_set_the_property_to_true()
        {
            var dict = new Dictionary<string, object> { { "Alive", "Alive" } };

            DictionaryConverter.CreateAndPopulate<Turkey>(dict, out _problems).Alive.ShouldBeTrue();
        }

        [Test]
        public void Checkbox_handling__if_the_property_type_is_boolean_and_the_value_does_not_equal_the_name_and_isnt_a_recognizeable_boolean_a_problem_should_be_attached()
        {
            var dict = new Dictionary<string, object> { { "Alive", "BOGUS" } };

            ICollection<ConvertProblem> problems;
            var value = DictionaryConverter.CreateAndPopulate<Turkey>(dict, out problems);
            value.Alive.ShouldBeFalse();
            problems.Count().ShouldEqual(1);
        }

        private class Turkey
        {
            public string Name { get; set; }
            public int Age { get; set; }
            public int? NullableInt { get; set; }
            public bool Alive { get; set; }
            public DateTime BirthDate { get; set; }
        }

        private class WrongBaseType
        {
            public string Name { get; set; }
        }

        private class NoCtor
        {
            private NoCtor()
            {
            }

            public string Name { get; set; }
            public int Age { get; set; }
        }
    }
}