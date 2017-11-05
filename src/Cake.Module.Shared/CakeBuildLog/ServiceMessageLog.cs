// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Diagnostics.Formatting;
using Cake.Diagnostics;
using System.Text.RegularExpressions;

namespace Cake.Module.Shared
{
    public abstract class ServiceMessageLog : ICakeLog
    {
        private readonly IConsole _console;
        private readonly object _lock;
        private readonly IDictionary<LogLevel, ConsolePalette> _palettes;
        private Func<string, bool> _match;

        public Verbosity Verbosity { get; set; }

        public ServiceMessageLog(IConsole console, System.Text.RegularExpressions.Regex formatExpression, Verbosity verbosity = Verbosity.Normal)
         : this(console, verbosity)
        {
            _match = s => formatExpression.IsMatch(s);
        }

        public ServiceMessageLog(IConsole console, Func<string, bool> match, Verbosity verbosity = Verbosity.Normal) : this(console, verbosity)
        {
            if (match == null) throw new ArgumentNullException(nameof(match));
            _match = match;
        }

        private ServiceMessageLog(IConsole console, Verbosity verbosity) {
            _console = console;
            _lock = new object();
            _palettes = CreatePalette();
            Verbosity = verbosity;
        }

        public virtual void Write(Verbosity verbosity, LogLevel level, string format, params object[] args)
        {
            if (verbosity > Verbosity)
            {
                return;
            }
            lock (_lock)
            {
                try
                {
                    if (_match(format))
                    {
                        var tokens = FormatParser.Parse(format);
                        foreach (var token in tokens)
                        {
                            if (level > LogLevel.Error)
                            {
                                _console.Write("{0}", token.Render(args));
                            }
                            else
                            {
                                _console.WriteError("{0}", token.Render(args));
                            }
                        }
                    }
                    else
                    {
                        var palette = _palettes[level];
                        var tokens = FormatParser.Parse(format);
                        foreach (var token in tokens)
                        {
                            SetPalette(token, palette);
                            if (level > LogLevel.Error)
                            {
                                _console.Write("{0}", token.Render(args));
                            }
                            else
                            {
                                _console.WriteError("{0}", token.Render(args));
                            }
                        }
                    }

                }
                finally
                {
                    _console.WriteLine();
                    _console.ResetColor();
                    if (level > LogLevel.Error)
                    {
                        _console.WriteLine();
                    }
                    else
                    {
                        _console.WriteErrorLine();
                    }
                }
            }
        }

        private void SetPalette(FormatToken token, ConsolePalette palette)
        {
            var property = token as PropertyToken;
            if (property != null)
            {
                _console.BackgroundColor = palette.ArgumentBackground;
                _console.ForegroundColor = palette.ArgumentForeground;
            }
            else
            {
                _console.BackgroundColor = palette.Background;
                _console.ForegroundColor = palette.Foreground;
            }
        }

        private IDictionary<LogLevel, ConsolePalette> CreatePalette()
        {
            var background = _console.BackgroundColor;
            var palette = new Dictionary<LogLevel, ConsolePalette>
            {
                { LogLevel.Fatal, new ConsolePalette(ConsoleColor.Magenta, ConsoleColor.White, ConsoleColor.DarkMagenta, ConsoleColor.White) },
                { LogLevel.Error, new ConsolePalette(ConsoleColor.DarkRed, ConsoleColor.White, ConsoleColor.Red, ConsoleColor.White) },
                { LogLevel.Warning, new ConsolePalette(background, ConsoleColor.Yellow, background, ConsoleColor.Yellow) },
                { LogLevel.Information, new ConsolePalette(background, ConsoleColor.White, ConsoleColor.DarkBlue, ConsoleColor.White) },
                { LogLevel.Verbose, new ConsolePalette(background, ConsoleColor.Gray, background, ConsoleColor.White) },
                { LogLevel.Debug, new ConsolePalette(background, ConsoleColor.DarkGray, background, ConsoleColor.Gray) }
            };
            return palette;
        }
    }
}