using System;
using System.Web.UI;
using FubuMVC.Core.Behaviors;
using FubuMVC.Core.Controller;
using FubuMVC.Core.View;

namespace FubuMVC.Tests
{
    public class TestController
    {
        public TestOutputModel SomeAction(TestInputModel value)
        {
            return new TestOutputModel { Prop1 = value.Prop1 };
        }

        public TestOutputModel2 AnotherAction(TestInputModel value)
        {
            return new TestOutputModel2 { Prop1 = value.Prop1 };
        }

        public TestOutputModel3 ThirdAction(TestInputModel value)
        {
            return new TestOutputModel3 { Prop1 = value.Prop1 };
        }
    }

    public class TestInputModel
    {
        public int PropInt { get; set; }
        public string Prop1 { get; set; }
    }

    public class TestOutputModel
    {
        public string Prop1 { get; set; }
    }

    public class TestOutputModel2 : TestOutputModel
    {
    }

    public class TestOutputModel3 : TestOutputModel
    {
    }

    public class TestPartialModel
    {
        public string PartialModelProp1 { get; set; }
    }

    public class TestView : IFubuView<TestOutputModel>
    {
        public void SetModel(object model)
        {
            throw new System.NotImplementedException();
        }

        public TestOutputModel Model
        {
            get { throw new System.NotImplementedException(); }
        }
    }

    public class TestUserControl : UserControl, IFubuView<TestPartialModel>
    {
        public void SetModel(object model)
        {
            throw new System.NotImplementedException();
        }

        public TestPartialModel Model
        {
            get { throw new System.NotImplementedException(); }
        }
    }

    public class TestBehavior2 : IControllerActionBehavior
    {
        public IControllerActionBehavior InsideBehavior { get; set; }
        public IInvocationResult Result { get; set; }
        public OUTPUT Invoke<INPUT, OUTPUT>(INPUT input, Func<INPUT, OUTPUT> func)
            where INPUT : class
            where OUTPUT : class
        {
            throw new System.NotImplementedException();
        }
    }

    public class TestBehavior : IControllerActionBehavior
    {
        public IControllerActionBehavior InsideBehavior { get; set; }
        public IInvocationResult Result { get; set; }
        public OUTPUT Invoke<INPUT, OUTPUT>(INPUT input, Func<INPUT, OUTPUT> func)
            where INPUT : class
            where OUTPUT : class
        {
            throw new System.NotImplementedException();
        }
    }

}