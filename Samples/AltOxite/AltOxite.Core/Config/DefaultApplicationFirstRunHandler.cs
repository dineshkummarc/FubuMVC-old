using System;
using AltOxite.Core.Domain;
using AltOxite.Core.Persistence;

namespace AltOxite.Core.Config
{
    public class DefaultApplicationFirstRunHandler : IApplicationFirstRunHandler
    {
        private readonly IRepository _repository;
        private readonly ISessionSourceConfiguration _sourceConfig;

        public DefaultApplicationFirstRunHandler(ISessionSourceConfiguration sourceConfig, IRepository repository)
        {
            _sourceConfig = sourceConfig;
            _repository = repository;
        }

        public void InitializeIfNecessary()
        {
            if (!_sourceConfig.IsNewDatabase) return;

            setup_admin_user();
            setup_sample_post();
        }

        private void setup_admin_user()
        {
            var defaultUser = new User
                {
                    Username = "Admin",
                    Password = "pa$$w0rd",
                    DisplayName = "Oxite Administrator"
                };

            _repository.Save(defaultUser);
        }

        private void setup_sample_post()
        {
            var defaultPost = new Post
                {
                    Title = "World.Hello()",
                    BodyShort = "Welcome to Oxite! &nbsp;This is a sample application targeting developers built on <a href=\"http://asp.net/mvc\">ASP.NET MVC</a>. &nbsp;Make any changes you like. &nbsp;If you build a feature you think other developers would be interested in and would like to share your code go to the <a href=\"http://www.codeplex.com/oxite\">Oxite Code Plex project</a> to see how you can contribute.<br /><br />To get started, sign in with \"Admin\" and \"pa$$w0rd\" and click on the Admin tab.<br /><br />For more information about <a href=\"http://oxite.net\">Oxite</a> visit the default <a href=\"/About\">About</a> page.",
                    Published = DateTime.Parse("2008-12-05 09:29:03.270")
                };

            _repository.Save(defaultPost);
        }
    }
}