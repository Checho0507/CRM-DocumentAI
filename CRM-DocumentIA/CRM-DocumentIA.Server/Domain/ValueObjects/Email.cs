// Domain/ValueObjects/Email.cs

using System.Text.RegularExpressions;

namespace CRM_DocumentIA.Domain.ValueObjects
{
    // Record se usa para ValueObjects por su inmutabilidad y comparación automática
    public record Email
    {
        public string Valor { get; }

        // Patrón básico para validar formato de correo
        private static readonly Regex EmailRegex = new Regex(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public Email(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("El email no puede estar vacío.", nameof(email));
            }

            var valorNormalizado = email.Trim().ToLowerInvariant();

            if (!EmailRegex.IsMatch(valorNormalizado))
            {
                throw new ArgumentException($"El formato de email '{email}' es inválido.", nameof(email));
            }

            this.Valor = valorNormalizado;
        }

        // Permite la conversión implícita de Email a string
        public static implicit operator string(Email email) => email.Valor;

        // Sobrecarga del método ToString()
        public override string ToString() => Valor;
    }
}