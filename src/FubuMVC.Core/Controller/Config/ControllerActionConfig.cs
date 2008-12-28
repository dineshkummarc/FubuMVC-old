using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using FubuMVC.Core.Behaviors;
using FubuMVC.Core.Util;

namespace FubuMVC.Core.Controller.Config
{
    public class ControllerActionConfig
    {
        public Expression _actionFunc;

        protected ControllerActionConfig()
        {
            Behaviors = new List<Type>();
            OtherUrls = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        }

        public static ControllerActionConfig ForAction<CONTROLLER, INPUT, OUTPUT>(Expression<Func<CONTROLLER, INPUT, OUTPUT>> expression)
            where CONTROLLER : class
            where INPUT : class, new()
            where OUTPUT : class
        {
            var method = ReflectionHelper.GetMethod(expression);

            return new ControllerActionConfig
                {
                    UniqueID = Guid.NewGuid().ToString(),
                    ControllerType = typeof (CONTROLLER),
                    ActionMethod = method,
                    _actionFunc = expression,
                    ActionName = GetActionName(method),
                    InputType = typeof (INPUT),
                    OutputType = typeof (OUTPUT)
                };
        }

        protected IList<Type> Behaviors { get; set; }
        protected HashSet<string> OtherUrls { get; set; }

        public virtual Type ControllerType { get; protected set;}
        public virtual MethodInfo ActionMethod { get; protected set; }
        public virtual string ActionName { get; protected set; }
        public virtual Type InputType { get; protected set; }
        public virtual Type OutputType { get; protected set; }
        public string PrimaryUrl { get; set; }

        public string UniqueID{ get; private set; }

        //TODO: This is smelly, but how else do I get generic-typed stuff out of non-generic class like this?
        // I need to somehow carry along the generic type payload, without actually being generic
        public virtual Func<CONTROLLER, INPUT, OUTPUT> GetActionFunc<CONTROLLER, INPUT, OUTPUT>()
            where CONTROLLER : class
            where INPUT : class, new()
            where OUTPUT : class
        {
            return ((Expression<Func<CONTROLLER, INPUT, OUTPUT>>)_actionFunc).Compile();
        }

        public virtual IEnumerable<string> GetOtherUrls() { return OtherUrls.AsEnumerable(); }
        public virtual IEnumerable<Type> GetBehaviors() { return Behaviors.AsEnumerable(); }

        public static string GetActionName(MethodInfo method)
        {
            return method.Name.ToLowerInvariant();
        }

        public bool IsTheSameActionAs<C, I, O>(Expression<Func<C, I, O>> otherFunc)
            where C : class
            where I : class
            where O : class
        {
            var otherActionName = GetActionName(ReflectionHelper.GetMethod(otherFunc));
            return IsTheSameActionAs(typeof(C), otherActionName);
        }

        public bool IsTheSameActionAs<C>(Expression<Func<C, object>> otherFunc)
           where C : class
        {
            var otherActionName = GetActionName(ReflectionHelper.GetMethod(otherFunc));
            return IsTheSameActionAs(typeof(C), otherActionName);
        }

        public virtual bool IsTheSameActionAs(ControllerActionConfig otherConfig)
        {
            var otherControllerType = otherConfig.ControllerType;
            var otherActionName = otherConfig.ActionName;

            return IsTheSameActionAs(otherControllerType, otherActionName);
        }

        protected virtual bool IsTheSameActionAs(Type otherControllerType, string otherActionName)
        {
            return ActionName.Equals(otherActionName, StringComparison.OrdinalIgnoreCase) && otherControllerType == ControllerType;
        }

        public virtual void ApplyDefaultBehaviors(IEnumerable<Type> defaultBehaviors)
        {
            Behaviors.AddRange(defaultBehaviors);
        }

        public virtual void AddBehavior<BEHAVIOR>()
            where BEHAVIOR : IControllerActionBehavior
        {
            Behaviors.Add(typeof (BEHAVIOR));
        }

        public virtual void RemoveBehavior<BEHAVIOR>()
            where BEHAVIOR : IControllerActionBehavior
        {
            Behaviors.Remove(typeof (BEHAVIOR));
        }

        public virtual void RemoveAllBehaviors()
        {
            Behaviors.Clear();
        }

        public virtual void AddOtherUrl(string url)
        {
            OtherUrls.Add(url);
        }

        public virtual void RemoveOtherUrl(string url)
        {
            OtherUrls.Remove(url);
        }
    }
}