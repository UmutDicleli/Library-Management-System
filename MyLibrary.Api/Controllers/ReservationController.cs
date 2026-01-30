using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyLibrary;

namespace MyLibrary.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationController : ControllerBase
    {
        private readonly MyDbContext _ctx;

        public ReservationController(MyDbContext ctx)
        {
            _ctx = ctx;
        }

        [HttpPost("reserve")]
        public async Task<IActionResult> Reserve(
    string memberEmail,
    string isbn,
    string demirbasNo)
        {
            var member = await _ctx.Members
                .FirstOrDefaultAsync(m => m.MemberEmail == memberEmail);

            if (member == null)
                return BadRequest("Üye bulunamadı.");

            var demirbas = demirbasNo.StartsWith("DB-")
                ? demirbasNo
                : "DB-" + demirbasNo;

            var copy = await _ctx.BookPublishes
                .Include(bp => bp.Book)
                .Include(bp => bp.RentBook)
                .Include(bp => bp.Reservations)
                .FirstOrDefaultAsync(bp =>
                    bp.Book.ISBN == isbn &&
                    bp.DemirbasNo == demirbas);

            if (copy == null)
                return BadRequest("Kitap kopyası bulunamadı.");

           
            if (copy.RentBook == null)
                return BadRequest("Kitap boş. Rezervasyon yapılamaz.");

           
            var alreadyReserved = await _ctx.Reservations.AnyAsync(r =>
                r.MemberFK == member.Id &&
                r.BookPublishFK == copy.Id &&
                r.IsActive);

            if (alreadyReserved)
                return BadRequest("Bu kitabı zaten rezerve etmişsiniz.");

            _ctx.Reservations.Add(new Reservation
            {
                MemberFK = member.Id,
                BookPublishFK = copy.Id,
                CreatedAt = DateTime.Now,
                IsActive = true
            });

            await _ctx.SaveChangesAsync();
            return Ok("Rezervasyon alındı.");
        }

        [HttpPost("cancel")]
        public async Task<IActionResult> Cancel(
            string memberEmail,
            string demirbasNo)
        {
            var demirbas = demirbasNo.StartsWith("DB-")
                ? demirbasNo
                : "DB-" + demirbasNo;

            var reservation = await _ctx.Reservations
                .Include(r => r.Member)
                .Include(r => r.BookPublish)
                .FirstOrDefaultAsync(r =>
                    r.Member.MemberEmail == memberEmail &&
                    r.BookPublish.DemirbasNo == demirbas &&
                    r.IsActive);

            if (reservation == null)
                return BadRequest("Aktif rezervasyon bulunamadı.");

            _ctx.Reservations.Remove(reservation);
            await _ctx.SaveChangesAsync();

            return Ok("Rezervasyon iptal edildi.");
        }
    }
}
