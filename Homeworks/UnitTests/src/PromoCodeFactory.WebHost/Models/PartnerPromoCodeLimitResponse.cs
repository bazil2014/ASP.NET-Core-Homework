using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using System;

namespace PromoCodeFactory.WebHost.Models
{
    public class PartnerPromoCodeLimitResponse
    {
        public Guid Id { get; set; }

        public Guid PartnerId { get; set; }

        public string CreateDate { get; set; }

        public string CancelDate { get; set; }

        public string EndDate { get; set; }

        public int Limit { get; set; }

        public PartnerPromoCodeLimitResponse(PartnerPromoCodeLimit limit)
        {
            Id = limit.Id;
            PartnerId = limit.PartnerId;
            Limit = limit.Limit;
            CreateDate = limit.CreateDate.ToString("dd.MM.yyyy hh:mm:ss");
            EndDate = limit.EndDate.ToString("dd.MM.yyyy hh:mm:ss");
            CancelDate = limit.CancelDate?.ToString("dd.MM.yyyy hh:mm:ss");
        }
    }
}