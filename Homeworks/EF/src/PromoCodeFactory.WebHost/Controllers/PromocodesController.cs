using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PromoCodeFactory.WebHost.Models;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using System.Linq;
using Castle.Core.Resource;

namespace PromoCodeFactory.WebHost.Controllers
{
    /// <summary>
    /// Промокоды
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public class PromocodesController : ControllerBase
    {
        private readonly IRepository<PromoCode> _promocodesRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<Preference> _preferenceRepository;

        public PromocodesController(IRepository<PromoCode> promocodesRepository, IRepository<Customer> customerRepository, IRepository<Preference> preferenceRepository)
        {
            _promocodesRepository = promocodesRepository;
            _customerRepository = customerRepository;
            _preferenceRepository = preferenceRepository;
        }

        /// <summary>
        /// Получить все промокоды (короткий формат)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<PromoCodeShortResponse>>> GetPromocodesAsync()
        {
            var promocodes = await _promocodesRepository.GetAllAsync();
            var promocodesList = promocodes.Select(q =>
                new PromoCodeShortResponse
                {
                    Id = q.Id,
                    Code = q.Code,
                    ServiceInfo = q.ServiceInfo,
                    BeginDate = q.BeginDate.ToString("d"),
                    EndDate = q.EndDate.ToString("d"),
                    PartnerName = q.PartnerName
                }
            );
            return Ok(promocodesList);
        }

        /// <summary>
        /// Создаём промокод и выдаём его первому попавшемуся клиенту с указанным предпочтением (т.к. в рамках ДЗ "промокод может быть выдан только одному клиенту из базы")
        /// </summary>
        /// <param name="request">Описание промокода</param>
        [HttpPost]
        public async Task<IActionResult> GivePromoCodesToCustomersWithPreferenceAsync(GivePromoCodeRequest request)
        {
            // Сперва убеждаемся, что указанное в промокоде предпочтение существует
            var preferences = await _preferenceRepository.GetAllAsync();
            var preference = preferences.FirstOrDefault(pref => pref.Name == request.Preference);
            if (preference == null)
                return NotFound($"Preference '{request.Preference}' does not exists.");

            // Потом находим первого попавшегося клиента с подходящим предпочтением
            // Первый попавшийся, т.к. согласно п.4 ДЗ "в данном примере промокод может быть выдан только одному клиенту из базы".
            var customer = (await _customerRepository.GetAllAsync()).
                FirstOrDefault(customer => customer.Preferences.Any(pref => pref.PreferenceId == preference.Id));

            // В задании явно не указано, нужно ли отказаться от создания промокода, если клиента с подходящим предпочтением не существует.
            // Просто указана последовательность действий.
            // Поэтому промокод будет создан в предположении, что далее будет реализована функциональность, которая при добавлении нового клиента с нужным предпочтением будет сразу отдавать ему "свободный" промокод.
            var promo = new PromoCode {
                Code = request.PromoCode,
                PartnerName = request.PartnerName,
                Owner = customer,
                ServiceInfo = request.ServiceInfo,
                Preference = preference
            };
            await _promocodesRepository.AddAsync(promo);

            return Ok();
        }
    }
}