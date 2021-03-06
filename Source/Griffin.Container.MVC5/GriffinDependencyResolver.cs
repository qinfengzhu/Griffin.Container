using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

namespace Griffin.Container.Mvc5
{
    /// <summary>
    /// Griffin.Container implementation
    /// </summary>
    public class GriffinDependencyResolver : IDependencyResolver
    {
        private readonly IParentContainer _container;

        /// <summary>
        /// Initializes a new instance of the <see cref="GriffinDependencyResolver"/> class.
        /// </summary>
        /// <param name="container">The container.</param>
        public GriffinDependencyResolver(IParentContainer container)
        {
            if (container == null) throw new ArgumentNullException("container");
            _container = container;
        }

        /// <summary>
        /// Gets current child container
        /// </summary>
        protected IChildContainer ChildContainer
        {
            get
            {
                var container = (IChildContainer)HttpContext.Current.Items["GriffinDependencyResolverChild"];
                if (container == null)
                {
                    container = CreateAndStartChildContainer();
                    HttpContext.Current.Items["GriffinDependencyResolverChild"] = container;
                }
                return container;
            }
        }

        private IChildContainer CreateAndStartChildContainer()
        {
            var child = _container.CreateChildContainer();
            foreach (var startable in child.ResolveAll<IScopedStartable>())
            {
                startable.StartScoped();
            }

            return child;
        }

        #region IDependencyResolver Members

        /// <summary>
        /// Resolves singly registered services that support arbitrary object creation.
        /// </summary>
        /// <returns>
        /// The requested service or object.
        /// </returns>
        /// <param name="serviceType">The type of the requested service or object.</param>
        public object GetService(Type serviceType)
        {
            return !ChildContainer.IsRegistered(serviceType) ? null : ChildContainer.Resolve(serviceType);
        }

        /// <summary>
        /// Resolves multiply registered services.
        /// </summary>
        /// <returns>
        /// The requested services.
        /// </returns>
        /// <param name="serviceType">The type of the requested services.</param>
        public IEnumerable<object> GetServices(Type serviceType)
        {
            return !ChildContainer.IsRegistered(serviceType) ? new object[0] : ChildContainer.ResolveAll(serviceType);
        }

        #endregion

        /// <summary>
        /// Dispose current child container (if any)
        /// </summary>
        public static void DisposeChildContainer()
        {
            var container = (IChildContainer)HttpContext.Current.Items["GriffinDependencyResolverChild"];
            if (container != null)
            {
                container.Dispose();
                HttpContext.Current.Items["GriffinDependencyResolverChild"] = null;
            }
        }
    }
}