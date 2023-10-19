using System;
using System.Collections.Generic;
using AutoMapper;
using Game.Domain;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
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
        private readonly LinkGenerator linkGenerator;

        public UsersController(IUserRepository userRepository, IMapper mapper, LinkGenerator linkGenerator)
        {
            this.userRepository = userRepository;
            this.mapper = mapper;
            this.linkGenerator = linkGenerator;
        }

        [HttpHead("{userId}")]
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

        [HttpDelete("{userId}")]
        [Produces("application/json", "application/xml")]
        public IActionResult DeleteUser([FromRoute] Guid? userId)
        {
            if (userId is null || userId.Value == Guid.Empty)
                return NotFound();

            var user = userRepository.FindById(userId.Value);
            if (user is null)
                return NotFound();

            userRepository.Delete(userId.Value);
            return NoContent();
        }

        [HttpGet(Name = nameof(GetUsers))]
        [Produces("application/json", "application/xml")]
        public IActionResult GetUsers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1)
                pageNumber = 1;
            pageSize = Math.Clamp(pageSize, 1, 20);

            var pageList = userRepository.GetPage(pageNumber, pageSize);
            var users = mapper.Map<IEnumerable<UserDto>>(pageList);

            var paginationHeader = new
            {
                previousPageLink = pageList.HasPrevious
                    ? linkGenerator.GetUriByRouteValues(HttpContext, nameof(GetUsers), new {pageNumber = pageNumber - 1, pageSize})
                    : null,
                nextPageLink = pageList.HasNext
                    ? linkGenerator.GetUriByRouteValues(HttpContext, nameof(GetUsers), new {pageNumber = pageNumber + 1, pageSize})
                    : null,
                totalCount = pageList.TotalCount,
                pageSize,
                currentPage = pageList.CurrentPage,
                totalPages = pageList.TotalPages,
            };

            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationHeader));

            return Ok(users);
        }
    }
}