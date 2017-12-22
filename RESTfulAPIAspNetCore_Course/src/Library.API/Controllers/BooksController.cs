using Library.API.Entities;
using Library.API.Helpers;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.API.Controllers
{
    [Route("api/authors/{authorId}/books")]
    public class BooksController : Controller
    {
        private ILibraryRepository _libraryRepository;

        public BooksController(ILibraryRepository libraryRepository)
        {
            _libraryRepository = libraryRepository;
        }

        [HttpGet()]
        public IActionResult GetBooksForAuthor(Guid authorId)
        {
            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var booksForAuthorFromRepo = _libraryRepository.GetBooksForAuthor(authorId);

            var booksForAuthor = AutoMapper.Mapper.Map<IEnumerable<BookDto>>(booksForAuthorFromRepo);

            return Ok(booksForAuthor);
        }

        [HttpGet("{id}", Name = "GetBookForAuthor")]
        public IActionResult GetBookForAuthor(Guid authorId, Guid id)
        { 
            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookForAuthorFromRepo = _libraryRepository.GetBookForAuthor(authorId, id);

            if (bookForAuthorFromRepo == null)
                return NotFound();

            var bookForAuthor = AutoMapper.Mapper.Map<BookDto>(bookForAuthorFromRepo);

            return Ok(bookForAuthor);
        }

        [HttpPost]
        public IActionResult CreateBookForAuthor(Guid authorId,
            [FromBody] BookInputDto book)
        {
            if (book == null)
                return BadRequest();

            if (book.Description == book.Title)
            {
                ModelState.AddModelError(nameof(BookInputDto),
                    "The provided description should be different from the title.");
            }

            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);

            if (!_libraryRepository.AuthorExists(authorId))
                return NotFound();

            var newBook = AutoMapper.Mapper.Map<Book>(book);

            _libraryRepository.AddBookForAuthor(authorId, newBook);

            if (!_libraryRepository.Save())
                throw new Exception($"Creating a book for auhtor {authorId} failed on save.");

            var bookToReturn = AutoMapper.Mapper.Map<BookDto>(newBook);

            return CreatedAtRoute("GetBookForAuthor",
                new { authorId = authorId, id = bookToReturn.Id },
                bookToReturn);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteBookForAuthor(Guid authorId, Guid id)
        {
            if (!_libraryRepository.AuthorExists(authorId))
                return NotFound();

            var bookForAuthorFromRepo = _libraryRepository.GetBookForAuthor(authorId, id);

            if (bookForAuthorFromRepo == null)
                return NotFound();

            _libraryRepository.DeleteBook(bookForAuthorFromRepo);

            if (!_libraryRepository.Save())
                throw new Exception($"Deleting book {id} for author {authorId} failed on save.");

            return NoContent();
        }

        [HttpPut("{id}")]
        public IActionResult UpdateBookForAuthor(Guid authorId, Guid id, [FromBody] BookUpdateDto book)
        {
            if (book == null)
                return BadRequest();

            if (book.Description == book.Title)
            {
                ModelState.AddModelError(nameof(BookUpdateDto),
                    "The provided description should be different from the title.");
            }

            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);

            if (!_libraryRepository.AuthorExists(authorId))
                return NotFound();

            var bookForAuthorFromRepo = _libraryRepository.GetBookForAuthor(authorId, id);
            if (bookForAuthorFromRepo == null)
            {
                var bookToadd = AutoMapper.Mapper.Map<Book>(book);
                bookToadd.Id = id;

                _libraryRepository.AddBookForAuthor(authorId, bookToadd);

                if (!_libraryRepository.Save())
                    throw new Exception($"Upserting the book {id} for an author {authorId} failed on save.");

                var bookToReturn = AutoMapper.Mapper.Map<BookDto>(bookToadd);

                return CreatedAtRoute("GetBookForAuthor",
                    new { authorId = authorId, id = bookToReturn.Id },
                    bookToReturn);
            }
                

            AutoMapper.Mapper.Map(book, bookForAuthorFromRepo);

            _libraryRepository.UpdateBookForAuthor(bookForAuthorFromRepo);

            if (!_libraryRepository.Save())
                throw new Exception($"Updating book {id} for author {authorId} failed on Save.");

            return NoContent();
        }

        [HttpPatch("{id}")]
        public IActionResult PartialUpdateBookForAuthor(Guid authorId, Guid id,
            [FromBody] JsonPatchDocument<BookUpdateDto> patchDoc)
        {
            if (patchDoc == null)
                return BadRequest();

            if (!_libraryRepository.AuthorExists(authorId))
                return NotFound();

            var bookForAuthorFromRepo = _libraryRepository.GetBookForAuthor(authorId, id);
            if (bookForAuthorFromRepo == null)
            {
                var bookDto = new BookUpdateDto();
                patchDoc.ApplyTo(bookDto);

                var bookToAdd = AutoMapper.Mapper.Map<Book>(bookDto);
                bookToAdd.Id = id;

                _libraryRepository.AddBookForAuthor(authorId, bookToAdd);
                if (!_libraryRepository.Save())
                    throw new Exception($"Upserting the book {id} for an author {authorId} failed on save.");

                var bookToReturn = AutoMapper.Mapper.Map<BookDto>(bookToAdd);

                return CreatedAtRoute("GetBookForAuthor",
                    new { authorId = authorId, id = bookToReturn.Id },
                    bookToReturn);
            }

            var bookToPatch = AutoMapper.Mapper.Map<BookUpdateDto>(bookForAuthorFromRepo);

            patchDoc.ApplyTo(bookToPatch);

            //add validation

            AutoMapper.Mapper.Map(bookToPatch, bookForAuthorFromRepo);

            _libraryRepository.UpdateBookForAuthor(bookForAuthorFromRepo);

            if (!_libraryRepository.Save())
                throw new Exception($"Patching book {id} for author {authorId} failed on Save.");

            return NoContent();
        }
    }
}
