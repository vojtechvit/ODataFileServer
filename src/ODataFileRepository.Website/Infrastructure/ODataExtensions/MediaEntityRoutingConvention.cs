using System.Linq;
using System.Text;
using System.Web.Http.Controllers;
using System.Web.OData.Routing;
using System.Web.OData.Routing.Conventions;

namespace ODataFileRepository.Infrastructure.ODataExtensions
{
    public class MediaEntityRoutingConvention : EntityRoutingConvention
    {
        private const string ODataMediaLinkEntryPath = "~/entityset/key/$value";
        private const string ValueAction = "Value";

        public override string SelectAction(ODataPath odataPath, HttpControllerContext controllerContext, ILookup<string, HttpActionDescriptor> actionMap)
        {
            if (odataPath.PathTemplate != ODataMediaLinkEntryPath)
            {
                return null;
            }

            controllerContext.RouteData.Values["key"] = ((KeyValuePathSegment)odataPath.Segments[1]).Value;

            var action = new StringBuilder(controllerContext.Request.Method.ToString().ToLowerInvariant());

            // select action based on the syntax: <verb>'MediaResource', where <verb> is Pascal case
            action[0] = char.ToUpperInvariant(action[0]);
            action.Append(ValueAction);

            var actionName = action.ToString();

            // return null if there is no matching method
            return actionMap.Contains(actionName) ? actionName : null;
        }
    }
}