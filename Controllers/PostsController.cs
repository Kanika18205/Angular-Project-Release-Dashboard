using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ReleaseDashboard.Data;
using ReleaseDashboard.DTOs.Posts;
using ReleaseDashboard.Models;
using ReleaseDashboard.Services;
using System.Reflection;

namespace ReleaseDashboard.Controllers
{
    [ApiController]
    [Route("api/posts")]
    [Authorize] //only authorised users can access
    public class PostsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IFileService _fileService;
        private readonly INotificationService _notificationService;

        public PostsController( AppDbContext context, IFileService fileService, IWebHostEnvironment environment, INotificationService notificationService)
        {
            _context = context;
            _fileService = fileService;
            _environment = environment;
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetPosts()
        {
            var posts = await _context.Posts.Include(p => p.CreatedByUser).Include(p=> p.Attachments).OrderByDescending(p => p.CreatedAt).Select(p=> new PostResponseDto
            {
                Id = p.Id,
                Version = p.Version,
                Title = p.Title,
                ChangeLog= p.ChangeLog,
                Author = p.CreatedByUser != null ? p.CreatedByUser.Username : "Unknown",
                CreatedAt = p.CreatedAt,
                Attachments = p.Attachments.Select(a => new PostAttachmentDto
                {
                    Id = a.Id,
                    FileName = a.FileName,
                    FileUrl = a.FilePath.Replace("\\", "/"),
                    ContentType = a.ContentType,
                    FileSize = a.FileSize
                }).ToList()
            }).ToListAsync();

            return Ok(posts);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePost([FromForm] CreatePostDto dto)
        {
            Console.WriteLine($"Files received: {dto.Files?.Count}");
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if(userIdClaim == null)
            {
                return Unauthorized("Invalid Token");
            }
            int userId = int.Parse(userIdClaim.Value);

            var post = new Post
            {
                Version = dto.Version,
                Title = dto.Title,
                ChangeLog = dto.ChangeLog,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
            var uploadedFiles = await _fileService.UploadFilesAsync(dto.Files, "Releases");
            foreach (var file in uploadedFiles)
            {
                _context.PostAttachments.Add(new PostAttachment
                {
                    PostId = post.Id,
                    FileName = file.OriginalName,
                    StoredFileName = file.StoredName,
                    FilePath = file.Path,
                    ContentType = file.ContentType,
                    FileSize = file.Size
                });
            }
            await _context.SaveChangesAsync();

            var createdPost = await _context.Posts
            .Include(p => p.CreatedByUser)
            .Include(p => p.Attachments)
            .Where(p => p.Id == post.Id)
            .Select(p => new PostResponseDto
            {
                Id = p.Id,
                Version = p.Version,
                Title = p.Title,
                ChangeLog = p.ChangeLog,
                Author = p.CreatedByUser != null
                    ? p.CreatedByUser.Username
                    : "Unknown",
                CreatedAt = p.CreatedAt,
                Attachments = p.Attachments.Select(a => new PostAttachmentDto
                {
                    Id = a.Id,
                    FileName = a.FileName,
                    FileUrl = a.FilePath.Replace("\\", "/"),
                    ContentType = a.ContentType,
                    FileSize = a.FileSize
                }).ToList()
            }).FirstAsync();

            await _notificationService.NotifyReleaseCreatedAsync(post, userId);

            return Ok(createdPost);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var currentUser = await _context.Users.FindAsync(userId);
            if (post == null)
            {
                return NotFound();
            }
            if (currentUser == null)
            {
                return Unauthorized();
            }

            if (currentUser.Role != "Admin")
            {
                return Forbid();
            }
            var attachments = await _context.PostAttachments
                .Where(a => a.PostId == id)
                .ToListAsync();

            foreach (var attachment in attachments)
            {
                var fullPath = Path.Combine(
                    _environment.WebRootPath,
                    attachment.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
            }
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            return Ok(new
            {
                message = "Release Deleted Successfully!"
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePost(int id, [FromForm] CreatePostDto dto)
        {
            var post = await _context.Posts.FindAsync(id);
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var currentUser = await _context.Users.FindAsync(userId);

            if (post == null)
            {
                return NotFound();
            }

            if (currentUser == null)
            {
                return Unauthorized();
            }

            if (currentUser.Role != "Admin" &&
                post.CreatedBy != userId)
            {
                return Forbid();
            }
            post.Version = dto.Version;
            post.Title = dto.Title;
            post.ChangeLog = dto.ChangeLog;


            var oldAttachments = await _context.PostAttachments
                .Where(a => a.PostId == post.Id)
                .ToListAsync();

            foreach (var attachment in oldAttachments)
            {
                attachment.IsCurrent = false;
            }

            _context.PostAttachments.RemoveRange(oldAttachments);

            // Upload new attachments
            var uploadedFiles = await _fileService.UploadFilesAsync(dto.Files, "Releases");

            foreach (var file in uploadedFiles)
            {
                _context.PostAttachments.Add(new PostAttachment
                {
                    PostId = post.Id,
                    FileName = file.OriginalName,
                    StoredFileName = file.StoredName,
                    FilePath = file.Path,
                    ContentType = file.ContentType,
                    FileSize = file.Size,
                    IsCurrent = true
                });
            }

            await _context.SaveChangesAsync();
            await _notificationService.NotifyReleaseUpdatedAsync(post, userId);

            return Ok(new
            {
                message = "Release Updated Successfully!"
            });
        }

            [HttpGet("attachments/{attachmentId}")]
        public async Task<IActionResult> DownloadAttachment(int attachmentId)
        {
            var attachment = await _context.PostAttachments.FirstOrDefaultAsync(a => a.Id == attachmentId);

            if (attachment == null)
                return NotFound();

            var fullPath = Path.Combine(
                 _environment.WebRootPath,
                 attachment.FilePath.TrimStart('/'));

            if (!System.IO.File.Exists(fullPath))
                return NotFound();

            return PhysicalFile(
                fullPath,
                attachment.ContentType,
                attachment.FileName);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPost(int id)
        {
            var post = await _context.Posts
                .Include(p => p.CreatedByUser)
                .Include(p => p.Attachments)
                .Where(p => p.Id == id)
                .Select(p => new PostResponseDto
                {
                    Id = p.Id,
                    Version = p.Version,
                    Title = p.Title,
                    ChangeLog = p.ChangeLog,
                    Author = p.CreatedByUser != null
                        ? p.CreatedByUser.Username
                        : "Unknown",
                    CreatedAt = p.CreatedAt,

                    Attachments = p.Attachments.Where(a=> a.IsCurrent).Select(a => new PostAttachmentDto
                    {
                        Id = a.Id,
                        FileName = a.FileName,
                        FileUrl = a.FilePath.Replace("\\", "/"),
                        ContentType = a.ContentType,
                        FileSize = a.FileSize
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (post == null)
                return NotFound();

            return Ok(post);
        }
    }
}


