using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.Composition;
using Cake.Core.Diagnostics;
using Cake.Module.Shared;
using Cake.AppVeyor.Module;

[assembly: CakeModule(typeof(AppVeyorModule))]

namespace Cake.AppVeyor.Module
{
    class AppVeyorModule : ICakeModule
    {
        public void Register(ICakeContainerRegistrar registrar)
        {
            if (!string.IsNullOrWhiteSpace(System.Environment.GetEnvironmentVariable("APPVEYOR")))
            {
                registrar.RegisterType<AppVeyorLog>().As<ICakeLog>().Singleton();
            }
        }
    }
}