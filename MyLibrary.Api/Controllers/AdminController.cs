using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyLibrary;
using MyLibrary.Api.Models;

namespace MyLibrary.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly MyDbContext _ctx;

        public AdminController(MyDbContext ctx)
        {
            _ctx = ctx;
        }

        [HttpPost("newcopy")]
        public async Task<ActionResult<ApiResponse>> AddNewCopy([FromBody] NewBookCopyRequest request)
        {

            if (string.IsNullOrWhiteSpace(request.BookTitle))
                return BadRequest("Kitap adı zorunludur.");

            if (string.IsNullOrWhiteSpace(request.AuthorName))
                return BadRequest("Yazar adı zorunludur.");

            if (string.IsNullOrWhiteSpace(request.PublisherName))
                return BadRequest("Yayınevi adı zorunludur.");

         
            var author = await _ctx.Authors
                .FirstOrDefaultAsync(a => a.AuthorName == request.AuthorName);

            if (author == null)
            {
                author = new Author { AuthorName = request.AuthorName };
                _ctx.Authors.Add(author);
                await _ctx.SaveChangesAsync();
            }

        
            var publisher = await _ctx.YayinEvis
                .FirstOrDefaultAsync(p => p.YayinEviName == request.PublisherName);

            if (publisher == null)
            {
                publisher = new YayinEvi { YayinEviName = request.PublisherName };
                _ctx.YayinEvis.Add(publisher);
                await _ctx.SaveChangesAsync();
            }

      
            var book = await _ctx.Books
                .FirstOrDefaultAsync(b => b.KitapAdi == request.BookTitle &&
                                          b.AuthorId == author.Id);

            if (book == null)
            {
                book = new Book
                {
                    KitapAdi = request.BookTitle,
                    AuthorId = author.Id
                };

                _ctx.Books.Add(book);
                await _ctx.SaveChangesAsync();
            }

         
            var newCopy = new BookPublish
            {
                BookFK = book.Id,
                YayinEviFK = publisher.Id
            };

            _ctx.BookPublishes.Add(newCopy);
            await _ctx.SaveChangesAsync();

            return new ApiResponse
            {
                Success = true,
                Message = "Yeni kitap başarıyla eklendi."
            };
        }

        [HttpPost("deletecopy")]
        public async Task<IActionResult> DeleteBookCopy([FromBody] DeleteBookCopyRequest request)
        {
            var copy = await _ctx.BookPublishes
                .Include(bp => bp.Book)
                .Include(bp => bp.YayinEvi)
                .FirstOrDefaultAsync(bp => bp.Id == request.BookPublishId);

            if (copy == null)
                return BadRequest("Bu ID ile bir kitap kopyası bulunamadı.");

            var isRented = await _ctx.RentBooks
                .AnyAsync(rb => rb.BookPublishFK == copy.Id);

            if (isRented)
                return BadRequest("Bu kitap şu anda kiralanmış, silinemez.");

            var book = copy.Book;
            var authorId = book.AuthorId;
            var publisherId = copy.YayinEviFK;

            _ctx.BookPublishes.Remove(copy);
            await _ctx.SaveChangesAsync();

            var hasOtherCopies = await _ctx.BookPublishes
                .AnyAsync(bp => bp.BookFK == book.Id);

            if (!hasOtherCopies)
            {
                _ctx.Books.Remove(book);
                await _ctx.SaveChangesAsync();

                var authorHasOtherBooks = await _ctx.Books
                    .AnyAsync(b => b.AuthorId == authorId);

                if (!authorHasOtherBooks)
                {
                    var author = await _ctx.Authors.FindAsync(authorId);
                    if (author != null)
                        _ctx.Authors.Remove(author);
                }

                var publisherHasOtherBooks = await _ctx.BookPublishes
                    .AnyAsync(bp => bp.YayinEviFK == publisherId);

                if (!publisherHasOtherBooks)
                {
                    var publisher = await _ctx.YayinEvis.FindAsync(publisherId);
                    if (publisher != null)
                        _ctx.YayinEvis.Remove(publisher);
                }

                await _ctx.SaveChangesAsync();
            }

            return Ok("Kitap kopyası ve ilişkili gereksiz kayıtlar başarıyla silindi.");
        }

    }
}
