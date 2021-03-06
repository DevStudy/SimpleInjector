﻿// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Integration.Wcf
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Activation;

    /// <summary>
    /// Factory that provides instances of <see cref="SimpleInjectorServiceHost"/> in managed hosting
    /// environments where the host instance is created dynamically in response to incoming messages.
    /// </summary>
    public class SimpleInjectorServiceHostFactory : ServiceHostFactory
    {
        private static Container? container;

        /// <summary>Sets the container.</summary>
        /// <param name="container">The container.</param>
        public static void SetContainer(Container container)
        {
            Requires.IsNotNull(container, nameof(container));

            if (SimpleInjectorServiceHostFactory.container != null)
            {
                throw new InvalidOperationException(
                    "This method can only be called once. The container has already been set.");
            }

            SimpleInjectorServiceHostFactory.container = container;
        }

        internal static bool HasInstanceContextModeSingle(Type serviceType) =>
            serviceType.GetServiceBehaviorAttribute()?.InstanceContextMode == InstanceContextMode.Single;

        /// <summary>
        /// Creates a <see cref="SimpleInjectorServiceHost"/> for a specified type of service with a specific
        /// base address.
        /// </summary>
        /// <returns>
        /// A <see cref="SimpleInjectorServiceHost"/> for the type of service specified with a specific base
        /// address.
        /// </returns>
        /// <param name="serviceType">
        /// Specifies the type of service to host.
        /// </param>
        /// <param name="baseAddresses">
        /// The <see cref="T:System.Array"/> of type <see cref="T:System.Uri"/> that contains the base
        /// addresses for the service hosted.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="serviceType"/> is a null
        /// reference.</exception>
        protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
        {
            Requires.IsNotNull(serviceType, nameof(serviceType));

            if (container is null)
            {
                throw new InvalidOperationException("The operation failed. Please call the " +
                    typeof(SimpleInjectorServiceHostFactory).FullName + ".SetContainer(Container) method " +
                    "supplying the application's " + typeof(Container).FullName + " instance during " +
                    "application startup (for instance inside the Application_Start event of the Global.asax). " +
                    "In case you're running on non-HTTP protocols such as net.tcp and net.pipe that is " +
                    "supported by the Windows Activation Service (WAS), please see the WCF integration " +
                    "documentation: https://simpleinjector.org/wcf.");
            }

            InstanceProducer producer = container.GetRegistration(serviceType, true)!;

            // Force building the expression tree. Decorators and interceptors might be applied at this point,
            // causing the lifestyle and registration properties to change.
            producer.BuildExpression();

            Type implementationType = producer.Registration.ImplementationType;

            return HasInstanceContextModeSingle(implementationType)
                ? new SimpleInjectorServiceHost(container, GetSingletonInstance(producer), baseAddresses)
                : new SimpleInjectorServiceHost(container, serviceType, implementationType, baseAddresses);
        }

        private static object GetSingletonInstance(InstanceProducer registration)
        {
            if (registration.Lifestyle != Lifestyle.Singleton)
            {
                throw new InvalidOperationException(string.Format(
                    "Service type {0} has been flagged as 'Single' using the ServiceBehaviorAttribute, " +
                    "causing WCF to hold on to this instance indefinitely, while {1} has been registered " +
                    "with the {2} lifestyle in the container. Please make sure that {1} is registered " +
                    "as Singleton as well, or mark {0} with " +
                    "[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)] instead.",
                    registration.ServiceType.FullName,
                    registration.Registration.ImplementationType.FullName,
                    registration.Lifestyle.Name));
            }

            return registration.GetInstance();
        }
    }
}