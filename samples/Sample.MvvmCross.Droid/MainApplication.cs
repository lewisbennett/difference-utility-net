using Android.App;
using Android.Runtime;
using MvvmCross.Platforms.Android.Views;
using System;

namespace Sample.MvvmCross.Droid;

[Application]
public class MainApplication : MvxAndroidApplication<Setup, Core.App>
{
    public MainApplication()
        : base()
    {
    }

    public MainApplication(IntPtr handle, JniHandleOwnership transfer)
        : base(handle, transfer)
    {
    }
}