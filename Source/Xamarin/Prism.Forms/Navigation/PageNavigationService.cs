﻿using System;
using System.Diagnostics;
using Microsoft.Practices.ServiceLocation;
using Xamarin.Forms;
using Prism.Common;
using System.Reflection;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Prism.Navigation
{
    /// <summary>
    /// Provides page based navigation for ViewModels.
    /// </summary>
    public class PageNavigationService : INavigationService, IPageAware
    {
        private Page _page;
        Page IPageAware.Page
        {
            get { return _page; }
            set { _page = value; }
        }

        /// <summary>
        /// Navigates to the most recent entry in the back navigation history by popping the calling Page off the navigation stack.
        /// </summary>
        /// <param name="useModalNavigation">If <c>true</c> uses PopModalAsync, if <c>false</c> uses PopAsync</param>
        /// <param name="animated">If <c>true</c> the transition is animated, if <c>false</c> there is no animation on transition.</param>
        public void GoBack(bool useModalNavigation = true, bool animated = true)
        {
            var navigation = GetPageNavigation();
            DoPop(navigation, useModalNavigation, animated);
        }

        /// <summary>
        /// Initiates navigation to the target specified by the <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type which will be used to identify the name of the navigation target.</typeparam>
        /// <param name="parameters">The navigation parameters</param>
        /// <param name="useModalNavigation">If <c>true</c> uses PopModalAsync, if <c>false</c> uses PopAsync</param>
        /// <param name="animated">If <c>true</c> the transition is animated, if <c>false</c> there is no animation on transition.</param>
        public void Navigate<T>(NavigationParameters parameters = null, bool useModalNavigation = true, bool animated = true)
        {
            Navigate(typeof(T).FullName, parameters, useModalNavigation, animated);
        }

        /// <summary>
        /// Initiates navigation to the target specified by the <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name of the target to navigate to.</param>
        /// <param name="parameters">The navigation parameters</param>
        /// <param name="useModalNavigation">If <c>true</c> uses PopModalAsync, if <c>false</c> uses PopAsync</param>
        /// <param name="animated">If <c>true</c> the transition is animated, if <c>false</c> there is no animation on transition.</param>
        public void Navigate(string name, NavigationParameters parameters = null, bool useModalNavigation = true, bool animated = true)
        {
            var targetView = ServiceLocator.Current.GetInstance<object>(name) as Page;
            if (targetView != null)
            {
                var navigation = GetPageNavigation();

                if (!CanNavigate(_page, parameters))
                    return;

                Page navigationPageFromProvider = GetNavigationPageFromProvider(_page, targetView);

                OnNavigatedFrom(_page, parameters);

                DoPush(navigation, (navigationPageFromProvider != null ? navigationPageFromProvider : targetView), useModalNavigation, animated);

                OnNavigatedTo(targetView, parameters);
            }
            else
                Debug.WriteLine("Navigation ERROR: {0} not found. Make sure you have registered {0} for navigation.", name);
        }
        public Task<NavigationParameters> NavigateAndWait<T>(NavigationParameters parameters = null, bool useModalNavigation = true, bool animated = true)
        {
            return NavigateAndWait(typeof(T).FullName, parameters, useModalNavigation, animated);
        }
        public Task<NavigationParameters> NavigateAndWait(string name, NavigationParameters parameters = null,
            bool useModalNavigation = true, bool animated = true)
        {
            var source = new TaskCompletionSource<NavigationParameters>();
            var targetView = ServiceLocator.Current.GetInstance<object>(name) as Page;
            if (targetView != null)
            {
                var navigation = GetPageNavigation();

                if (!CanNavigate(_page, parameters))
                    return null;

                Page navigationPageFromProvider = GetNavigationPageFromProvider(_page, targetView);

                OnNavigatedFrom(_page, parameters);
                var target = (navigationPageFromProvider != null ? navigationPageFromProvider : targetView);
                EventHandler handler = null;
                handler = (sender, args) =>
                {
                    target.Disappearing -= handler;
                    var navigationParameters = GetNavigationParameters(target);
                    source.SetResult(navigationParameters);
                };

                target.Disappearing += handler;
                DoPush(navigation, target, useModalNavigation, animated);

                OnNavigatedTo(targetView, parameters);
            }
            else
                Debug.WriteLine("Navigation ERROR: {0} not found. Make sure you have registered {0} for navigation.", name);
            return source.Task;
        }

        private static NavigationParameters GetNavigationParameters(object item)
        {
            var navigationAndWaitParameterProvider = item as INavigationAndWaitParameterProvider;
            if (navigationAndWaitParameterProvider != null)
                return navigationAndWaitParameterProvider.Provide();

            var bindableObject = item as BindableObject;
            if (bindableObject != null)
            {
                var navigationAndWaitParameterProviderBindingContext = bindableObject.BindingContext as INavigationAndWaitParameterProvider;
                if (navigationAndWaitParameterProviderBindingContext != null)
                    return navigationAndWaitParameterProviderBindingContext.Provide();
            }

            return null;
        }

        private async static void DoPush(INavigation navigation, Page view, bool useModalNavigation, bool animated)
        {
            if (useModalNavigation)
                await navigation.PushModalAsync(view, animated);
            else
                await navigation.PushAsync(view, animated);
        }

        private async static void DoPop(INavigation navigation, bool useModalNavigation, bool animated)
        {
            if (useModalNavigation)
                await navigation.PopModalAsync(animated);
            else
                await navigation.PopAsync(animated);
        }

        private INavigation GetPageNavigation()
        {
            return _page != null ? _page.Navigation : Application.Current.MainPage.Navigation;
        }

        private static bool CanNavigate(object item, NavigationParameters parameters)
        {
            var confirmNavigationItem = item as IConfirmNavigation;
            if (confirmNavigationItem != null)
                return confirmNavigationItem.CanNavigate(parameters);

            var bindableObject = item as BindableObject;
            if (bindableObject != null)
            {
                var confirmNavigationBindingContext = bindableObject.BindingContext as IConfirmNavigation;
                if (confirmNavigationBindingContext != null)
                    return confirmNavigationBindingContext.CanNavigate(parameters);
            }

            return true;
        }

        private static void OnNavigatedFrom(object page, NavigationParameters parameters)
        {
            var currentPage = page as Page;
            if (currentPage != null)
                InvokeOnNavigationAwareElement(currentPage, v => v.OnNavigatedFrom(parameters));
        }

        private static void OnNavigatedTo(object page, NavigationParameters parameters)
        {
            var currentPage = page as Page;
            if (currentPage != null)
                InvokeOnNavigationAwareElement(page, v => v.OnNavigatedTo(parameters));
        }

        private static void InvokeOnNavigationAwareElement(object item, Action<INavigationAware> invocation)
        {
            var navigationAwareItem = item as INavigationAware;
            if (navigationAwareItem != null)
                invocation(navigationAwareItem);

            var bindableObject = item as BindableObject;
            if (bindableObject != null)
            {
                var navigationAwareDataContext = bindableObject.BindingContext as INavigationAware;
                if (navigationAwareDataContext != null)
                    invocation(navigationAwareDataContext);
            }
        }

        static Dictionary<Type, INavigationPageProvider> _navigationProviderCache = new Dictionary<Type, INavigationPageProvider>();
        
        private Page GetNavigationPageFromProvider(Page sourceView, Page targetView)
        {
            INavigationPageProvider provider = null;
            Type viewType = targetView.GetType();

            if (_navigationProviderCache.ContainsKey(viewType))
            {
                provider = _navigationProviderCache[viewType];
            }
            else
            {
                var navigationPageProvider = viewType.GetTypeInfo().GetCustomAttribute<NavigationPageProviderAttribute>(true);
                if (navigationPageProvider != null)
                {
                    provider = ServiceLocator.Current.GetInstance(navigationPageProvider.Type) as INavigationPageProvider;
                    if (provider == null)
                        throw new InvalidCastException("Could not create the navigation page provider.  Please make sure the navigation page provider implements the INavigationPageProvider interface.");
                }
            }

            if (!_navigationProviderCache.ContainsKey(viewType))
                _navigationProviderCache.Add(viewType, provider);

            if (provider != null)
                return provider.CreatePageForNavigation(sourceView, targetView);

            return null;
        }
    }
}
