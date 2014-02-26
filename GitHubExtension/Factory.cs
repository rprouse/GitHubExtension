using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using Ninject;

namespace Alteridem.GitHub
{
    /// <summary>
    /// The dependency injection factory that wraps
    /// the framework allowing it to be easily swapped out.
    /// </summary>
    public static class Factory
    {
        private static IKernel _kernel;

        public static T Get<T>()
        {
            if (_kernel == null)
            {
                CreateKernel();
            }
            return _kernel.Get<T>();
        }

        private static void CreateKernel()
        {
            _kernel = new StandardKernel();
            _kernel.Load(Assembly.GetExecutingAssembly());
        }
    }
}