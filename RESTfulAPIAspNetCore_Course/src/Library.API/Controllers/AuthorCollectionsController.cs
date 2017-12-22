using Library.API.Entities;
using Library.API.Helpers;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.API.Controllers
{
    [Route("api/authorcollections")]
    public class AuthorCollectionsController : Controller
    {
        private ILibraryRepository _libraryRepository;

        public AuthorCollectionsController(ILibraryRepository libraryRepository)
        {
            _libraryRepository = libraryRepository;
        }

        [HttpPost]
        public IActionResult CreateAuthorCollection(
            [FromBody] IEnumerable<AuthorInputDto> authorCollection)
        {
            if (authorCollection == null)
                return BadRequest();

            var authors = AutoMapper.Mapper.Map<IEnumerable<Author>>(authorCollection);

            foreach (var author in authors)
            {
                _libraryRepository.AddAuthor(author);
            }

            if (!_libraryRepository.Save())
                throw new Exception("Creating an author collection failed on Save.");

            var newAuthors = AutoMapper.Mapper.Map<IEnumerable<AuthorDto>>(authors);

            var ids = string.Join(",", newAuthors.Select(x => x.Id));

            return CreatedAtRoute("GetAuthorCollection",
                new { ids = ids },
                newAuthors);
        }

        //(key1, key2, etc., ...)
        [HttpGet("({ids})", Name = "GetAuthorCollection")]
        public IActionResult GetAuthorCollection(
            [ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<Guid> ids)
        {
            if (ids == null)
                return BadRequest();

            var newAthors = _libraryRepository.GetAuthors(ids);

            if (ids.Count() != newAthors.Count())
                return NotFound();

            var authorsToReturn = AutoMapper.Mapper.Map<IEnumerable<AuthorDto>>(newAthors);
            return Ok(authorsToReturn);
        }
    }
}
