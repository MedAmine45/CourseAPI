using AutoMapper;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Helpers;
using CourseLibrary.API.Models;
using CourseLibrary.API.ResourceParameters;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CourseLibrary.API.Controllers
{

    /// <summary>
    /// Improving our architecture 
    /// IActionResult vs ActionResult
    /// Working with AutoMapper 
    /// Parent/child relationships
    /// Dealing with faults
    /// Supporting Head
    /// 
    /// 
    /// Filtering : Filtering a collection means limiting the collection taking into account a predicate 
    /// Searching : Searching a collection means adding matching items to the collection based on a predefined set of rules 
    /// 
    /// put is for full updates 
    /// All resource fields are either
    ///overwritten or set  to their default values
    ///patch is for partial updates
    ///Allows sending over change sets via JsonPatchDocument 
    /// </summary>
    [Route("api/authors")]
    [ApiController]
    public class AuthorsController : ControllerBase
    {
        private readonly ICourseLibraryRepository _courseLibraryRepository;
        private readonly IMapper _mapper;
        public AuthorsController(ICourseLibraryRepository courseLibraryRepository ,
            IMapper mapper)
        {
            _courseLibraryRepository = courseLibraryRepository ??
                throw new ArgumentNullException(nameof(courseLibraryRepository));
            _mapper = mapper ??
               throw new ArgumentNullException(nameof(mapper));
        }
        [HttpGet()]
     
        public ActionResult<IEnumerable<AuthorDto>> GetAuthors()
        {
            //throw new Exception("test exception");
            var authorsFromRespo = _courseLibraryRepository.GetAuthors();
            // var authors = new List<AuthorDto>();

            return Ok(_mapper.Map<IEnumerable<AuthorDto>>(authorsFromRespo));

        }
        [HttpGet("authorsResourceParameters")]
        [HttpHead]
        public ActionResult<IEnumerable<AuthorDto>> GetAuthors([FromQuery] AuthorsResourceParameters authorsResourceParameters)
        {
            //throw new Exception("test exception");
            var authorsFromRespo = _courseLibraryRepository.GetAuthors(authorsResourceParameters);
           // var authors = new List<AuthorDto>();
            
            return  Ok(_mapper.Map<IEnumerable<AuthorDto>>(authorsFromRespo));

        }
        [HttpGet("{authorId}")]
        public IActionResult GetAuthor(Guid authorId)
        {

            var authorFromRespo = _courseLibraryRepository.GetAuthor(authorId) ;

            if (authorFromRespo == null)
            {
                return NotFound();
            }
            return Ok(_mapper.Map<AuthorDto>(authorFromRespo));

        }

        [HttpPost]
        public ActionResult<AuthorDto> CreateAuthor(AuthorForCreationDto author)
        {
            if( author == null)
            {
                return BadRequest();
            }
            var authorEntity = _mapper.Map<Entities.Author>(author);
            _courseLibraryRepository.AddAuthor(authorEntity);
            _courseLibraryRepository.Save();

            var authorToReturn = _mapper.Map<AuthorDto>(authorEntity);
            return CreatedAtRoute("GetAuthor",
                new { authorId = authorToReturn.Id },
                authorToReturn);
        }

        [HttpOptions]
        public IActionResult GetAuthorsOptions()
        {
            Response.Headers.Add("Allow", "GET,OPTIONS,POST");
            return Ok();
        }

        [HttpDelete("{authorId}")]
        public ActionResult DeleteAuthor(Guid authorId)
        {
            var authorFromRepo = _courseLibraryRepository.GetAuthor(authorId);

            if (authorFromRepo == null)
            {
                return NotFound();
            }

            _courseLibraryRepository.DeleteAuthor(authorFromRepo);

            _courseLibraryRepository.Save();

            return NoContent();
        }


    }
}
