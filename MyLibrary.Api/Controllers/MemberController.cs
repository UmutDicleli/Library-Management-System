using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyLibrary;

namespace MyLibrary.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MemberController : ControllerBase
    {
        private readonly MyDbContext _context;

        public MemberController(MyDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] Member member)
        {
            if (string.IsNullOrWhiteSpace(member.MemberFirstName))
                return BadRequest("FirstName boş olamaz");

            if (string.IsNullOrWhiteSpace(member.MemberLastName))
                return BadRequest("Soyad boş olamaz!");

            if (string.IsNullOrWhiteSpace(member.MemberEmail))
                return BadRequest("Email boş olamaz!");

            if (string.IsNullOrWhiteSpace(member.MemberPhoneNumber))
                return BadRequest("Telefon numarası boş olamaz!");

            if (_context.Members.Any(m => m.MemberEmail == member.MemberEmail))
                return Conflict("Bu email ile bir üye zaten var!");

            if (_context.Members.Any(m => m.MemberPhoneNumber == member.MemberPhoneNumber))
                return Conflict("Bu telefon numarası ile bir üye zaten var!");

            _context.Members.Add(member);
            _context.SaveChanges();

            return Ok("Üye başarıyla kaydedildi!");
        }

        [HttpDelete("delete")]
        public IActionResult DeleteMember([FromQuery] string email)
        {
            var member = _context.Members
                .Include(m => m.RentedBooks)
                .FirstOrDefault(m => m.MemberEmail == email);

            if (member == null)
                return BadRequest("Üye bulunamadı!");

            if (member.RentedBooks.Any())
                return BadRequest("Üyenin üzerinde kiralanmış kitaplar var. Önce iade etmelisiniz!");

            _context.Members.Remove(member);
            _context.SaveChanges();

            return Ok("Üye başarıyla silindi.");
        }

    }
}
