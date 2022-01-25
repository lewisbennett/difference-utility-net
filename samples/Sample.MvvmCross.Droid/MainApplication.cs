using System;
using Android.App;
using Android.Runtime;
using MvvmCross.Platforms.Android.Views;
using Sample.MvvmCross.Core;

namespace Sample.MvvmCross.Droid;

[Application]
public class MainApplication : MvxAndroidApplication<Setup, App>
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