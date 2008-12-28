using System;
using System.Collections.Generic;
using NUnit.Framework;
using FubuMVC.Core.Behaviors;
using FubuMVC.Core.Controller;
using Rhino.Mocks;

namespace FubuMVC.Tests.Controller
{
    [TestFixture]
    public class ThunderdomeActionInvokerTester
    {
        private ThunderdomeActionInvoker<TestController, TestInputModel, TestOutputModel> _invoker;
        private TestController _controller;
        private IControllerActionBehavior _behavior;

        [SetUp]
        public void SetUp()
        {
            _controller = new TestController();
            _behavior = MockRepository.GenerateMock<IControllerActionBehavior>();
            _invoker =
                new ThunderdomeActionInvoker<TestController, TestInputModel, TestOutputModel>(_controller, _behavior);
        }


        [Test]
        public void should_throw_exception_if_there_was_a_problem_populating_the_input_model()
        {
            typeof (InvalidOperationException).ShouldBeThrownBy(() => _invoker.Invoke((c, i) => null, new Dictionary<string, object>{{"PropInt", "BOGUS"}}));
        }

        [Test]
        public void invoke_executes_the_action_and_returns_a_result()
        {
            var testName = "TEST";
            var requestData = new Dictionary<string, object> { { "Prop1", testName } };

            _invoker.Invoke((c, i) => c.SomeAction(i), requestData);

            _behavior.AssertWasCalled(b => b.Invoke<TestInputModel, TestOutputModel>(null, null), o => o.IgnoreArguments());
        }
    }
}