using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyLibrary;
using MyLibrary.Api.Models;
using System;
using System.Security.Cryptography;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly MyDbContext _ctx;
    private readonly IWebHostEnvironment _env;

    public AdminController(MyDbContext ctx, IWebHostEnvironment env)
    {
        _ctx = ctx;
        _env = env;
    }

    private string CalculateHash(string path)
    {
        using var sha = SHA256.Create();
        var bytes = System.IO.File.ReadAllBytes(path);
        return Convert.ToBase64String(sha.ComputeHash(bytes));
    }

    private async Task<string> SaveFileToFolder(IFormFile file, string folderName)
    {
        var dir = Path.Combine(_env.WebRootPath, folderName);
        Directory.CreateDirectory(dir);

        var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
        var path = Path.Combine(dir, fileName);

        using var stream = new FileStream(path, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/{folderName}/{fileName}";
    }


    [HttpPost("addcopies")]
    public async Task<IActionResult> AddCopies(NewBookCopiesRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.BookTitle) ||
            string.IsNullOrWhiteSpace(request.AuthorName) ||
            string.IsNullOrWhiteSpace(request.PublisherName) ||
            string.IsNullOrWhiteSpace(request.ISBN) ||
            request.Quantity <= 0)
            return BadRequest("Tüm alanlar zorunludur.");

        var author = await _ctx.Authors
            .FirstOrDefaultAsync(a => a.AuthorName == request.AuthorName);

        if (author == null)
        {
            author = new Author
            {
                AuthorName = request.AuthorName

            };
            _ctx.Authors.Add(author);
        }

        var publisher = await _ctx.YayinEvis
            .FirstOrDefaultAsync(p => p.YayinEviName == request.PublisherName);

        if (publisher == null)
        {
            publisher = new YayinEvi
            {
                YayinEviName = request.PublisherName
                
            };
            _ctx.YayinEvis.Add(publisher);
        }

        var book = await _ctx.Books
            .Include(b => b.Author)
            .FirstOrDefaultAsync(b => b.ISBN == request.ISBN);

        if (book == null)
        {
            book = new Book
            {
                KitapAdi = request.BookTitle,
                ISBN = request.ISBN,
                Author = author
            };
            _ctx.Books.Add(book);
        }

        else
        {
            if (!string.Equals(book.KitapAdi, request.BookTitle, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(book.Author.AuthorName, request.AuthorName, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Bu ISBN farklı bir kitapla kayıtlı. Kitap adı veya yazar uyuşmuyor.");
            }
        }

        for (int i = 0; i < request.Quantity; i++)
        {
            var demirbas = $"{request.DemirbasPrefix}{request.StartNumber + i}";

            if (await _ctx.BookPublishes.AnyAsync(x => x.DemirbasNo == demirbas))
                return BadRequest($"Demirbaş çakışması: {demirbas}");

            _ctx.BookPublishes.Add(new BookPublish
            {
                Book = book,
                YayinEvi = publisher,
                DemirbasNo = demirbas,
                Description = request.Description,
                ImageUrl = request.ImageUrl
            });
        }

        await _ctx.SaveChangesAsync();
        return Ok("Kitap ve fiziksel kopyalar başarıyla eklendi.");
    }

    [HttpPost("upload-book-image")]
    public async Task<IActionResult> UploadBookImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Dosya seçilmedi.");

        var url = await SaveFileToFolder(file, "book-images");
        return Ok(url);
    }

    [HttpGet("catalog")]
    public async Task<IActionResult> Catalog()
    {
        var result = await _ctx.BookPublishes
            .Include(bp => bp.Book).ThenInclude(b => b.Author)
            .Include(bp => bp.YayinEvi)
            .GroupBy(bp => new
            {
                BookId = bp.Book.Id,
                PublisherId = bp.YayinEvi.Id
            })
            .Select(g => new
            {
                publishId = g.Max(x => x.Id),   

                bookId = g.Key.BookId,
                publisherId = g.Key.PublisherId,

                title = g.First().Book.KitapAdi,
                author = g.First().Book.Author.AuthorName,
                publisher = g.First().YayinEvi.YayinEviName,

                imageUrl = g.First().ImageUrl,
                description = g.First().Description,

                totalCopies = g.Count()
            })
            .ToListAsync();

        return Ok(result);
    }

    [HttpGet("book-publish-detail")]
    public async Task<IActionResult> GetBookPublishDetail(int publishId)
    {
        var publish = await _ctx.BookPublishes
            .Include(bp => bp.Book).ThenInclude(b => b.Author)
            .Include(bp => bp.YayinEvi)
            .FirstOrDefaultAsync(bp => bp.Id == publishId);

        if (publish == null)
            return NotFound();

        var totalCount = await _ctx.BookPublishes
            .CountAsync(bp => bp.BookFK == publish.BookFK);

        return Ok(new
        {
            title = publish.Book.KitapAdi,
            author = publish.Book.Author.AuthorName,
            publisher = publish.YayinEvi.YayinEviName,
            description = publish.Description,
            imageUrl = publish.ImageUrl,
            totalCount = totalCount
        });
    }

    [HttpPost("deletecopies")]
    public async Task<IActionResult> DeleteCopies(DeleteCopiesRequest request)
    {
        using var tx = await _ctx.Database.BeginTransactionAsync();

        try
        {
            List<string> demirbasList = new();

            if (request.Mode == "list")
            {
                demirbasList = request.Value
                    .Split(',')
                    .Select(x => $"{request.DemirbasPrefix}{x.Trim()}")
                    .ToList();
            }
            else if (request.Mode == "range")
            {
                var parts = request.Value.Split('-');
                if (parts.Length != 2)
                    return BadRequest("Aralık formatı hatalı.");

                int start = int.Parse(parts[0]);
                int end = int.Parse(parts[1]);

                for (int i = start; i <= end; i++)
                    demirbasList.Add($"{request.DemirbasPrefix}{i}");
            }
            else
            {
                return BadRequest("Geçersiz silme modu.");
            }

            var copies = await _ctx.BookPublishes
                .Where(bp => demirbasList.Contains(bp.DemirbasNo))
                .ToListAsync();

            if (copies.Count != demirbasList.Count)
                return BadRequest("Bazı kitap kopyaları bulunamadı. İşlem iptal edildi.");

            var rented = await _ctx.RentBooks
                .AnyAsync(rb => demirbasList.Contains(rb.DemirbasNo));

            if (rented)
                return BadRequest("Bazı kitaplar kiralanmış. İşlem iptal edildi.");

            _ctx.BookPublishes.RemoveRange(copies);
            await _ctx.SaveChangesAsync();

            await tx.CommitAsync();
            return Ok("Seçilen kitap kopyaları başarıyla silindi.");
        }
        catch
        {
            await tx.RollbackAsync();
            return BadRequest("Silme işlemi sırasında hata oluştu.");
        }
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(string book, string author, string publisher)
    {
        var query = _ctx.BookPublishes
            .Include(bp => bp.Book).ThenInclude(b => b.Author)
            .Include(bp => bp.YayinEvi)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(book))
            query = query.Where(bp => bp.Book.KitapAdi.Contains(book));

        if (!string.IsNullOrWhiteSpace(author))
            query = query.Where(bp => bp.Book.Author.AuthorName.Contains(author));

        if (!string.IsNullOrWhiteSpace(publisher))
            query = query.Where(bp => bp.YayinEvi.YayinEviName.Contains(publisher));

        var result = await query
            .GroupBy(bp => new
            {
                bp.Book.KitapAdi,
                Author = bp.Book.Author.AuthorName,
                Publisher = bp.YayinEvi.YayinEviName
            })
            .Select(g => new SearchResultDto
            {
                Title = g.Key.KitapAdi,
                Author = g.Key.Author,
                Publisher = g.Key.Publisher,
                TotalCount = g.Count()
            })
            .ToListAsync();

        return Ok(result);
    }

    [HttpGet("authors")]
    public async Task<IActionResult> GetAuthors()
    {
        var authors = await _ctx.Authors
            .OrderBy(a => a.AuthorName)
            .Select(a => new
            {
                id = a.Id,
                authorName = a.AuthorName,
                photoUrl = a.PhotoUrl
            })
            .ToListAsync();

        return Ok(authors);
    }


    [HttpPost("upload-author-image")]
    public async Task<IActionResult> UploadAuthorImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Dosya seçilmedi.");

        var url = await SaveFileToFolder(file, "authors");
        return Ok(url);
    }

    [HttpPost("authors/add-photo")]
    public async Task<IActionResult> AddAuthorPhoto([FromBody] AuthorPhotoDto dto)
    {
        var author = await _ctx.Authors.FindAsync(dto.AuthorId);
        if (author == null) return NotFound("Yazar bulunamadı.");

        if (!string.IsNullOrWhiteSpace(author.PhotoUrl))
            return BadRequest("Bu yazarda zaten fotoğraf var. Güncelle kullan.");

        author.PhotoUrl = dto.PhotoUrl;
        await _ctx.SaveChangesAsync();
        return Ok("Fotoğraf eklendi.");
    }

    public class AuthorPhotoDto
    {
        public int AuthorId { get; set; }
        public string PhotoUrl { get; set; } = "";
    }

    [HttpPut("authors/update-photo")]
    public async Task<IActionResult> UpdateAuthorPhoto([FromBody] AuthorPhotoDto dto)
    {
        var author = await _ctx.Authors.FindAsync(dto.AuthorId);
        if (author == null) return NotFound("Yazar bulunamadı.");

        author.PhotoUrl = dto.PhotoUrl;
        await _ctx.SaveChangesAsync();
        return Ok("Fotoğraf güncellendi.");
    }

    [HttpDelete("authors/delete-photo/{authorId}")]
    public async Task<IActionResult> DeleteAuthorPhoto(int authorId)
    {
        var author = await _ctx.Authors.FindAsync(authorId);
        if (author == null) return NotFound("Yazar bulunamadı.");

        author.PhotoUrl = null;
        await _ctx.SaveChangesAsync();
        return Ok("Fotoğraf silindi.");
    }
    [HttpGet("catalog/by-author")]
    public async Task<IActionResult> CatalogByAuthor(int authorId)
    {
        var result = await _ctx.BookPublishes
            .Include(bp => bp.Book).ThenInclude(b => b.Author)
            .Include(bp => bp.YayinEvi)
            .Where(bp => bp.Book.AuthorId == authorId)
            .GroupBy(bp => new
            {
                BookId = bp.Book.Id,
                PublisherId = bp.YayinEvi.Id
            })
            .Select(g => new
            {
                bookId = g.Key.BookId,

                title = g.First().Book.KitapAdi,
                author = g.First().Book.Author.AuthorName,
                publisher = g.First().YayinEvi.YayinEviName,

                imageUrl = g.First().ImageUrl,
                description = g.First().Description,

                totalCopies = g.Count()
            })
            .ToListAsync();

        return Ok(result);
    }

    [HttpGet("publisher/book-detail")]
    public async Task<IActionResult> PublisherBookDetail(int publisherId, int bookId)
    {
        var result = await _ctx.BookPublishes
            .Include(bp => bp.Book)
                .ThenInclude(b => b.Author)
            .Include(bp => bp.YayinEvi)
            .Where(bp => bp.YayinEviFK == publisherId && bp.BookFK == bookId)
            .GroupBy(bp => new
            {
                bp.Book.Id,
                bp.Book.KitapAdi,
                Author = bp.Book.Author.AuthorName,
                Publisher = bp.YayinEvi.YayinEviName,
                bp.ImageUrl,
                bp.Description
            })
            .Select(g => new
            {
                bookId = g.Key.Id,
                title = g.Key.KitapAdi,
                author = g.Key.Author,
                publisher = g.Key.Publisher,
                imageUrl = g.Key.ImageUrl,
                description = g.Key.Description,
                totalCopies = g.Count()
            })
            .FirstOrDefaultAsync();

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpGet("publishers")]
    public async Task<IActionResult> GetPublishers()
    {
        var publishers = await _ctx.YayinEvis
            .Where(y => y.BookPublishes.Any())
            .OrderBy(y => y.YayinEviName)
            .Select(y => new
            {
                id = y.Id,
                publisherName = y.YayinEviName,
                photoUrl = y.PhotoUrl
            })
            .ToListAsync();

        return Ok(publishers);
    }

    [HttpPost("upload-publisher-image")]
    public async Task<IActionResult> UploadPublisherImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Dosya seçilmedi.");

        var url = await SaveFileToFolder(file, "publishers");
        return Ok(url);
    }

    [HttpPost("publishers/add-photo")]
    public async Task<IActionResult> AddPublisherPhoto([FromBody] PublisherPhotoDto dto)
    {
        var publisher = await _ctx.YayinEvis.FindAsync(dto.PublisherId);
        if (publisher == null)
            return NotFound("Yayınevi bulunamadı.");

        if (!string.IsNullOrWhiteSpace(publisher.PhotoUrl))
            return BadRequest("Bu yayınevinde zaten fotoğraf var.");

        publisher.PhotoUrl = dto.PhotoUrl;
        await _ctx.SaveChangesAsync();
        return Ok("Fotoğraf eklendi.");
    }

   

    [HttpPost("publishers/update-photo")]
    public async Task<IActionResult> UpdatePublisherPhoto([FromBody] PublisherPhotoDto dto)
    {
        var publisher = await _ctx.YayinEvis.FindAsync(dto.PublisherId);
        if (publisher == null)
            return NotFound("Yayınevi bulunamadı.");

        publisher.PhotoUrl = dto.PhotoUrl;
        await _ctx.SaveChangesAsync();
        return Ok("Fotoğraf güncellendi.");
    }


    [HttpDelete("publishers/delete-photo/{publisherId}")]
    public async Task<IActionResult> DeletePublisherPhoto(int publisherId)
    {
        var publisher = await _ctx.YayinEvis.FindAsync(publisherId);
        if (publisher == null)
            return NotFound("Yayınevi bulunamadı.");

        publisher.PhotoUrl = null;
        await _ctx.SaveChangesAsync();
        return Ok("Fotoğraf silindi.");
    }

    [HttpGet("catalog/by-publisher")]
    public async Task<IActionResult> CatalogByPublisher(int publisherId)
    {
        var result = await _ctx.BookPublishes
            .Include(bp => bp.Book).ThenInclude(b => b.Author)
            .Include(bp => bp.YayinEvi)
            .Where(bp => bp.YayinEviFK == publisherId)
            .GroupBy(bp => new
            {
                BookId = bp.Book.Id,
                PublisherId = bp.YayinEvi.Id
            })
            .Select(g => new
            {
                bookId = g.Key.BookId,

                title = g.First().Book.KitapAdi,
                author = g.First().Book.Author.AuthorName,
                publisher = g.First().YayinEvi.YayinEviName,

                imageUrl = g.First().ImageUrl,
                description = g.First().Description,

                totalCopies = g.Count()
            })
            .ToListAsync();

        return Ok(result);
    }

    [HttpGet("book-by-author-detail")]
    public async Task<IActionResult> BookByAuthorDetail(int bookId, int publisherId)
    {
        var publish = await _ctx.BookPublishes
            .Include(bp => bp.Book).ThenInclude(b => b.Author)
            .Include(bp => bp.YayinEvi)
            .Where(bp => bp.Book.Id == bookId && bp.YayinEvi.Id == publisherId)
            .FirstOrDefaultAsync();

        if (publish == null)
            return NotFound("Kitap bulunamadı.");

        var totalCopies = await _ctx.BookPublishes
            .CountAsync(bp =>
                bp.Book.Id == bookId &&
                bp.YayinEvi.Id == publisherId
            );

        return Ok(new
        {
            title = publish.Book.KitapAdi,
            author = publish.Book.Author.AuthorName,
            publisher = publish.YayinEvi.YayinEviName,
            imageUrl = publish.ImageUrl,
            description = publish.Description,
            totalCopies = totalCopies
        });
    }

    public class PublisherPhotoDto
    {
        public int PublisherId { get; set; }
        public string PhotoUrl { get; set; } = "";
    }


    [HttpGet("autocomplete")]
    public async Task<IActionResult> AutoComplete([FromQuery] string q, [FromQuery] string type)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            return Ok(new List<object>());

        q = q.ToLower();

        var query = _ctx.BookPublishes
            .Include(bp => bp.Book).ThenInclude(b => b.Author)
            .Include(bp => bp.YayinEvi)
            .AsQueryable();

        switch (type)
        {
            case "book":
                query = query.Where(bp => bp.Book.KitapAdi.ToLower().Contains(q));
                break;
            case "author":
                query = query.Where(bp => bp.Book.Author.AuthorName.ToLower().Contains(q));
                break;
            case "publisher":
                query = query.Where(bp => bp.YayinEvi.YayinEviName.ToLower().Contains(q));
                break;
            default:
                return Ok(new List<object>());
        }

        var result = await query
            .GroupBy(bp => new
            {
                bp.Book.KitapAdi,
                Author = bp.Book.Author.AuthorName,
                Publisher = bp.YayinEvi.YayinEviName
            })
            .Select(g => new
            {
                publishId = g.Min(x => x.Id),
                title = g.Key.KitapAdi,
                author = g.Key.Author,
                publisher = g.Key.Publisher,
                totalCount = g.Count()
            })
            .OrderByDescending(x => x.totalCount)
            .Take(5)
            .ToListAsync();

        return Ok(result);
    }
}
