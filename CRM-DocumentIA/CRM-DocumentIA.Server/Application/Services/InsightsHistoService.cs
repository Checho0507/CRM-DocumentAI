using CRM_DocumentIA.Server.Application.Dtos.Rag;
using CRM_DocumentIA.Server.Application.DTOs.InsightsHisto;
using CRM_DocumentIA.Server.Domain.Entities;
using CRM_DocumentIA.Server.Domain.Interfaces;
using CRM_DocumentIA.Server.Domain.Interfaces.Repositories;
using CRM_DocumentIA.Server.Infrastructure.Rag;

namespace CRM_DocumentIA.Server.Application.Services
{
    public class InsightsHistoService
    {
        private readonly IInsightsHistoRepository _repository;
        private readonly IChatRepository _chatRepository;
        private readonly IRagClient _ragClient;

        public InsightsHistoService(IInsightsHistoRepository repository, IRagClient ragClient, IChatRepository chatRepository)
        {
            _repository = repository;
            _ragClient = ragClient;
            _chatRepository = chatRepository;
        }

        public async Task<InsightsHistoDto> CreateAsync(CreateInsightsHistoDto dto)
        {
            var entity = new InsightsHisto
            {
                UserId = dto.UserId,
                Question = dto.Question,
                Answer = dto.Answer,
                Date = DateTime.Now
            };

            var created = await _repository.CreateAsync(entity);

            return new InsightsHistoDto
            {
                Id = created.Id,
                UserId = created.UserId,
                Question = created.Question,
                Answer = created.Answer,
                Date = created.Date
            };
        }

        public async Task<InsightsHistoDto?> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return null;

            return new InsightsHistoDto
            {
                Id = entity.Id,
                UserId = entity.UserId,
                Question = entity.Question,
                Answer = entity.Answer,
                Date = entity.Date
            };
        }

        public async Task<IEnumerable<InsightsHistoDto>> GetByUserIdAsync(int userId)
        {
            var list = await _repository.GetByUserIdAsync(userId);

            return list.Select(x => new InsightsHistoDto
            {
                Id = x.Id,
                UserId = x.UserId,
                Question = x.Question,
                Answer = x.Answer,
                Date = x.Date
            });
        }

        public async Task<IEnumerable<InsightsHistoDto>> GetAllAsync()
        {
            var list = await _repository.GetAllAsync();

            return list.Select(x => new InsightsHistoDto
            {
                Id = x.Id,
                UserId = x.UserId,
                Question = x.Question,
                Answer = x.Answer,
                Date = x.Date
            });
        }

        public async Task<bool> UpdateAsync(int id, UpdateInsightsHistoDto dto)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                return false;

            entity.Question = dto.Question;
            entity.Answer = dto.Answer;

            return await _repository.UpdateAsync(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }

        public async Task<InsightsHistoDto> AskRagAndSaveAsync(AskRagDto dto)
        {

            if (dto.chatId is null)
            {
                var newChat = new Chat
                {
                    UserId = dto.userId,
                    CreatedAt = DateTime.UtcNow,
                    Title = dto.Query.Length > 40
                        ? dto.Query.Substring(0, 40)
                        : dto.Query
                };

                await _chatRepository.CreateAsync(newChat);
                await _chatRepository.SaveChangesAsync();

                dto.chatId = newChat.Id; // usando el autoincrement generado
            }

            var ragRequest = new RagRequestDto
            {
                Query = dto.Query
            };

            var ragResponse = await _ragClient.AskAsync(ragRequest);

            var entity = new InsightsHisto
            {
                UserId = 1,
                Question = dto.Query,
                Answer = ragResponse.Answer,
                Date = DateTime.Now,
                ChatId = (int)dto.chatId
            };

            var created = await _repository.CreateAsync(entity);

            return new InsightsHistoDto
            {
                Id = created.Id,
                UserId = created.UserId,
                Question = created.Question,
                Answer = created.Answer,
                Date = created.Date,
                chatId = created.ChatId,
            };
        }


    }
}
