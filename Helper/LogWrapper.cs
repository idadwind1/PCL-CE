using System;
using System.IO;
using PCL.Core.Helper.Logger;

namespace PCL.Core.Helper;

public enum LogLevel
{
    Trace,
    Debug,
    Info,
    Warning,
    Error,
    Fatal
}

public delegate void LogHandler(LogLevel level, string msg, bool isError = false, string? module = null, Exception? ex = null);

public enum InfoLevel
{
    Trace = LogLevel.Trace,
    Debug = LogLevel.Debug,
    Hint = LogLevel.Info,
    MsgBox = LogLevel.Warning
}

public enum ErrorLevel
{
    Debug = LogLevel.Debug,
    Hint = LogLevel.Info,
    MsgBox = LogLevel.Warning,
    Feedback = LogLevel.Error,
    Fatal = LogLevel.Fatal,
}

public static class LogWrapper
{
    public static event LogHandler? OnLog;

    public static Logger.Logger CurrentLogger = new(new LoggerConfiguration(
        Path.Combine(Environment.CurrentDirectory, "PCL", "Log"),
        LoggerSegmentMode.BySize,
        5 * 1024 * 1024,
        null,
        true,
        10));
    
    public static void Info(string? module, string msg, InfoLevel level = InfoLevel.Hint)
    {
        switch (level)
        {
            case InfoLevel.Debug:
                CurrentLogger.Debug($"{module} {msg}");
                break;
            case InfoLevel.Hint:
                CurrentLogger.Info($"{module} {msg}");
                break;
            case InfoLevel.MsgBox:
                CurrentLogger.Warn($"{module} {msg}");
                break;
            case InfoLevel.Trace:
                CurrentLogger.Trace($"{module} {msg}");
                break;
            default:
                CurrentLogger.Info($"{module} {msg}");
                break;
        }
        OnLog?.Invoke((LogLevel)level, msg, false, module);
    }
    
    public static void Info(string msg, InfoLevel level = InfoLevel.Hint)
    {
        Info(null, msg, level);
    }
    
    public static void Error(Exception? ex, string? module, string msg, ErrorLevel level = ErrorLevel.Debug)
    {
        switch (level)
        {
            case ErrorLevel.Debug:
                CurrentLogger.Debug($"{module} {msg}: {ex?.Message}");
                break;
            case ErrorLevel.Fatal:
            case ErrorLevel.Feedback:
            case ErrorLevel.Hint:
            case ErrorLevel.MsgBox:
                CurrentLogger.Fatal($"{module} {msg}: {ex?.Message}");
                break;
            default:
                CurrentLogger.Error($"{module} {msg}: {ex?.Message}");
                break;
        }
        OnLog?.Invoke((LogLevel)level, msg, true, module, ex);
    }
    
    public static void Error(Exception? ex, string msg, ErrorLevel level = ErrorLevel.Debug)
    {
        Error(ex, null, msg, level);
    }

    public static void Error(string? module, string msg, ErrorLevel level = ErrorLevel.Debug)
    {
        Error(null, module, msg, level);
    }

    public static void Error(string msg, ErrorLevel level = ErrorLevel.Debug)
    {
        Error((string?)null, msg, level);
    }
    
    public static void Debug(string? module, string msg)
    {
        Info(module, msg, InfoLevel.Debug);
    }
    
    public static void Debug(string msg)
    {
        Debug(null, msg);
    }
    
    public static void Trace(string? module, string msg)
    {
        Info(module, msg, InfoLevel.Trace);
    }
    
    public static void Trace(string msg)
    {
        Trace(null, msg);
    }
}
