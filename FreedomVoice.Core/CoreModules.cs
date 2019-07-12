using System;
using AutoMapper;
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
            ServiceContainer.RegisterFactory<IDbContext>(() => context);

            ServiceContainer.RegisterFactory<IRepository<Conversation>>(() => new EfRepository<Conversation>(ServiceContainer.Resolve<IDbContext>()));
            ServiceContainer.RegisterFactory<IRepository<Phone>>(() => new EfRepository<Phone>(ServiceContainer.Resolve<IDbContext>()));
            ServiceContainer.RegisterFactory<IRepository<Message>>(() => new EfRepository<Message>(ServiceContainer.Resolve<IDbContext>()));

            ServiceContainer.Register<ICacheService>(() => new CacheService(ServiceContainer.Resolve<IRepository<Conversation>>(), 
                ServiceContainer.Resolve<IRepository<Message>>(), ServiceContainer.Resolve<IRepository<Phone>>(), ServiceContainer.Resolve<IMapper>()));

            ServiceContainer.RegisterFactory<INetworkService>(() => new NetworkService(ServiceContainer.Resolve<ICacheService>(), ServiceContainer.Resolve<IMapper>()));

            ServiceContainer.RegisterFactory<IConversationService>(() => new ConversationService(ServiceContainer.Resolve<ICacheService>(), 
                ServiceContainer.Resolve<INetworkService>(), ServiceContainer.Resolve<IMapper>()));

            ServiceContainer.RegisterFactory<IMessagesService>(() => new MessageService(ServiceContainer.Resolve<ICacheService>(),
                ServiceContainer.Resolve<INetworkService>(), ServiceContainer.Resolve<IMapper>(), ServiceContainer.Resolve<IConversationService>()));

            ServiceContainer.RegisterFactory<IPushService>(() => new PushService(ServiceContainer.Resolve<INetworkService>()));

            ServiceContainer.Register<IPhoneFormatter>(() => new PhoneFormatter());
        }
    }
}
