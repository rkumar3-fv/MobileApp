﻿using AutoMapper;
using FreedomVoice.DAL.DbEntities;
using FreedomVoice.DAL.DbEntities.Enums;

namespace FreedomVoice.Core
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<FreedomVoice.Entities.Response.Conversation, Conversation>()
                .ForSourceMember(x => x.IsRemoved, opt => opt.DoNotValidate());
            CreateMap<Conversation, FreedomVoice.Entities.Response.Conversation>()
                .ForMember(x => x.IsRemoved, opt => opt.Ignore());
            
            CreateMap<FreedomVoice.Entities.Phone, Phone>();

            CreateMap<FreedomVoice.Entities.Message, Message>()
                .ForSourceMember(x => x.LastUpdateDate, opt => opt.DoNotValidate());
            CreateMap<Message, FreedomVoice.Entities.Message>()  
                .ForMember(x => x.LastUpdateDate, opt => opt.Ignore());
            
            CreateMap<FreedomVoice.Entities.Enums.SendingState, SendingState>();
        }
    }
}