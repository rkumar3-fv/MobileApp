using AutoMapper;
using FreedomVoice.DAL.DbEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreedomVoice.Core
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<FreedomVoice.Entities.Response.Conversation, Conversation>();
            CreateMap<FreedomVoice.Entities.Phone, Phone>();
            CreateMap<FreedomVoice.Entities.Message, Message>();
        }
    }
}
