using Microsoft.Owin;
using Ninject;
using Ninject.Web.Common.OwinHost;
using Ninject.Web.WebApi;
using Ninject.Web.WebApi.OwinHost;
using ODataFileRepository.Infrastructure.ODataExtensions;
using Owin;
using System.Web.Http;
using System.Web.OData.Batch;
using System.Web.OData.Extensions;
using System.Web.OData.Formatter;
using System.Web.OData.Formatter.Deserialization;
using System.Web.OData.Routing;
using System.Web.OData.Routing.Conventions;

[assembly: OwinStartup(typeof(ODataFileRepository.Website.Startup))]

namespace ODataFileRepository.Website
{
    public partial class Startup
    {
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