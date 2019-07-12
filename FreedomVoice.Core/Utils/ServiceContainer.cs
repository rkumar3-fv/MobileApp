using System;
using System.Collections.Generic;

namespace FreedomVoice.Core.Utils
{
    public class ServiceContainer
    {
        static readonly object Locker = new object();

        static ServiceContainer _instance;

        private ServiceContainer()
        {
            Services = new Dictionary<Type, Lazy<object>>();
        }

        private Dictionary<Type, Lazy<object>> Services { get; }

        private static ServiceContainer Instance
        {
            get
            {
                lock (Locker)
                {
                    return _instance ?? (_instance = new ServiceContainer());
                }
            }
        }

        public static void Register<T>(T service)
        {
            Instance.Services[typeof(T)] = new Lazy<object>(() => service);
        }

        public static void RegisterFactory<T>(Func<T> service)
        {
            Instance.Services[typeof(T)] = new Lazy<object>(() => service);
        }

        public static void Register<T>() where T : new()
        {
            Instance.Services[typeof(T)] = new Lazy<object>(() => new T());
        }

        public static void Register<T>(Func<object> function)
        {
            Instance.Services[typeof(T)] = new Lazy<object>(function);
        }

        public static T Resolve<T>()
        {
            Lazy<object> service;
            if (Instance.Services.TryGetValue(typeof(T), out service))
            {
                if (service.Value is Func<T>)
                {
                    return ((Func<T>)service.Value)();
                }
                return (T)service.Value;
            }

            throw new KeyNotFoundException($"Service not found for type '{typeof (T)}'");
        }
    }
}
