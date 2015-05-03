using ODataFileRepository.Website.Models;
using System.Web.Http;
using System.Web.OData;

namespace ODataFileRepository.Website.Controllers
{
    public sealed class FilesController : ODataController
    {
        public IHttpActionResult Get()
        {
            var files = new File[0];

            return Ok(files);
        }
    }
}