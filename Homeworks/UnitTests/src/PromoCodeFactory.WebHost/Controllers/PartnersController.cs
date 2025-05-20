using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.WebHost.Models;

namespace PromoCodeFactory.WebHost.Controllers
{
    /// <summary>
    /// Партнеры
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public class PartnersController
        : ControllerBase
    {
        private readonly IRepository<Partner> _partnersRepository;

        public PartnersController(IRepository<Partner> partnersRepository)
        {
            _partnersRepository = partnersRepository;
        }

        [HttpGet]
        [ProducesResponseType<List<PartnerResponse>>(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<PartnerResponse>>> GetPartnersAsync()
        {
            var partners = await _partnersRepository.GetAllAsync();

            var response = partners.Select(x => new PartnerResponse(x));

            return Ok(response);
        }
        
        [HttpGet("{id}/limits/{limitId}")]
        [ProducesResponseType<PartnerPromoCodeLimit>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PartnerPromoCodeLimit>> GetPartnerLimitAsync(Guid id, Guid limitId)
        {
            var partner = await _partnersRepository.GetByIdAsync(id);

            if (partner == null)
                return NotFound();
            
            var limit = partner.PartnerLimits
                .FirstOrDefault(x => x.Id == limitId);

            var response = new PartnerPromoCodeLimitResponse(limit);
            
            return Ok(response);
        }
        
        [HttpPost("{id}/limits")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SetPartnerPromoCodeLimitAsync(Guid id, SetPartnerPromoCodeLimitRequest request)
        {
            var partner = await _partnersRepository.GetByIdAsync(id);

            if (partner == null)
                return NotFound();
            
            //Если партнер заблокирован, то нужно выдать исключение
            if (!partner.IsActive)
                return BadRequest("Данный партнер не активен");

            //Установка лимита партнеру
            var activeLimit = partner.PartnerLimits.FirstOrDefault(x => 
                !x.CancelDate.HasValue);
            
            if (activeLimit != null)
            {
                //Если партнеру выставляется лимит, то мы 
                //должны обнулить количество промокодов, которые партнер выдал, если лимит закончился, 
                //то количество не обнуляется
                partner.NumberIssuedPromoCodes = 0;
                
                //При установке лимита нужно отключить предыдущий лимит
                activeLimit.CancelDate = DateTime.Now;
            }

            // Добавка: если описание лимита промокода отсуствует, то нужно выдать исключение
            if (request == null)
                return BadRequest("Нет описания лимита промокода");

            if (request.Limit <= 0)
                return BadRequest("Лимит должен быть больше 0");

            DateTime createDate = DateTime.Now;
            if (request.EndDate <= createDate)
                return BadRequest("Промокод не должен быть просрочен");

            var newLimit = new PartnerPromoCodeLimit()
            {
                Limit = request.Limit,
                Partner = partner,
                PartnerId = partner.Id,
                CreateDate = createDate,
                EndDate = request.EndDate
            };
            
            partner.PartnerLimits.Add(newLimit);

            await _partnersRepository.UpdateAsync(partner);
            
            return CreatedAtAction(nameof(GetPartnerLimitAsync), new {id = partner.Id, limitId = newLimit.Id}, null);
        }
        
        [HttpPost("{id}/canceledLimits")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CancelPartnerPromoCodeLimitAsync(Guid id)
        {
            var partner = await _partnersRepository.GetByIdAsync(id);
            
            if (partner == null)
                return NotFound();
            
            //Если партнер заблокирован, то нужно выдать исключение
            if (!partner.IsActive)
                return BadRequest("Данный партнер не активен");
            
            //Отключение лимита
            var activeLimit = partner.PartnerLimits.FirstOrDefault(x => 
                !x.CancelDate.HasValue);
            
            if (activeLimit != null)
            {
                activeLimit.CancelDate = DateTime.Now;
            }

            await _partnersRepository.UpdateAsync(partner);

            return NoContent();
        }
    }
}