using Game.Domain;
using WebApi.Models;

namespace WebApi.Mappers
{
    public interface IUserDtoMapper
    {
        UserDto Map(UserEntity entity);
    }
    
    public class UserDtoMapper : IUserDtoMapper
    {
        public UserDto Map(UserEntity entity)
        {
            return new UserDto()
            {
                CurrentGameId = entity.CurrentGameId,
                FullName = $"{entity.LastName} {entity.FirstName}",
            };
        }
    }
}