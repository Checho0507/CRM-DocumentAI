using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CRM_DocumentIA.Server.Domain.Entities;
using CRM_DocumentIA.Server.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

public interface ITwoFactorService
{
    Task<Guid> GenerateAndSendAsync(int usuarioId, string email);
    Task<bool> VerifyAsync(Guid tempId, string code);
    Task<bool> IsVerifiedAsync(Guid tempId, int usuarioId);
}

public class TwoFactorService : ITwoFactorService
{
    private readonly ApplicationDbContext _db;
    private readonly SmtpEmailService _email;
    private readonly IConfiguration _config;

    public TwoFactorService(ApplicationDbContext db, SmtpEmailService email, IConfiguration config)
    {
        _db = db;
        _email = email;
        _config = config;
    }

    private string HashCode(string code)
    {
        var key = Encoding.UTF8.GetBytes(_config["TwoFA:HashKey"] ?? "default-secret-key-please-change");
        using var h = new HMACSHA256(key);
        var payload = Encoding.UTF8.GetBytes(code);
        return Convert.ToBase64String(h.ComputeHash(payload));
    }

    public async Task<Guid> GenerateAndSendAsync(int usuarioId, string email)
    {
        try
        {
            
            var code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
            var hashed = HashCode(code);

            
            var req = new TwoFA
            {
                Id = Guid.NewGuid(),
                UsuarioId = usuarioId,
                CodeHash = hashed,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                Verified = false,
                Attempts = 0
            };

            _db.TwoFA.Add(req);
            await _db.SaveChangesAsync();

            // DEBUG: Verificar configuración SMTP
            var smtpHost = _config["Email:SmtpHost"];
            var smtpUser = _config["Email:SmtpUser"];
            var html = $"<p>Tu código de verificación es <strong>{code}</strong>. Expira en 5 minutos.</p>";

            await _email.SendEmailAsync(email, "Código de verificación (2FA) - CRM", html);

            return req.Id;
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task<bool> VerifyAsync(Guid tempId, string code)
    {
        var req = await _db.TwoFA.FirstOrDefaultAsync(x => x.Id == tempId);
        if (req == null) return false;
        if (req.Verified) return false;
        if (req.ExpiresAt < DateTime.UtcNow) return false;
        if (req.Attempts >= 5) return false;

        var hashed = HashCode(code);
        if (hashed == req.CodeHash)
        {
            req.Verified = true;
            await _db.SaveChangesAsync();
            return true;
        }
        req.Attempts++;
        await _db.SaveChangesAsync();
        return false;
    }

    public async Task<bool> IsVerifiedAsync(Guid tempId, int usuarioId)
    {
        var req = await _db.TwoFA.FirstOrDefaultAsync(x => x.Id == tempId && x.UsuarioId == usuarioId);
        return req != null && req.Verified && req.ExpiresAt >= DateTime.UtcNow;
    }
}