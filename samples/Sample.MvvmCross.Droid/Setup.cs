using Microsoft.Extensions.Logging;
using MvvmCross.Platforms.Android.Core;
using Sample.MvvmCross.Core;

namespace Sample.MvvmCross.Droid;

public class Setup : MvxAndroidSetup<App>
{
    protected override ILoggerFactory CreateLogFactory()
    {
        return null;
    }

    protected override ILoggerProvider CreateLogProvider()
    {
        return null;
    }
}