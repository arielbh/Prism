using System;
using Android.App;
using Xamarin.Forms.Platform.Android;
using Android.OS;

namespace Prism.Forms.Android
{

	public class PrismFormsAppCompatActivity : FormsAppCompatActivity, Application.IActivityLifecycleCallbacks
	{

		public PrismFormsAppCompatActivity ()
		{
		}

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);
			Application.RegisterActivityLifecycleCallbacks (this);

		}

		public override void OnBackPressed ()
		{
			base.OnBackPressed ();
			OnBackButtonEvent ();

		}

		public event Action OnBackButtonEvent = delegate {};

		public void OnActivityCreated (Activity activity, global::Android.OS.Bundle savedInstanceState)
		{
		}

		public void OnActivityDestroyed (Activity activity)
		{
		}

		public void OnActivityPaused (Activity activity)
		{
		}

		public void OnActivityResumed (Activity activity)
		{
		}

		public void OnActivitySaveInstanceState (Activity activity, global::Android.OS.Bundle outState)
		{
		}

		public void OnActivityStarted (Activity activity)
		{
		}

		public void OnActivityStopped (Activity activity)
		{
		}
	}
}
