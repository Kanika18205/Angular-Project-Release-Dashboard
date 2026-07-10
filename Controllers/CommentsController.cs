using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ReleaseDashboard.Data;
using ReleaseDashboard.DTOs.Comments;
using ReleaseDashboard.Models;
using ReleaseDashboard.Services;
using System.Security.Claims;

namespace ReleaseDashboard.Controllers
{
    [ApiController]
    [Route("api/comments")]
    [Authorize]
    public class CommentsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IFileService _fileService;
        private readonly INotificationService _notificationService;


        public CommentsController(AppDbContext context, IFileService fileService, INotificationService notificationService)
        {
            _context = context;
            _fileService = fileService;
            _notificationService = notificationService;
        }

        //GET COMMENTS
        [HttpGet("post/{postId}")]
        public async Task<IActionResult> GetComments(int postId)
        {
            var comments = await _context.Comments
                .Include(c => c.User)
                .Include(c => c.Attachments)
                .Where(c => c.PostId == postId)
                .OrderBy(c => c.CreatedAt)
                .Select(c => new CommentResponseDto
                {
                    Id = c.Id,
                    Text = c.Text,
                    Author = c.User != null ? c.User.Username : "Unknown",
                    CreatedAt = c.CreatedAt,
                    Attachments = c.Attachments.Select(a => new CommentAttachmentDto
                    {
                        Id = a.Id,
                        FileName = a.FileName,
                        FileUrl = a.FilePath.Replace("\\", "/"),
                        ContentType = a.ContentType,
                        FileSize = a.FileSize,
                    }).ToList()
                }).ToListAsync();
            return Ok(comments);
        }

        //CREATE COMMENT
        [HttpPost]
        public async Task<IActionResult> CreateComment([FromForm] CreateCommentDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Text) && (dto.Files == null || dto.Files.Count == 0))
            {
                return BadRequest("A comment must contain text or at least one attachment.");
            }

            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if(claim == null)
            {
                return Unauthorized();
            }
            int userId = int.Parse(claim.Value);
            var comment = new Comment
            {
                Text = dto.Text,
                PostId = dto.PostId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            var uploadedFiles = await _fileService.UploadFilesAsync(dto.Files, "Comments");

            foreach (var file in uploadedFiles)
            {
                _context.CommentAttachments.Add(
                    new CommentAttachment
                    {
                        CommentId = comment.Id,
                        FileName = file.OriginalName,
                        StoredFileName = file.StoredName,
                        FilePath = file.Path,
                        ContentType = file.ContentType,
                        FileSize = file.Size
                    });
            }
            await _context.SaveChangesAsync();
            await _notificationService.NotifyCommentAddedAsync(comment,userId);
            var createdComment = await _context.Comments
                .Include(c => c.User)
                .Include(c => c.Attachments)
                .Where(c => c.Id == comment.Id)
                .Select(c => new CommentResponseDto
                {
                    Id = c.Id,
                    Text = c.Text,
                    Author = c.User != null ? c.User.Username : "Unknown",
                    CreatedAt = c.CreatedAt,
                    Attachments = c.Attachments.Select(a => new CommentAttachmentDto
                    {
                        Id = a.Id,
                        FileName = a.FileName,
                        FileUrl = a.FilePath.Replace("\\", "/"),
                        ContentType = a.ContentType,
                        FileSize = a.FileSize,
                    }).ToList()
                }).FirstAsync();
            return Ok(createdComment);
        }

        //DELETE COMMENTS
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await _context.Comments
                .Include(c => c.Attachments)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comment == null)
                return NotFound();
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            foreach (var attachment in comment.Attachments)
            {
                var fullPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    attachment.FilePath);

                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
            }

            await _notificationService.NotifyCommentDeletedAsync(comment,userId);
            _context.Comments.Remove(comment);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Comment deleted successfully."
            });
        }

        //DOWNLOAD
        [HttpGet("attachments/{attachmentId}")]
        public async Task<IActionResult> DownloadAttachment(int attachmentId)
        {
            var attachment = await _context.CommentAttachments.FirstOrDefaultAsync(a => a.Id == attachmentId);

            if (attachment == null)
                return NotFound();

            var fullPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                attachment.FilePath);

            if (!System.IO.File.Exists(fullPath))
                return NotFound();

            return PhysicalFile(
                fullPath,
                attachment.ContentType,
                attachment.FileName);
        }

    }
}
