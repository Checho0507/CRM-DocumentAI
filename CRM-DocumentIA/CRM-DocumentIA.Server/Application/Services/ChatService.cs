using CRM_DocumentIA.Server.Application.DTOs.Chat;
using CRM_DocumentIA.Server.Application.DTOs.InsightsHisto;
using CRM_DocumentIA.Server.Domain.Entities;
using CRM_DocumentIA.Server.Domain.Interfaces.Repositories;

namespace CRM_DocumentIA.Server.Application.Services
{
    public class ChatService
    {
        private readonly IChatRepository _chatRepository;

        public ChatService(IChatRepository chatRepository)
        {
            _chatRepository = chatRepository;
        }

        public async Task<ChatDto> CreateChatAsync(CreateChatDto dto)
        {
            var newChat = new Chat
            {
                UserId = dto.UserId,
                Title = dto.Title ?? $"Nuevo chat - {DateTime.UtcNow:yyyy-MM-dd HH:mm}"
            };

            await _chatRepository.CreateAsync(newChat);
            await _chatRepository.SaveChangesAsync();

            return new ChatDto
            {
                Id = newChat.Id,
                UserId = newChat.UserId,
                Title = newChat.Title,
                CreatedAt = newChat.CreatedAt
            };
        }

        public async Task<IEnumerable<ChatDto>> GetChatsByUserAsync(int userId)
        {
            var chats = await _chatRepository.GetByUserAsync(userId);

            return chats.Select(c => new ChatDto
            {
                Id = c.Id,
                Title = c.Title,
                CreatedAt = c.CreatedAt,
                UserId = c.UserId
            });
        }

        public async Task<ChatDetailDto?> GetChatDetailAsync(int id)
        {
            var chat = await _chatRepository.GetByIdAsync(id);
            if (chat == null) return null;

            var result = new ChatDetailDto
            {
                Id = chat.Id,
                Title = chat.Title,
                CreatedAt = chat.CreatedAt,
                UserId = chat.UserId,
                Insights = chat.Insights.Select(i => new InsightsHistoDto
                {
                    Id = i.Id,
                    UserId = i.UserId,
                    Question = i.Question,
                    Answer = i.Answer,
                    Date = i.Date
                }).ToList()
            };

            return result;
        }

        public async Task<bool> DeleteChatAsync(int id)
        {
            var deleted = await _chatRepository.DeleteAsync(id);

            if (!deleted)
                return false;

            await _chatRepository.SaveChangesAsync();
            return true;
        }
    }
}
