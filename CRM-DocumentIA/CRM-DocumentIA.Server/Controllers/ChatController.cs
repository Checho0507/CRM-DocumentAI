using CRM_DocumentIA.Server.Application.DTOs.Chat;
using CRM_DocumentIA.Server.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace CRM_DocumentIA.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly ChatService _chatService;

        public ChatController(ChatService chatService)
        {
            _chatService = chatService;
        }

        // POST api/chat
        [HttpPost]
        public async Task<IActionResult> CreateChat([FromBody] CreateChatDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var chat = await _chatService.CreateChatAsync(dto);
            return Ok(chat);
        }

        // GET api/chat/user/5
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetChatsByUser(int userId)
        {
            Console.WriteLine($"userId recibido: {userId}");

            var chats = await _chatService.GetChatsByUserAsync(userId);
            return Ok(chats);
        }

        // GET api/chat/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetChatDetail(int id)
        {
            var chat = await _chatService.GetChatDetailAsync(id);
            if (chat == null)
                return NotFound(new { message = "Chat not found" });

            return Ok(chat);
        }

        // DELETE api/chat/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChat(int id)
        {
            var deleted = await _chatService.DeleteChatAsync(id);
            if (!deleted)
                return NotFound(new { message = "Chat not found" });

            return Ok(new { message = "Chat deleted successfully" });
        }
    }
}
