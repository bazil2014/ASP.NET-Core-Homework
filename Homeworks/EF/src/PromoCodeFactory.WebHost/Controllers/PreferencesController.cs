using Microsoft.AspNetCore.Mvc;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain.Administration;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.WebHost.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PromoCodeFactory.WebHost.Controllers
{
    /// <summary>
    /// Предпочтения
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public class PreferencesController
    {
        private readonly IRepository<Preference> _preferencesRepository;

        public PreferencesController(IRepository<Preference> preferencesRepository)
        {
            _preferencesRepository = preferencesRepository;
        }

        /// <summary>
        /// Получить перечень всех предпочтений
        /// </summary>
        [HttpGet]
        public async Task<IEnumerable<PreferenceResponse>> GetPreferencesAsync()
        {
            var preferences = await _preferencesRepository.GetAllAsync();

            var preferencesModelList = preferences.Select(x =>
                new PreferenceResponse()
                {
                    Id = x.Id,
                    Name = x.Name
                }).ToList();

            return preferencesModelList;
        }
    }
}
