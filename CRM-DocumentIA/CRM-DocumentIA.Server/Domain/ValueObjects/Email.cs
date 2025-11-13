using System.Text.RegularExpressions;

namespace CRM_DocumentIA.Domain.ValueObjects
{
    public record Email
    {
        private const string EmailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        
        public string Value { get; } // ✅ Asegurar que se llama Value

        public Email(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("El email no puede estar vacío");

            if (!Regex.IsMatch(value, EmailPattern))
                throw new ArgumentException("El formato del email no es válido");

            Value = value.ToLowerInvariant();
        }

        public static implicit operator string(Email email) => email.Value;
        public static explicit operator Email(string value) => new Email(value);

        public override string ToString() => Value;
    }
}