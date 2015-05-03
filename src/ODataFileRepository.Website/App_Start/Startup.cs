using Microsoft.Owin;
using Owin;
using System.Web.Http;
using System.Web.OData.Batch;
using System.Web.OData.Extensions;

[assembly: OwinStartup(typeof(ODataFileRepository.Website.Startup))]

namespace ODataFileRepository.Website
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var config = new HttpConfiguration();

            config.MapHttpAttributeRoutes();

            var batchHandler = new DefaultODataBatchHandler(new HttpServer(config));
            config.EnableCaseInsensitive(true);

            config.MapODataServiceRoute(
                routeName: "ODataService",
                routePrefix: "",
                model: OData.CreateModel(),
                batchHandler: batchHandler);

            app.UseWebApi(config);
        }
    }
}