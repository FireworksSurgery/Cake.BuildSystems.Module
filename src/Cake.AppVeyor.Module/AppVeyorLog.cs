using System;
using System.Text.RegularExpressions;
using Cake.Common.Build.AppVeyor;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Cake.Module.Shared;

namespace Cake.AppVeyor.Module
{
    public sealed class AppVeyorLog : ServiceMessageLog
    {
        private readonly AppVeyorProvider _ap;

        public AppVeyorLog(ICakeEnvironment cakeEnvironment, IProcessRunner processRunner, IConsole console, Verbosity verbosity = Verbosity.Normal) : base(console, s => false, verbosity)
        {
            _ap = new Cake.Common.Build.AppVeyor.AppVeyorProvider(cakeEnvironment, processRunner, this);
        }

        public override void Write(Verbosity verbosity, LogLevel level, string format, params object[] args)
        {
            if (!string.IsNullOrWhiteSpace(System.Environment.GetEnvironmentVariable("APPVEYOR")))
            {
                switch (level)
                {
                    case LogLevel.Fatal:
                    case LogLevel.Error:
                        _ap.AddErrorMessage(format, args);
                        break;
                    case LogLevel.Warning:
                        _ap.AddWarningMessage(format, args);
                        break;
                    case LogLevel.Information:
                        _ap.AddInformationalMessage(format, args);
                        break;
                    case LogLevel.Verbose:
                    case LogLevel.Debug:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(level), level, null);
                }
            }
            base.Write(verbosity, level, format, args);
        }
    }

}
