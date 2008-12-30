﻿using System;
using System.Linq.Expressions;
using System.Web;
using AltOxite.Core.Web;
using AltOxite.Core.Web.Behaviors;
using AltOxite.Core.Web.Controllers;
using FubuMVC.Container.StructureMap.Config;
using FubuMVC.Core.Behaviors;

namespace AltOxite.Web
{
    public class Global : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            ControllerConfig.Configure = x =>
            {
                // Default Behaviors for all actions
                /////////////////////////////////////////////////
                x.ByDefault.EveryControllerAction(d => d
                    .Will<set_the_current_site_details_on_the_output_viewmodel>()
                    .Will<set_the_current_logged_in_user_on_the_output_viewmodel>()
                    .Will<load_the_current_principal>()
                    .Will<set_up_default_data_the_first_time_this_app_is_run>()
                    .Will<execute_the_result>()
                    .Will<access_the_database_through_a_unit_of_work>());

                // Automatic controller registration
                /////////////////////////////////////////////////
                x.AddControllersFromAssembly.ContainingType<ViewModel>(c =>
                {
                    // All objects in Web.Controllers whose name ends with "*Controller"
                    // All public OMIOMO methods are actions, so no need to filter the methods
                    c.Where(t => 
                        t.Namespace.EndsWith("Web.Controllers") 
                        && t.Name.EndsWith("Controller"));

                    c.MapActionsWhere((m,i,o) => true);
                });

                // Manual overrides
                /////////////////////////////////////////////////

                //-- Make the primary URL for logout be "/logout" instead of "login/logout"
                x.OverrideConfigFor(LogoutAction, config =>
                {
                    config.PrimaryUrl = "logout/";
                    config.RemoveAllBehaviors();
                    config.AddBehavior<execute_the_result>();
                });

                x.OverrideConfigFor(BlogPostIndexAction, config=>
                {
                    config.PrimaryUrl = "blog/{PostYear}/{PostMonth}/{PostDay}/{Slug}";
                });
            };

            Bootstrapper.Bootstrap();
        }

        private readonly Expression<Func<LoginController, object>> LogoutAction = c => c.Logout(null);
        private readonly Expression<Func<BlogPostController, object>> BlogPostIndexAction = c => c.Index(null);
    }
}