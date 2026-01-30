using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyLibrary;
using MyLibrary.Api.Models;

namespace MyLibrary.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RentController : ControllerBase
    {
        private readonly MyDbContext _ctx;

        public RentController(MyDbContext ctx)
        {
            _ctx = ctx;
        }

        [HttpPost("rent-direct")]
        public async Task<IActionResult> RentDirect([FromBody] RentDirectRequest request)
        {
            var member = await _ctx.Members
                .FirstOrDefaultAsync(m => m.MemberEmail == request.MemberEmail);

            if (member == null)
                return BadRequest("Üye bulunamadı.");

            var demirbas = request.DemirbasNo.StartsWith("DB-")
                ? request.DemirbasNo
                : "DB-" + request.DemirbasNo;

            var copy = await _ctx.BookPublishes
                .Include(bp => bp.Book)
                .Include(bp => bp.RentBook)
                .Include(bp => bp.Reservations)
                .FirstOrDefaultAsync(bp =>
                    bp.Book.ISBN == request.ISBN &&
                    bp.DemirbasNo == demirbas);

            if (copy == null)
                return BadRequest("Kitap bulunamadı.");

            if (copy.RentBook != null)
                return BadRequest("Kitap şu an kiralanmış.");

            var firstReservation = copy.Reservations
                .Where(r => r.IsActive)
                .OrderBy(r => r.CreatedAt)
                .FirstOrDefault();

            if (firstReservation != null)
            {
                if (firstReservation.MemberFK != member.Id)
                    return BadRequest("Bu kitap rezerve edilmiş. Sırada değilsiniz.");

                _ctx.Reservations.Remove(firstReservation);
            }

            _ctx.RentBooks.Add(new RentBook
            {
                MemberFK = member.Id,
                BookPublishFK = copy.Id,
                MemberFirstName = member.MemberFirstName,
                MemberLastName = member.MemberLastName,
                DemirbasNo = copy.DemirbasNo
            });

            await _ctx.SaveChangesAsync();
            return Ok($"Kiralandı. Demirbaş: {copy.DemirbasNo}");
        }


        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery] string title,
            [FromQuery] string? publisher)
        {
            if (string.IsNullOrWhiteSpace(title))
                return BadRequest("Kitap adı zorunludur.");

            var query = _ctx.BookPublishes
                .Include(bp => bp.Book)
                .Include(bp => bp.YayinEvi)
                .Where(bp =>
                    bp.RentBook == null &&
                    bp.Book.KitapAdi.Contains(title)
                );

            if (!string.IsNullOrWhiteSpace(publisher))
            {
                query = query.Where(bp =>
                    bp.YayinEvi.YayinEviName.Contains(publisher));
            }

            var result = await query
                .Select(bp => new AvailableBookDto
                {
                    BookTitle = bp.Book.KitapAdi,
                    PublisherName = bp.YayinEvi.YayinEviName,
                    DemirbasNo = bp.DemirbasNo
                })
                .ToListAsync();

            return Ok(result);
        }


        [HttpPost("rent-copy")]
        public async Task<IActionResult> RentCopy([FromBody] RentCopyRequest request)
        {
            var member = await _ctx.Members
                .FirstOrDefaultAsync(m => m.MemberEmail == request.MemberEmail);

            if (member == null)
                return BadRequest("Üye bulunamadı.");

            var copy = await _ctx.BookPublishes
                .Include(bp => bp.RentBook)
                .Include(bp => bp.Reservations)
                .FirstOrDefaultAsync(bp =>
                    bp.DemirbasNo == request.DemirbasNo);

            if (copy == null)
                return BadRequest("Kitap bulunamadı.");

           
            if (copy.RentBook != null)
                return BadRequest("Kitap şu an kiralanmış.");

        
            var firstReservation = copy.Reservations
                .Where(r => r.IsActive)
                .OrderBy(r => r.CreatedAt)
                .FirstOrDefault();

            if (firstReservation != null)
            {
                if (firstReservation.MemberFK != member.Id)
                    return BadRequest("Bu kitap rezerve edilmiş. Sırada değilsiniz.");

                _ctx.Reservations.Remove(firstReservation);
            }

            _ctx.RentBooks.Add(new RentBook
            {
                MemberFK = member.Id,
                BookPublishFK = copy.Id,
                MemberFirstName = member.MemberFirstName,
                MemberLastName = member.MemberLastName,
                DemirbasNo = copy.DemirbasNo
            });

            await _ctx.SaveChangesAsync();
            return Ok($"Kiralandı. Demirbaş: {copy.DemirbasNo}");
        }


        [HttpPost("return")]
        public async Task<IActionResult> ReturnBook(ReturnBookRequest request)
        {
            var demirbas = $"{request.DemirbasPrefix}{request.DemirbasNo}";
            var rent = await _ctx.RentBooks
                .Include(r => r.BookPublish)
                .Include(r => r.Member)
                .FirstOrDefaultAsync(r =>
                    r.Member.MemberEmail == request.MemberEmail &&
                    r.BookPublish.DemirbasNo == demirbas);


            if (rent == null)
                return BadRequest("Kayıt bulunamadı.");

            _ctx.RentBooks.Remove(rent);
            await _ctx.SaveChangesAsync();

            return Ok("İade tamamlandı.");
        }

        [HttpGet("book-detail")]
        public async Task<IActionResult> BookDetail(string title)
        {
            var book = await _ctx.Books
                .Include(b => b.Author)
                .FirstOrDefaultAsync(b => b.KitapAdi == title);

            if (book == null)
                return NotFound();

            var count = await _ctx.BookPublishes
                .CountAsync(bp => bp.BookFK == book.Id);

            return Ok(new
            {
                title = book.KitapAdi,
                author = book.Author.AuthorName,

                totalCount = count
            });
        }

    }
}
