﻿using FreedomVoice.iOS.Services;
using FreedomVoice.iOS.Services.Implementations;

namespace FreedomVoice.iOS.Utilities
{
    /// <summary>
    /// Class for registering services for the app
    /// </summary>
    public static class ServiceRegistrar
    {
        /// <summary>
        /// Call on startup of the app, it configures ServiceContainer
        /// </summary>
        public static void Startup()
        {
            ServiceContainer.Register<ILoginService> (() => new LoginService());
            ServiceContainer.Register<IAccountsService>(() => new AccountsService());
            ServiceContainer.Register<IForgotPasswordService>(() => new ForgotPasswordService());
            ServiceContainer.Register<IPresentationNumbersService>(() => new PresentationNumbersService());
            ServiceContainer.Register<IPollingIntervalService>(() => new PollingIntervalService());
            ServiceContainer.Register<IExtensionsService>(() => new ExtensionsService());
            ServiceContainer.Register<IFoldersService>(() => new FoldersService());
            ServiceContainer.Register<IMessagesService>(() => new MessagesService());
            ServiceContainer.Register<ICallReservationService>(() => new CallReservationService());
            ServiceContainer.Register<IMediaService>(() => new MediaService());
            ServiceContainer.Register<IMessageOperationsService>(() => new MessageOperationsService());
        }
    }
}