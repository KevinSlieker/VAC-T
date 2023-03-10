using VAC_T.Models;
using VAC_T.Data.DTO;

namespace VAC_T.Data
{
    public class MappingProfile : AutoMapper.Profile
    {
        public MappingProfile() 
        {
            CreateMap<Company, CompanyDTO>();
            CreateMap<Company, CompanyDTOSmall>();
            CreateMap<CompanyDTO, Company>();
            CreateMap<CompanyDTOSmall, Company>();
            CreateMap<CompanyDTOForUpdate, Company>();
            CreateMap<JobOffer, JobOfferDTO>();
            CreateMap<JobOffer, JobOfferDTOSmall>();
            CreateMap<JobOfferDTO, JobOffer>();
            CreateMap<JobOfferDTOForUpdateAndCreate, JobOffer>();
            CreateMap<JobOfferDTOForCreateTemp, JobOffer>();
            CreateMap<VAC_TUser, UserDTOSmall>();
            CreateMap<Solicitation, SolicitationDTOSmall>();
        }
    }
}
