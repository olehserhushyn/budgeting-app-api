using System.Runtime.Serialization;

namespace FamilyBudgeting.Domain.Exceptions
{
    public class DomainValidationException : Exception
    {
        public DomainValidationException()
        {
        }

        public DomainValidationException(string? message) : base(message)
        {
        }
    }
}
