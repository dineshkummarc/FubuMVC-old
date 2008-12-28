using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;

namespace FubuMVC.Core.Controller.Config
{
    public interface IRouteConfigurer
    {
        void LoadRoutes(RouteCollection routeCollection);
    }

    public class RouteConfigurer : IRouteConfigurer
    {
        private readonly IList<Route> _registeredRoutes = new List<Route>();
        private readonly FubuConventions _conventions;
        private readonly FubuConfiguration _config;

        public RouteConfigurer(FubuConfiguration configuration, FubuConventions conventions)
        {
            _config = configuration;
            _conventions = conventions;

            _config.GetControllerActionConfigs().Each(ConfigureAction);
        }
        
        public Route AppDefaultRoute { get; private set; }

        public void ConfigureAction(ControllerActionConfig config)
        {
            set_this_as_app_default_if_necessary(config);

            register_Routes(config);
        }

        private void register_Routes(ControllerActionConfig config)
        {
            _registeredRoutes.Add(CreateRoute(config, _config.GetDefaultUrlFor(config.ControllerType)));

            _registeredRoutes.Add(CreateRoute(config, config.PrimaryUrl));

            config.GetOtherUrls().Each(url => _registeredRoutes.Add(CreateRoute(config, url)));
        }

        private void set_this_as_app_default_if_necessary(ControllerActionConfig config)
        {
            if (AppDefaultRoute != null && !_conventions.IsAppDefaultUrl(config)) return;
            
            _registeredRoutes.Remove(AppDefaultRoute);
            AppDefaultRoute = CreateRoute(config, "");
            _registeredRoutes.Add(AppDefaultRoute);
        }

        public Route CreateRoute(ControllerActionConfig config, string urlFormat)
        {
            return new Route(urlFormat, new ActionRouteHandler(config));
        }

        public IEnumerable<Route> GetRegisteredRoutes()
        {
            return _registeredRoutes.AsEnumerable();
        }

        public void LoadRoutes(RouteCollection routeCollection)
        {
            _registeredRoutes.Each(r => routeCollection.Add(r));
        }
    }
}