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

        [HttpPost("rentbook")]
        public async Task<ActionResult<ApiResponse>> RentBook([FromBody] RentBookRequest request)
        {
            var member = await _ctx.Members
                .FirstOrDefaultAsync(m => m.MemberEmail == request.MemberEmail);

            if (member == null)
            {
                return BadRequest("Üye bulunamadı! Lütfen önce kayıt olun.");
            }

            if (member == null)
                return new ApiResponse { Success = false, Message = "Üye bulunamadı." };

            var book = await _ctx.Books
                .FirstOrDefaultAsync(b => b.KitapAdi == request.BookTitle);

            if (book == null)
                return new ApiResponse { Success = false, Message = "Kitap bulunamadı." };

            var availableCopy = await _ctx.BookPublishes
                .Include(bp => bp.RentBook)
                .Where(bp => bp.BookFK == book.Id)
                .FirstOrDefaultAsync(bp => bp.RentBook == null);

            if (availableCopy == null)
                return new ApiResponse { Success = false, Message = "Kitap şu anda kirada." };

            var rent = new RentBook
            {
                BookPublishFK = availableCopy.Id,
                MemberFK = member.Id
            };

            _ctx.RentBooks.Add(rent);
            await _ctx.SaveChangesAsync();

            return new ApiResponse { Success = true, Message = "Kitap başarıyla kiralandı." };
        }

        [HttpPost("returnbook")]
        public async Task<IActionResult> ReturnBook([FromBody] ReturnBookRequest request)
        {
          
            var member = await _ctx.Members
                .FirstOrDefaultAsync(m => m.MemberEmail == request.MemberEmail);

          
            if (member == null)
            {
                return BadRequest("Üye bulunamadı! Lütfen önce kayıt olun.");
            }

       
            var book = await _ctx.Books
                .FirstOrDefaultAsync(b => b.KitapAdi == request.BookTitle);

            if (book == null)
                return BadRequest("Kitap bulunamadı!");

           
            var rent = await _ctx.RentBooks
                .Include(rb => rb.BookPublish)
                .FirstOrDefaultAsync(rb => rb.MemberFK == member.Id &&
                                           rb.BookPublish.BookFK == book.Id);

            if (rent == null)
                return BadRequest("Bu üye bu kitabı kiralamamış.");

       
            _ctx.RentBooks.Remove(rent);
            await _ctx.SaveChangesAsync();

            return Ok("Kitap başarıyla iade edildi!");
        }
    }
}