using System.Linq;
using System.Text;
using System.Web.Http.Controllers;
using System.Web.OData.Routing;
using System.Web.OData.Routing.Conventions;

namespace ODataFileRepository.Infrastructure.ODataExtensions
{
    public class MediaEntityStreamRoutingConvention : EntityRoutingConvention
    {
        private const string ODataMediaLinkEntryPath = "~/entityset/key/$value";
        private const string ValueAction = "Value";

        public override string SelectAction(
            ODataPath odataPath,
            HttpControllerContext controllerContext,
            ILookup<string, HttpActionDescriptor> actionMap)
        {
            if (odataPath.PathTemplate != ODataMediaLinkEntryPath)
            {
                return null;
            }

            controllerContext.RouteData.Values["key"] = ((KeyValuePathSegment)odataPath.Segments[1]).Value;

            string method = controllerContext.Request.Method.ToString().ToLowerInvariant();

            var action = new StringBuilder(method);

            // select action based on the syntax: <verb>'MediaResource', where <verb> is Pascal case
            action[0] = char.ToUpperInvariant(action[0]);

            string simpleActionName = action.ToString();
            action.Append(ValueAction);

            var composedActionName = action.ToString();

            if (actionMap.Contains(composedActionName))
            {
                return composedActionName;
            }

            // [OData-Protocol]: A successful DELETE request to the entity's edit URL or to the edit URL of its media resource deletes the media entity
            if (method == "delete" && actionMap.Contains(simpleActionName))
            {
                return simpleActionName;
            }

            return null;
        }
    }
}