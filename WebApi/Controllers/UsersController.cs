using System;
using AutoMapper;
using Game.Domain;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using WebApi.Mappers;
using WebApi.Models;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : Controller
    {
        private readonly IUserRepository userRepository;
        private readonly IMapper mapper;

        // Чтобы ASP.NET положил что-то в userRepository требуется конфигурация
        public UsersController(IUserRepository userRepository, IMapper mapper)
        {
            this.userRepository = userRepository;
            this.mapper = mapper;
        }

        [HttpGet("{userId}", Name = nameof(GetUserById))]
        [Produces("application/json", "application/xml")]
        public ActionResult<UserDto> GetUserById([FromRoute] Guid userId)
        {
            var user = userRepository.FindById(userId);
            if (user is null)
                return NotFound();

            return Ok(mapper.Map<UserDto>(user));
        }

        [HttpPost]
        [Produces("application/json", "application/xml")]
        public IActionResult CreateUser([FromBody] UserCreateDto user)
        {
            if (user is null)
                return BadRequest();

            if (!ModelState.IsValid)
                return UnprocessableEntity(ModelState);

            var entity = mapper.Map<UserEntity>(user);
            entity = userRepository.Insert(entity);

            return CreatedAtRoute(
                nameof(GetUserById),
                new {userId = entity.Id},
                entity.Id);
        }

        [HttpPut("{userId}")]
        [Produces("application/json", "application/xml")]
        public IActionResult UpdateUser([FromRoute] Guid? userId, [FromBody] UserUpdateDto userUpdate)
        {
            if (userUpdate is null || !userId.HasValue)
                return BadRequest();

            if (!ModelState.IsValid)
                return UnprocessableEntity(ModelState);
            
            var user = userRepository.FindById(userId.Value);
            if (user is not null)
                mapper.Map(userUpdate, user);
            else
                user = mapper.Map<UserEntity>(userUpdate);

            if (user.Id != Guid.Empty)
            {
                userRepository.UpdateOrInsert(user, out var isInserted);
                if (!isInserted)
                    return NoContent();
            }
            else
                user = userRepository.Insert(user);
            
            return CreatedAtRoute(
                nameof(GetUserById),
                new {userId = user.Id},
                user.Id);
        }
        
        [HttpPatch("{userId}")]
        [Produces("application/json", "application/xml")]
        public IActionResult PartiallyUpdateUser([FromRoute] Guid? userId, [FromBody] JsonPatchDocument<UserUpdateDto> patchDoc)
        {
            if (patchDoc is null)
                return BadRequest();
            
            if (!userId.HasValue || userId.Value == Guid.Empty)
                return NotFound();
            
            var updateDto = new UserUpdateDto();
            patchDoc.ApplyTo(updateDto, ModelState);
            
            if (!TryValidateModel(updateDto))
                return UnprocessableEntity(ModelState);
            
            var user = userRepository.FindById(userId.Value);
            if (user is null)
                return NotFound();
            
            user = mapper.Map<UserEntity>(updateDto);
            userRepository.Update(user);
            
            return NoContent();
        }
    }
}