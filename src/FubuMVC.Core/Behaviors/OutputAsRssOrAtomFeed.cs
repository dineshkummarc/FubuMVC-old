using System.ServiceModel.Syndication;
using FubuMVC.Core.Controller;
using FubuMVC.Core.Controller.Config;
using FubuMVC.Core.Controller.Results;
using FubuMVC.Core.Routing;
using Microsoft.Practices.ServiceLocation;

namespace FubuMVC.Core.Behaviors
{
    public class OutputAsRssOrAtomFeed : behavior_base_for_convenience
    {
        private readonly IServiceLocator _locator;
        private readonly ICurrentRequest _currentRequest;
        private readonly FubuConventions _conventions;

        public OutputAsRssOrAtomFeed(IServiceLocator locator, ICurrentRequest currentRequest, FubuConventions conventions)
        {
            _locator = locator;
            _currentRequest = currentRequest;
            _conventions = conventions;
        }

        public override OUTPUT AfterInvocation<OUTPUT>(OUTPUT output, IInvocationResult insideResult)
        {
            var isRss = _currentRequest.GetUrl().ToString().ToLower().EndsWith(_conventions.DefaultRssExtension.ToLower());
            var isAtom = _currentRequest.GetUrl().ToString().ToLower().EndsWith(_conventions.DefaultAtomExtension.ToLower());

            if (isRss || isAtom)
            {
                var feedConvertor = _locator.GetInstance<IFeedConverterFor<OUTPUT>>();

                SyndicationFeed syndicationFeed;
                if (feedConvertor.TryConvertModel(output, out syndicationFeed))
                {
                    Result = ResultOverride.IfAvailable(output) ?? (isRss
                        ? new RenderRssOrAtomResult(syndicationFeed.SaveAsRss20)
                        : new RenderRssOrAtomResult(syndicationFeed.SaveAsRss20));
                    return output;
                }

                Result = ResultOverride.IfAvailable(output) ?? new RedirectResult("404");
                return output;
            }

            Result = insideResult;
            return output;
        }
    }
}