// A Hello World! program in C#.
using System;
using Cake.Core.Annotations;
using Cake.Core.Composition;
using Cake.Core.Diagnostics;

namespace Cake.TFBuildModule
{
    class TFBuildEngineModule : ICakeModule
    {
        public void Register(ICakeContainerRegistrar registrar)
        {
            registrar.RegisterType<TFBuildEngine>().As<ICakeEngine>().Singleton();
        }
    }
}