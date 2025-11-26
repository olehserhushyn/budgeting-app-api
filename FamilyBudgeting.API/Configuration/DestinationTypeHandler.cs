using Dapper;
using FamilyBudgeting.Domain.Data.ValueObjects;
using System.Data;

namespace FamilyBudgeting.API.Configuration
{
    public class DestinationTypeHandler : SqlMapper.TypeHandler<DestinationType>
    {
        public override void SetValue(IDbDataParameter parameter, DestinationType value)
        {
            parameter.Value = value.ToString().ToLowerInvariant(); // PostgreSQL uses lowercase enums
        }

        public override DestinationType Parse(object value)
        {
            return Enum.Parse<DestinationType>(value.ToString(), ignoreCase: true);
        }
    }
}
