using Microsoft.AspNetCore.Mvc;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.DataAccess;
using PromoCodeFactory.DataAccess.Repositories;
using PromoCodeFactory.WebHost.Models;
using System;
using System.Threading.Tasks;
using System.Linq;


namespace PromoCodeFactory.WebHost.Controllers
{
    /// <summary>
    /// Клиенты
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<Preference> _preferenceRepository;


        public CustomersController(IRepository<Customer> customerRepository, IRepository<Preference> preferenceRepository)
        {
            _customerRepository = customerRepository;
            _preferenceRepository = preferenceRepository;
        }

        /// <summary>
        /// Получение списка клиентов
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<CustomerShortResponse>> GetCustomersAsync()
        {
            var customerSet = await _customerRepository.GetAllAsync();
            var result = customerSet.Select(q => new CustomerShortResponse
            {
                Id = q.Id,
                Email = q.Email,
                FirstName = q.FirstName,
                LastName = q.LastName
            }).ToList();

            return Ok(result);
        }

        /// <summary>
        /// Получить данные клиента вместе с выданными ему промокодами по id
        /// </summary>
        /// <param name="id">GUID клиента</param>
        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerResponse>> GetCustomerAsync(Guid id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);

            if (customer == null)
                return NotFound();

            var customerModel = new CustomerResponse()
            {
                Id = customer.Id,
                Email = customer.Email,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Preferences = customer.Preferences.Select(p => new PreferenceResponse()
                {
                    Id = p.PreferenceId,
                    Name = p.Preference.Name
                }).ToList(),
                PromoCodes = customer.PromoCodes.Select(p => new PromoCodeShortResponse()
                {
                    Id = p.Id,
                    Code = p.Code,
                    ServiceInfo = p.ServiceInfo,
                    BeginDate = p.BeginDate.ToString("d"),
                    EndDate = p.EndDate.ToString("d"),
                    PartnerName = p.PartnerName
                }).ToList()
            };

            return customerModel;
        }

        /// <summary>
        /// Создание нового клиента с перечнем предпочтений
        /// </summary>
        /// <param name="request">Атрибуты клиента</param>
        [HttpPost]
        public async Task<IActionResult> CreateCustomerAsync(CreateOrEditCustomerRequest request)
        {
            //Получаем предпочтения из бд и сохраняем большой объект
            var preferences = await _preferenceRepository.GetRangeByIdsAsync(request.PreferenceIds);

            var customer = new Customer()
            {
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
            };
            customer.Preferences = preferences.Select(x => new CustomerPreference()
            {
                Customer = customer,
                Preference = x
            }).ToList();

            await _customerRepository.AddAsync(customer);

            // Вот так делать не нужно - улетаем в цикл, т.к. CustomerPreference содержит ссылку на Customer, который ссылается на CustomerPreference.
            //return Ok(customer);
            return Ok(customer.Id);

            //// TODO: Хоть так, хоть так возвращает ошибку: "No route matches the supplied values"
            //return CreatedAtRoute(nameof(GetCustomerAsync), new { id = customer.Id });
            //return CreatedAtAction(nameof(GetCustomerAsync), new { id = customer.Id });
        }

        /// <summary>
        /// Обновление данных клиента вместе с его предпочтениями
        /// </summary>
        /// <param name="id">GUID клиента</param>
        /// <param name="request">Атрибуты клиента</param>
        [HttpPut("{id}")]
        public async Task<IActionResult> EditCustomersAsync(Guid id, CreateOrEditCustomerRequest request)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null)
                return NotFound();

            var preferences = await _preferenceRepository.GetRangeByIdsAsync(request.PreferenceIds);

            customer.FirstName = request.FirstName;
            customer.LastName = request.LastName;
            customer.Email = request.Email;

            // Очистка обязательна. Иначе валится ошибка нарушения уникальности PK: "UNIQUE constraint failed: CustomerPreference.CustomerId, CustomerPreference.PreferenceId"
            customer.Preferences.Clear();
            customer.Preferences = preferences.Select(preference => new CustomerPreference
            {
                Customer = customer,
                Preference = preference
            }).ToList();

            await _customerRepository.UpdateAsync(customer);

            return Ok(customer.Id);
        }

        /// <summary>
        /// Удаление клиента вместе с выданными ему промокодами
        /// </summary>
        /// <param name="id">GUID клиента</param>
        [HttpDelete]
        public async Task<IActionResult> DeleteCustomer(Guid id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null)
                return NotFound();

            await _customerRepository.DeleteAsync(customer);

            return Ok();
        }
    }
}