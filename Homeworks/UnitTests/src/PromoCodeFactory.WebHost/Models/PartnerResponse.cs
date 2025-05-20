using System;
using System.Collections.Generic;
using System.Linq;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;

namespace PromoCodeFactory.WebHost.Models
{
    public class PartnerResponse
    {
        public Guid Id { get; set; }

        public bool IsActive { get; set; }
        
        public string Name { get; set; }

        public int NumberIssuedPromoCodes  { get; set; }

        public List<PartnerPromoCodeLimitResponse> PartnerLimits { get; set; }

        public PartnerResponse(Partner partner)
        {
            Id = partner.Id;
            Name = partner.Name;
            NumberIssuedPromoCodes = partner.NumberIssuedPromoCodes;
            IsActive = true;
            PartnerLimits = partner.PartnerLimits
                .Select(y => new PartnerPromoCodeLimitResponse(y)).ToList();
        }
    }
}