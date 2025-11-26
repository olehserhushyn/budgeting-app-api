using Dapper;
using FamilyBudgeting.Domain.Data.ValueObjects;
using System.Data;

namespace FamilyBudgeting.API.Configuration
{
    public class InvitationStatusTypeHandler : SqlMapper.TypeHandler<InvitationStatus>
    {
        public override void SetValue(IDbDataParameter parameter, InvitationStatus value)
        {
            Console.WriteLine($"0000000000000000000000 Setting InvitationStatus: {value} -> {value.ToString()}");
            parameter.DbType = DbType.String;
            parameter.Value = value.ToString().ToLowerInvariant(); // PostgreSQL uses lowercase enums
        }

        public override InvitationStatus Parse(object value)
        {
            return Enum.Parse<InvitationStatus>(value.ToString(), ignoreCase: true);
        }
    }
}
