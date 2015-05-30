using ODataFileRepository.Website.DataAccess.Contracts;
using ODataFileRepository.Website.DataAccess.Exceptions;
using ODataFileRepository.Website.Infrastructure.ODataExtensions;
using ODataFileRepository.Website.ServiceModels;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Results;

namespace ODataFileRepository.Website.Controllers
{
    [ExtendedODataFormatting, ODataRouting]
    public sealed class UploadSessionsController : ApiController
    {
        private readonly Lazy<IFileDataAccess> _fileDataAccess;
        private readonly Lazy<IUploadSessionDataAccess> _uploadSessionDataAccess;

        public UploadSessionsController(
            Lazy<IFileDataAccess> fileDataAccess,
            Lazy<IUploadSessionDataAccess> uploadSessionDataAccess)
        {
            _fileDataAccess = fileDataAccess;
            _uploadSessionDataAccess = uploadSessionDataAccess;
        }

        private IFileDataAccess FileDataAccess
        {
            get { return _fileDataAccess.Value; }
        }

        private IUploadSessionDataAccess UploadSessionDataAccess
        {
            get { return _uploadSessionDataAccess.Value; }
        }

        public async Task<IHttpActionResult> Get()
        {
            var files = await UploadSessionDataAccess.GetAllAsync();

            return Ok(files.Select(f => new UploadSession(f)));
        }

        public async Task<IHttpActionResult> Get([FromODataUri] string key)
        {
            var uploadSession = await UploadSessionDataAccess.GetAsync(key);

            if (uploadSession == null)
            {
                return NotFound();
            }

            return Ok(new UploadSession(uploadSession));
        }

        [HttpPost]
        public async Task<IHttpActionResult> Post(UploadSession uploadSession)
        {
            if (uploadSession == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string uploadSessionIdentifier = Guid.NewGuid().ToString("N").ToLowerInvariant();
            string fileIdentifier = Guid.NewGuid().ToString("N").ToLowerInvariant();
            string fileName = uploadSession.FileName as string;

            var createdUploadSession = await UploadSessionDataAccess.CreateAsync(
                uploadSessionIdentifier,
                fileIdentifier,
                fileName);

            return Created(new UploadSession(createdUploadSession));
        }

        public async Task<IHttpActionResult> PutValue([FromODataUri] string key)
        {
            try
            {
                var contentTypeHeader = Request.Content.Headers.ContentType;

                if (contentTypeHeader == null || contentTypeHeader.MediaType == null)
                {
                    return BadRequest();
                }

                var contentLength = Request.Content.Headers.ContentLength;

                if (!contentLength.HasValue)
                {
                    return StatusCode(HttpStatusCode.LengthRequired);
                }

                if (contentLength.Value <= 0)
                {
                    return BadRequest();
                }

                var contentRange = Request.Content.Headers.ContentRange;

                if (contentRange == null
                    || !contentRange.HasRange
                    || !contentRange.HasLength
                    || !contentRange.From.HasValue
                    || !contentRange.To.HasValue
                    || !contentRange.Unit.Equals("bytes", StringComparison.Ordinal)
                    || contentRange.Length <= 0
                    || contentRange.From.Value < 0
                    || contentRange.From.Value > contentRange.Length - 1
                    || contentRange.From.Value > contentRange.To.Value
                    || contentRange.To.Value < 0
                    || contentRange.To.Value > contentRange.Length - 1
                    || contentRange.To.Value - contentRange.From.Value + 1 != contentLength)
                {
                    return BadRequest();
                }

                var mediaType = contentTypeHeader.MediaType;

                var stream = await Request.Content.ReadAsStreamAsync();

                var uploadSession = await UploadSessionDataAccess.UploadSegmentAsync(
                    key,
                    mediaType,
                    contentRange.From.Value,
                    contentRange.To.Value,
                    contentRange.Length.Value,
                    stream);

                if (uploadSession.Finished)
                {
                    var uploadedFile = await FileDataAccess.GetAsync(uploadSession.FileIdentifier);

                    return Ok(new UploadSession(uploadSession, uploadedFile));
                }
                else
                {
                    return StatusCode(HttpStatusCode.Accepted);
                }
            }
            catch (ResourceNotFoundException)
            {
                return NotFound();
            }
        }

        public async Task<IHttpActionResult> Delete([FromODataUri] string key)
        {
            try
            {
                await UploadSessionDataAccess.DeleteAsync(key);

                return StatusCode(HttpStatusCode.NoContent);
            }
            catch (ResourceNotFoundException)
            {
                return NotFound();
            }
        }

        /// <summary>Creates an action result with the specified values that is a response to a POST operation with an entity to an entity set.</summary>
		/// <returns>A <see cref="T:System.Web.OData.Results.CreatedODataResult`1" /> with the specified values.</returns>
		/// <param name="entity">The created entity.</param>
		/// <typeparam name="TEntity">The created entity type.</typeparam>
		private CreatedODataResult<TEntity> Created<TEntity>(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            return new CreatedODataResult<TEntity>(entity, this);
        }
    }
}