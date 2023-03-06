using VAC_T.Models;
using VAC_T.Data.DTO;

namespace VAC_T.Data
{
    public class MappingProfile : AutoMapper.Profile
    {
        public MappingProfile() 
        {
            CreateMap<Company, CompanyDTO>();
            CreateMap<CompanyDTO, Company>();
            CreateMap<CompanyDTOForUpdate, Company>();
            CreateMap<JobOffer, JobOfferDTO>();
            CreateMap<JobOffer, JobOfferDTOSmall>();
            CreateMap<VAC_TUser, UserDTOSmall>();
        }
    }
}
