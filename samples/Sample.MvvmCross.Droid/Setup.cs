using Microsoft.Extensions.Logging;
using MvvmCross.Platforms.Android.Core;

namespace Sample.MvvmCross.Droid
{
    public class Setup : MvxAndroidSetup<Core.App>
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
}