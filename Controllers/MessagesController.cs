using AutoMapper;
using DATINGAPP.API.Data;
using DATINGAPP.API.Dtos;
using DATINGAPP.API.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DATINGAPP.API.Controllers
{
    [Route("api/users/{userId}/[controller]")]
    [ApiController]
    public class MessagesController:ControllerBase
    {
        private readonly IDatingREpository repo;
        private readonly IMapper mapper;

        public MessagesController(IDatingREpository repo, IMapper mapper)
        {
            this.repo = repo;
            this.mapper = mapper;
        }


        [HttpGet("{id}", Name ="GetMessage")]
        public async Task<IActionResult> GetMessage(int userId, int id)
        {

            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var messageFromRepo = await repo.GetMessage(id);

            if (messageFromRepo == null)
                return NotFound();

            return Ok(messageFromRepo);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMessage(int userId, MessageForCreationDto messageForCreationDto)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            messageForCreationDto.SenderId = userId;

            var recipient = await repo.GetUser(messageForCreationDto.RecipientId);

            if (recipient == null)
                return BadRequest("ვერ ვიპოვეთ მომხმარებელი");

            var message = mapper.Map<Message>(messageForCreationDto);

            repo.Add(message);

            var messageToReturn = mapper.Map<MessageForCreationDto>(message);

            if (await repo.SaveAll())
                return CreatedAtRoute("GetMessage", new { id = message.Id }, messageToReturn);

            throw new Exception("მესიჯი ვერ შეინახა");
        }
    }
}
