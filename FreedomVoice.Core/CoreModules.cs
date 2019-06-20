﻿using AutoMapper;
using FreedomVoice.Core.Services;
using FreedomVoice.Core.Services.Interfaces;
using FreedomVoice.Core.Utils;
using FreedomVoice.Core.Utils.Interfaces;
using FreedomVoice.DAL;
using FreedomVoice.DAL.DbEntities;
using FreedomVoice.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace FreedomVoice.Core
{
    public class CoreModules
    {
        public static void Load(string dbPath)
        {
            AutoMapper.Mappers.EnumToEnumMapper.Map<SendingState, DAL.DbEntities.Enums.SendingState>(SendingState.Sending);
            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MappingProfile());
            });
            IMapper mapper = mappingConfig.CreateMapper();
            ServiceContainer.Register(mapper);
            

            FreedomVoiceContext context = new FreedomVoiceContext(dbPath);
            context.Database.Migrate();
            ServiceContainer.Register<IDbContext>(context);
            ServiceContainer.Register<IRepository<Conversation>>(() => new EfRepository<Conversation>(ServiceContainer.Resolve<IDbContext>()));
            ServiceContainer.Register<IRepository<Phone>>(() => new EfRepository<Phone>(ServiceContainer.Resolve<IDbContext>()));
            ServiceContainer.Register<IRepository<Message>>(() => new EfRepository<Message>(ServiceContainer.Resolve<IDbContext>()));

            ServiceContainer.Register<ICacheService>(() => new CacheService(ServiceContainer.Resolve<IRepository<Conversation>>(), 
                ServiceContainer.Resolve<IRepository<Message>>(), ServiceContainer.Resolve<IRepository<Phone>>(), ServiceContainer.Resolve<IMapper>()));
            ServiceContainer.Register<INetworkService>(() => new NetworkService(ServiceContainer.Resolve<ICacheService>(), ServiceContainer.Resolve<IMapper>()));
            ServiceContainer.Register<IConversationService>(() => new ConversationService(ServiceContainer.Resolve<ICacheService>(), 
                ServiceContainer.Resolve<INetworkService>(), ServiceContainer.Resolve<IMapper>()));
            ServiceContainer.Register<IMessagesService>(() => new MessageService(ServiceContainer.Resolve<ICacheService>(),
                ServiceContainer.Resolve<INetworkService>(), ServiceContainer.Resolve<IMapper>(), ServiceContainer.Resolve<IConversationService>()));
            
            ServiceContainer.Register<IPushService>(() => new PushService(ServiceContainer.Resolve<INetworkService>()));

            ServiceContainer.Register<IPhoneFormatter>(() => new PhoneFormatter());
        }
    }
}
