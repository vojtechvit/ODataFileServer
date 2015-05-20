using Microsoft.Owin;
using Ninject.Web.Common.OwinHost;
using Ninject.Web.WebApi.OwinHost;
using ODataFileRepository.Infrastructure.ODataExtensions;
using Owin;
using System.Diagnostics.CodeAnalysis;
using System.Web.Http;
using System.Web.OData.Batch;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;
using System.Web.OData.Routing.Conventions;

[assembly: OwinStartup(typeof(ODataFileRepository.Website.Startup))]

namespace ODataFileRepository.Website
{
    public partial class Startup
    {
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic",
            Justification = "Method must be non-static due to OWIN conventions.")]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope",
            Justification = "HttpConfiguration, DefaultODataBatchHandler and HttpServer are disposed by the web server.")]
        public void Configuration(IAppBuilder app)
        {
            var config = new HttpConfiguration();

            var model = OData.CreateModel();
            var pathHandler = new DefaultODataPathHandler();
            var routingConventions = ODataRoutingConventions.CreateDefault();
            routingConventions.Insert(0, new MediaEntityStreamRoutingConvention());
            var batchHandler = new DefaultODataBatchHandler(new HttpServer(config));

            config.MapODataServiceRoute(
                routeName: "ODataService",
                routePrefix: "",
                model: model,
                pathHandler: pathHandler,
                routingConventions: routingConventions,
                batchHandler: batchHandler);

            app.UseNinjectMiddleware(Ninject.GetConfiguredKernel);
            app.UseNinjectWebApi(config);
        }
    }
}