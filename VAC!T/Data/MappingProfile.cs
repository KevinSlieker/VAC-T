using VAC_T.Models;
using VAC_T.Data.DTO;
using VAC_T.Services;
using static VAC_T.Models.RepeatAppointment;
//using static VAC_T.DAL.Service;
using VAC_T.Business;

namespace VAC_T.Data
{
    public class MappingProfile : AutoMapper.Profile
    {
        public MappingProfile()
        {
            CreateMap<Appointment, AppointmentDTO>();
            CreateMap<Appointment, AppointmentDTOForCreate>();
            CreateMap<Appointment, AppointmentDTOAvailable>()
                .ForMember(dest => dest.InputForSelect, opt => opt.MapFrom(src => src.RepeatAppointmentId == null ? src.Id.ToString() : src.Time.ToString() + "_" + src.RepeatAppointmentId.ToString()));
            CreateMap<AppointmentDTO, Appointment>();
            CreateMap<AppointmentDTOForCreate, Appointment>();
            CreateMap<Company, CompanyDTO>();
            CreateMap<Company, CompanyDTOSmall>();
            CreateMap<CompanyDTO, Company>();
            CreateMap<CompanyDTOSmall, Company>();
            CreateMap<CompanyDTOForUpdate, Company>();
            CreateMap<JobOffer, JobOfferDTO>();
            CreateMap<JobOffer, JobOfferDTOSmall>();
            CreateMap<JobOffer, JobOfferDTOWQuestions>();
            CreateMap<JobOfferDTO, JobOffer>();
            CreateMap<JobOfferDTOForUpdateAndCreate, JobOffer>();
            CreateMap<JobOfferDTOForCreateTemp, JobOffer>();
            CreateMap<VAC_TUser, UserDTOSmall>();
            CreateMap<VAC_TUser, UserDetailsDTO>();
            CreateMap<Solicitation, SolicitationDTOSmall>();
            CreateMap<Solicitation, SolicitationDTOComplete>();
            CreateMap<Solicitation, SolicitationDTOSelect>();
            CreateMap<RepeatAppointment, RepeatAppointmentEnumViewModel>()
                .ForMember(dest => dest.IsMonday, opt => opt.MapFrom(src => src.RepeatsWeekdays.HasValue == true ? src.RepeatsWeekdays.Value.HasFlag(Repeats_Weekdays.Monday) : false))
                .ForMember(dest => dest.IsTuesday, opt => opt.MapFrom(src => src.RepeatsWeekdays.HasValue == true ? src.RepeatsWeekdays.Value.HasFlag(Repeats_Weekdays.Tuesday) : false))
                .ForMember(dest => dest.IsWednesday, opt => opt.MapFrom(src => src.RepeatsWeekdays.HasValue == true ? src.RepeatsWeekdays.Value.HasFlag(Repeats_Weekdays.Wednesday) : false))
                .ForMember(dest => dest.IsThursday, opt => opt.MapFrom(src => src.RepeatsWeekdays.HasValue == true ? src.RepeatsWeekdays.Value.HasFlag(Repeats_Weekdays.Thursday) : false))
                .ForMember(dest => dest.IsFriday, opt => opt.MapFrom(src => src.RepeatsWeekdays.HasValue == true ? src.RepeatsWeekdays.Value.HasFlag(Repeats_Weekdays.Friday) : false))
                .ForMember(dest => dest.IsFirst, opt => opt.MapFrom(src => src.RepeatsRelativeWeek.HasValue == true ? src.RepeatsRelativeWeek.Value.HasFlag(Repeats_Relative_Week.First) : false))
                .ForMember(dest => dest.IsSecond, opt => opt.MapFrom(src => src.RepeatsRelativeWeek.HasValue == true ? src.RepeatsRelativeWeek.Value.HasFlag(Repeats_Relative_Week.Second) : false))
                .ForMember(dest => dest.IsThird, opt => opt.MapFrom(src => src.RepeatsRelativeWeek.HasValue == true ? src.RepeatsRelativeWeek.Value.HasFlag(Repeats_Relative_Week.Third) : false))
                .ForMember(dest => dest.IsFourth, opt => opt.MapFrom(src => src.RepeatsRelativeWeek.HasValue == true ? src.RepeatsRelativeWeek.Value.HasFlag(Repeats_Relative_Week.Fourth) : false))
                .ForMember(dest => dest.IsLast, opt => opt.MapFrom(src => src.RepeatsRelativeWeek.HasValue == true ? src.RepeatsRelativeWeek.Value.HasFlag(Repeats_Relative_Week.Last) : false));
            CreateMap<RepeatAppointmentEnumViewModel, RepeatAppointment>()
                .ForMember(dest => dest.RepeatsWeekdays, opt => opt.MapFrom(src => MappingService.MapRepeatsWeekdays(src)))
                .ForMember(dest => dest.RepeatsRelativeWeek, opt => opt.MapFrom(src => MappingService.MapRepeatsRelativeWeek(src)));
            CreateMap<RepeatAppointment, RepeatAppointmentDTO>();
            CreateMap<RepeatAppointment, RepeatAppointmentDTOForCreate>();
            CreateMap<RepeatAppointmentDTOForCreate, RepeatAppointment>();
            CreateMap<Answer, AnswerViewModel>()
                .ForMember(dest => dest.MultipleChoiceAnswers, opt => opt.MapFrom(src => (src.Question.Type == "Meerkeuze" && src.Question.MultipleOptions == true) ? src.AnswerText.Split('_', '_') : new string[0]));
            CreateMap<AnswerViewModel, Answer>()
                .ForMember(dest => dest.AnswerText, opt => opt.MapFrom(src => (src.Question.Type == "Meerkeuze" && src.Question.MultipleOptions == true) ? string.Join('_', src.MultipleChoiceAnswers) : src.AnswerText));
            CreateMap<Question, QuestionDTOId>();
            CreateMap<Question, QuestionDTOSmall>();
            CreateMap<Question, QuestionDTOMedium>();
            CreateMap<Question, QuestionDTOComplete>();
            CreateMap<QuestionDTOForCreate, Question>();
            CreateMap<QuestionOption, QuestionOptionDTOSmall>();
            CreateMap<QuestionOptionDTOSmall, QuestionOption>();
            CreateMap<QuestionDTOMedium, Question>();
            CreateMap<Answer, AnswerDTOSmall>();
            CreateMap<Answer, AnswerDTOMedium>();
            CreateMap<Answer, AnswerDTOComplete>();
            CreateMap<Answer, AnswerDTOExtended>()
                                .ForMember(dest => dest.MultipleChoiceAnswers, opt => opt.MapFrom(src => (src.Question.Type == "Meerkeuze" && src.Question.MultipleOptions == true) ? src.AnswerText.Split('_', '_') : new string[0]));
                                //.ForMember(dest => dest.DisplayAnswerText, opt => opt.MapFrom(src => AnswerService.PrepareAnswerForDisplay(src.AnswerText, src.Question));
            CreateMap<AnswerViewModel, AnswerDTOExtended>();
            CreateMap<AnswerDTOExtended, AnswerViewModel>();
            CreateMap<AnswerDTOForCreate, AnswerViewModel>();
            CreateMap<AnswerDTOForCreate, Answer>()
                .ForMember(dest => dest.AnswerText, opt => opt.MapFrom((src, dest) => (dest.Question.Type == "Meerkeuze" && dest.Question.MultipleOptions == true) ? string.Join('_', src.MultipleChoiceAnswers) : src.AnswerText));

        }
    }
}
