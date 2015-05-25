using ODataFileRepository.Website.DataAccess.Contracts;
using ODataFileRepository.Website.DataAccess.Exceptions;
using ODataFileRepository.Website.Infrastructure.ODataExtensions;
using ODataFileRepository.Website.ServiceModels;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.OData;
using System.Web.OData.Results;

namespace ODataFileRepository.Website.Controllers
{
    [ExtendedODataFormatting, ODataRouting]
    public sealed class FilesController : ApiController
    {
        private readonly Lazy<IFileDataAccess> _fileDataAccess;

        public FilesController(
            Lazy<IFileDataAccess> fileDataAccess)
        {
            _fileDataAccess = fileDataAccess;
        }

        private IFileDataAccess FileDataAccess
        {
            get { return _fileDataAccess.Value; }
        }

        public async Task<IHttpActionResult> Get()
        {
            var files = await FileDataAccess.GetAllAsync();

            return Ok(files.Select(f => new File(f)));
        }

        public async Task<IHttpActionResult> Get([FromODataUri] string key)
        {
            var file = await FileDataAccess.GetAsync(key);

            if (file == null)
            {
                return NotFound();
            }

            return Ok(new File(file));
        }

        public async Task<IHttpActionResult> GetValue([FromODataUri] string key)
        {
            var fileStream = await FileDataAccess.GetStreamAsync(key);

            if (fileStream == null)
            {
                return NotFound();
            }

            var range = Request.Headers.Range;

            if (range == null)
            {
                // if the range header is present but null, then the header value must be invalid
                if (Request.Headers.Contains("Range"))
                {
                    return StatusCode(HttpStatusCode.RequestedRangeNotSatisfiable);
                }

                // if no range was requested, return the entire stream
                var response = Request.CreateResponse(HttpStatusCode.OK);

                response.Headers.AcceptRanges.Add("bytes");
                response.Content = new StreamContent(fileStream);
                response.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(fileStream.MediaType);

                return ResponseMessage(response);
            }
            else
            {
                if (!fileStream.CanSeek)
                {
                    return StatusCode(HttpStatusCode.NotImplemented);
                }

                var response = Request.CreateResponse(HttpStatusCode.PartialContent);
                response.Headers.AcceptRanges.Add("bytes");

                try
                {
                    // return the requested range(s)
                    response.Content = new ByteRangeStreamContent(fileStream, range, fileStream.MediaType);
                }
                catch (InvalidByteRangeException)
                {
                    response.Dispose();
                    throw;
                }

                // change status code if the entire stream was requested
                if (response.Content.Headers.ContentLength.Value == fileStream.Length)
                {
                    response.StatusCode = HttpStatusCode.OK;
                }

                return ResponseMessage(response);
            }
        }

        public async Task<IHttpActionResult> Post()
        {
            if (!Request.Content.Headers.ContentLength.HasValue || Request.Content.Headers.ContentLength.Value <= 0)
            {
                return BadRequest();
            }

            var contentTypeHeader = Request.Content.Headers.ContentType;

            if (contentTypeHeader == null || contentTypeHeader.MediaType == null)
            {
                return BadRequest();
            }

            var identifier = Guid.NewGuid().ToString("N").ToLowerInvariant();
            var mediaType = contentTypeHeader.MediaType;

            var stream = await Request.Content.ReadAsStreamAsync();

            var file = await FileDataAccess.CreateAsync(identifier, null, mediaType, stream);

            return Created(new File(file));
        }

        public async Task<IHttpActionResult> Put([FromODataUri] string key, File file)
        {
            if (file == null)
            {
                return BadRequest();
            }

            try
            {
                file.Id = key;

                await FileDataAccess.UpdateAsync(file);

                return Updated(file);
            }
            catch (ResourceNotFoundException)
            {
                return NotFound();
            }
        }

        public async Task<IHttpActionResult> Patch([FromODataUri] string key, Delta<File> fileDelta)
        {
            if (fileDelta == null)
            {
                return BadRequest();
            }

            try
            {
                var fileMetadata = await FileDataAccess.GetAsync(key);
                var file = new File(fileMetadata);

                fileDelta.Patch(file);

                file.Id = key;

                await FileDataAccess.UpdateAsync(file);

                return Updated(file);
            }
            catch (ResourceNotFoundException)
            {
                return NotFound();
            }
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

                if (!Request.Content.Headers.ContentLength.HasValue)
                {
                    return StatusCode(HttpStatusCode.LengthRequired);
                }

                if (Request.Content.Headers.ContentLength.Value <= 0)
                {
                    return BadRequest();
                }

                var mediaType = contentTypeHeader.MediaType;

                var stream = await Request.Content.ReadAsStreamAsync();

                await FileDataAccess.UpdateStreamAsync(key, mediaType, stream);

                return StatusCode(HttpStatusCode.NoContent);
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
                await FileDataAccess.DeleteAsync(key);

                return StatusCode(HttpStatusCode.NoContent);
            }
            catch (ResourceNotFoundException)
            {
                return NotFound();
            }
        }

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);

            Request.SetMediaStreamReferenceProvider(new DefaultMediaStreamReferenceProvider());
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

        /// <summary>Creates an action result with the specified values that is a response to a PUT, PATCH, or a MERGE operation on an OData entity.</summary>
        /// <returns>An <see cref="T:System.Web.OData.Results.UpdatedODataResult`1" /> with the specified values.</returns>
        /// <param name="entity">The updated entity.</param>
        /// <typeparam name="TEntity">The updated entity type.</typeparam>
        private UpdatedODataResult<TEntity> Updated<TEntity>(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            return new UpdatedODataResult<TEntity>(entity, this);
        }
    }
}