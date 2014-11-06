using System.Reflection;
using Ninject;
using Ninject.Parameters;
using Ninject.Syntax;

namespace Alteridem.GitHub
{
    /// <summary>
    /// The dependency injection factory that wraps
    /// the framework allowing it to be easily swapped out.
    /// </summary>
    public static class Factory
    {
        private static IKernel _kernel = new StandardKernel();

        static Factory()
        {
            // Add modules in this assembly
            AddAssembly(Assembly.GetExecutingAssembly());
        }

        public static void AddAssembly(Assembly assembly)
        {
            _kernel.Load(assembly);
        }

        public static T Get<T>()
        {
            try
            {
                return _kernel.Get<T>();
            }
            catch(ActivationException)
            {
                return default(T);
            }
        }

        public static T Get<T>(params IParameter[] parameters)
        {
            return _kernel.Get<T>(parameters);
        }

        public static IBindingToSyntax<T> Bind<T>()
        {
            return _kernel.Bind<T>();
        }

        public static IBindingToSyntax<T> Rebind<T>()
        {
            return _kernel.Rebind<T>();
        }
    }
}