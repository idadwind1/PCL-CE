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

    private static void CallLog(string msg, LogLevel level)
    {
        switch (level)
        {
            case LogLevel.Fatal:
                CurrentLogger.Fatal(msg);
                break;
            case LogLevel.Error:
                CurrentLogger.Error(msg);
                break;
            case LogLevel.Warning:
                CurrentLogger.Warn(msg);
                break;
            case LogLevel.Info:
                CurrentLogger.Info(msg);
                break;
            case LogLevel.Debug:
                CurrentLogger.Debug(msg);
                break;
            case LogLevel.Trace:
                CurrentLogger.Trace(msg);
                break;
            default:
                CurrentLogger.Info(msg);
                break;
        }
    }
    
    public static void Info(string? module, string msg, InfoLevel level = InfoLevel.Hint)
    {
        CallLog($"{module} {msg}", (LogLevel)level);
        OnLog?.Invoke((LogLevel)level, msg, false, module);
    }
    
    public static void Info(string msg, InfoLevel level = InfoLevel.Hint)
    {
        Info(null, msg, level);
    }
    
    public static void Error(Exception? ex, string? module, string msg, ErrorLevel level = ErrorLevel.Debug)
    {
        CallLog($"{module} {msg}: {ex?.ToString()}", (LogLevel)level);
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
