﻿
using System.Threading.Tasks;

namespace Prism.Navigation
{
    /// <summary>
    /// Provides page based navigation for ViewModels.
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// Navigates to the most recent entry in the back navigation history by popping the calling Page off the navigation stack.
        /// </summary>
        /// <param name="useModalNavigation">If <c>true</c> uses PopModalAsync, if <c>false</c> uses PopAsync</param>
        /// <param name="animated">If <c>true</c> the transition is animated, if <c>false</c> there is no animation on transition.</param>
        void GoBack(bool useModalNavigation = true, bool animated = true);

        /// <summary>
        /// Initiates navigation to the target specified by the <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type which will be used to identify the name of the navigation target.</typeparam>
        /// <param name="parameters">The navigation parameters</param>
        /// <param name="useModalNavigation">If <c>true</c> uses PopModalAsync, if <c>false</c> uses PopAsync</param>
        /// <param name="animated">If <c>true</c> the transition is animated, if <c>false</c> there is no animation on transition.</param>
        void Navigate<T>(NavigationParameters parameters = null, bool useModalNavigation = true, bool animated = true);

        /// <summary>
        /// Initiates navigation to the target specified by the <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name of the target to navigate to.</param>
        /// <param name="parameters">The navigation parameters</param>
        /// <param name="useModalNavigation">If <c>true</c> uses PopModalAsync, if <c>false</c> uses PopAsync</param>
        /// <param name="animated">If <c>true</c> the transition is animated, if <c>false</c> there is no animation on transition.</param>
        void Navigate(string name, NavigationParameters parameters = null, bool useModalNavigation = true, bool animated = true);

        Task<NavigationParameters> NavigateAndWait<T>(NavigationParameters parameters = null, bool useModalNavigation = true,
            bool animated = true);

        Task<NavigationParameters> NavigateAndWait(string name, NavigationParameters parameters = null,
            bool useModalNavigation = true, bool animated = true);
    }
}
