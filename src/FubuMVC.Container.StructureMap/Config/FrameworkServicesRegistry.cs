using Microsoft.Practices.ServiceLocation;
using FubuMVC.Core.Controller.Config;
using FubuMVC.Core.Routing;
using FubuMVC.Core.Security;
using FubuMVC.Core.View;
using FubuMVC.Core.View.WebForms;
using FubuMVC.Core.Web.Security;
using StructureMap.Attributes;
using StructureMap.Configuration.DSL;

namespace FubuMVC.Container.StructureMap.Config
{
    public class FrameworkServicesRegistry : Registry
    {
        // --------------------------------------------------------
        // --- DEFAULT/REQUIRED FRAMEWORK SERVICES FOR FUBU-MVC ---
        // --------------------------------------------------------

        // Core Services:
        //  - IRouteConfigurer
        //  - ISerivceLocator
        //  - IControllerConfigContext
        //  - IUrlResolver

        // Security/Authentication:
        //  - ISecurityContext
        //  - IAuthenticationContext

        // View Rendering Services
        //  - IViewRenderer
        //  - IWebFormsViewRenderer (for web-forms specific functionality like RenderPartialFor/RenderPartialExpression
        //  - IWebFormsControlBuilder (for Web Forms/ASPX rendering)
        //  - IOutputWriter

        protected override void configure()
        {
            //routeConfigurer = new RouteConfigurer();
            //ForRequestedType<IRouteConfigurer>().TheDefault.IsThis(routeConfigurer);

            ForRequestedType<IServiceLocator>()
                .AsSingletons()
                .TheDefault.Is.Object(new StructureMapServiceLocator());

            ForRequestedType<IControllerConfigContext>()
                .CacheBy(InstanceScope.Hybrid)
                .TheDefault.Is.OfConcreteType<DefaultControllerConfigContext>();
            
            
            ForRequestedType<IOutputWriter>().TheDefault.Is.OfConcreteType<HttpResponseOutputWriter>();

            ForRequestedType<ISecurityContext>().TheDefault.Is.OfConcreteType<WebSecurityContext>();
            ForRequestedType<IAuthenticationContext>().TheDefault.Is.OfConcreteType<WebAuthenticationContext>();

            ForRequestedType<IViewRenderer>().TheDefault.Is.OfConcreteType<WebFormsViewRenderer>();

            //***  Can be replaced by DefaultConventionScanner
            //***  Left in here for now to make documentation easier later
            ForRequestedType<IRouteConfigurer>().AsSingletons().TheDefault.Is.OfConcreteType<RouteConfigurer>();
            ForRequestedType<IWebFormsControlBuilder>().TheDefault.Is.OfConcreteType<WebFormsControlBuilder>();
            ForRequestedType<IWebFormsViewRenderer>().TheDefault.Is.OfConcreteType<WebFormsViewRenderer>();
            ForRequestedType<IUrlResolver>().TheDefault.Is.OfConcreteType<UrlResolver>();
            //***  

            Scan(x =>
                {
                    x.TheCallingAssembly();
                    x.AssemblyContainingType<FrameworkServicesRegistry>();
                    
                    //NOTE: Currently disabled to ease documentation later, see above
                    //x.With<DefaultConventionScanner>();  
                });
        }

        public IRouteConfigurer routeConfigurer { get; private set; }
    }
}