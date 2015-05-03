using System.Web.Http;

namespace ODataFileRepository.Website.Controllers
{
    public class DefaultController : ApiController
    {
        [Route("")]
        public IHttpActionResult Get()
        {
            return Ok("me");
        }
    }
}
