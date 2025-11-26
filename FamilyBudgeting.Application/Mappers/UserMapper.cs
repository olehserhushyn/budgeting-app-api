using FamilyBudgeting.Domain.DTOs.Models.Users;
using FamilyBudgeting.Domain.Data.Users;

namespace FamilyBudgeting.Domain.Mappers
{
    public static class UserMapper
    {
        public static ApplicationUser ConvertDtoToDomain(UserDto userDto)
        {
            return new ApplicationUser(userDto.Email, userDto.FirstName, userDto.LastName, userDto.IsDeleted) { Id = userDto.Id };
        }

        public static IEnumerable<ApplicationUser> ConvertDtoToDomains(IEnumerable<UserDto> userDtos)
        {
            return userDtos.Select(x => new ApplicationUser(x.Email, x.FirstName, x.LastName, x.IsDeleted) { Id = x.Id });
        }

        public static UserDto ConvertDomainToDto(ApplicationUser user)
        {
            return new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PasswordHash = user.PasswordHash,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                IsDeleted = user.IsDeleted,
                UserName = user.UserName
            };
        }

        public static IEnumerable<UserDto> ConvertDomainToDtos(IEnumerable<ApplicationUser> users)
        {
            return users.Select(x => new UserDto
                {
                    Id = x.Id,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    Email = x.Email,
                    PasswordHash = x.PasswordHash,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,
                    IsDeleted = x.IsDeleted,
                    UserName = x.UserName
                }
            );
        }
    }
}
