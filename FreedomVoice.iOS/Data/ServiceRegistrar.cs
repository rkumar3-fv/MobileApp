using FreedomVoice.iOS.Data.Implementations;
using FreedomVoice.iOS.Utilities;
using FreedomVoice.iOS.ViewModels;

namespace FreedomVoice.iOS.Data
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
            ServiceContainer.Register<IForgotPasswordService>(() => new ForgotPasswordService());
            ServiceContainer.Register<IAccountsService>(() => new AccountsService());

            ServiceContainer.Register<LoginViewModel>();
            ServiceContainer.Register<ForgotPasswordViewModel>();
            ServiceContainer.Register<AccountsViewModel>();
        }
    }
}