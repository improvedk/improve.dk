---
permalink: the-cost-of-latent-logging-code
title: The Cost of Latent Logging Code
date: 2009-09-12
tags: [.NET]
---
Logging is an integral part of most applications, whether it's for logging performance metrics or causality data. Avoiding performance hits due to logging can be tricky though as we don't want to spend CPU cycles on the logging infrastructure when logging is disabled, while still keeping the full logging ability when required.

<!-- more -->

Imagine the following scenario in which we want to log an exception in our application:

```csharp
Logger.Log("An error occured at " + DateTime.Now + " on computer " + Environment.MachineName + " in process" + Process.GetCurrentProcess().ProcessName + ".");
```

Inside the Logger.Log method we may have a check for whether logging is enabled like the following:

```csharp
if(LoggingEnabled)
	File.AppendAllText("Log.txt", logMessage + Environment.NewLine);
```

What's the problem? Although we do not touch the file system when logging is disabled, we still incur the rather large overhead of string concatenation and retrieving the machine and process name. Usually this overhead will be even larger depending on what extra information is logged as part of the actual log message. Yes, we could append the machine and process names within the Logger.Log method, but that's beyond the problem I'm describing.

We can avoid this by checking for LoggingEnabled in our actual application code:

```csharp
if(Logger.LoggingEnabled)
	Logger.Log("An error occured at " + DateTime.Now + " on computer " + Environment.MachineName + " in process" + Process.GetCurrentProcess().ProcessName + ".");
```

While this does save us from doing string concatenation and retrieving other data when logging is disabled, it's rather ugly to have logging checks scattered around the application code.

An alternative to sending a log message directly to the Logger.Log method would be to send a Func that fetches the log message if needed:

```csharp
public class Logger
{
	public static void Log(Func<string> message)
	{
		if (LoggingEnabled)
			File.AppendAllText("Log.txt", message() + Environment.NewLine);
	}
}
```

```csharp
Logger.Log(() => "An error occured at " + DateTime.Now + " on computer " + Environment.MachineName + " in process" + Process.GetCurrentProcess().ProcessName + ".");
```

This has the big benefit of only executing the actual logging message functionality if logging is enabled, thus reducing the overhead to a near zero.

While this is rather straight forward as long as the logging is performed synchrounously, there's a pitfall if we perform asynchronous logging. Take the following asynchronous Logger implementation as an example:

```csharp
public static class Logger
{
	private static Queue<Func<string>> logMessages = new Queue<Func<string>>();
	public static bool LoggingEnabled { get; set; }

	public static void Log(Func<string> message)
	{
		if (LoggingEnabled)
			logMessages.Enqueue(message);
	}

	public static void FlushMessages()
	{
		while(logMessages.Count > 0)
			File.AppendAllText("Log.txt", logMessages.Dequeue()() + Environment.NewLine);
	}
}
```

Instead of outputting the log message to the log file immediately when the Log function is called, we now store the actual log messages in a [FIFO](http://en.wikipedia.org/wiki/FIFO_(computing)) queue. At some point we'll call Logger.FlushMessages to flush out the messages to the text file. To optimize the process we'd usually concatenate the messages in a StringBuilder and perform just a single sequential write to disk, but to KISS I'm just writing out the messages one by one.

We'll perform a number of logs using the following code:

```csharp
string date = DateTime.Now.ToString();

Logger.Log("An error occured at " + DateTime.Now + " (" + date + ") on computer " + Environment.MachineName + " in process" + Process.GetCurrentProcess().ProcessName + ".");
```

If you open the log file, you'll notice a discrepancy between the two dates that are logged, while you might expect them to be identical. As the actual lambda function is performed at log flush time instead of log time, the DateTime.Now variable will include the flush moment instead of the original logging moment.

The solution in this case is simple. All we need to do is to store the results of the log Funcs instead of the actual funcs:

```csharp
public static class Logger
{
	private static Queue<string> logMessages = new Queue<string>();
	public static bool LoggingEnabled { get; set; }

	public static void Log(Func<string> message)
	{
		if (LoggingEnabled)
			logMessages.Enqueue(message());
	}

	public static void FlushMessages()
	{
		while(logMessages.Count > 0)
			File.AppendAllText("Log.txt", logMessages.Dequeue() + Environment.NewLine);
	}
}
```

We can still implement asynchronous logging, as long as the actual log message is retrieved synchronously or we make sure the logging Func only references local immutable variables - though the last case kinda destroys the performance gain.

```csharp
string date = DateTime.Now.ToString();

Logger.Log(() => "An error occured at " + date + " on computer " + Environment.MachineName + " in process" + Process.GetCurrentProcess().ProcessName + ".");
```

To sum up the speed gains of using deferrered lambda logging, I've implemented a simple synchronous Logger implementation:

```csharp
public static class Logger
{
	public static bool LoggingEnabled { get; set; }

	public static void Log(string message)
	{
		if (LoggingEnabled)
			File.AppendAllText("Log.txt", message + Environment.NewLine);
	}

	public static void Log(Func<string> message)
	{
		if (LoggingEnabled)
			File.AppendAllText("Log.txt", message() + Environment.NewLine);
	}
}
```

And to perform the actual performance measurements I'm using my [CodeProfiler](http://www.improve.dk/blog/2008/04/16/profiling-code-the-easy-way) class with 1000 iterations of the logging code:

```csharp
class Program
{
	static void Main(string[] args)
	{
		Logger.LoggingEnabled = true;
		var timeWithLoggingEnabled = profileCode();
		var lambdaTimeWithLoggingEnabled = profileLambdaCode();

		Logger.LoggingEnabled = false;
		var timeWithLoggingDisabled = profileCode();
		var lambdaTimeWithLoggingDisabled = profileLambdaCode();

		Console.WriteLine("Logging enabled: " + timeWithLoggingEnabled.TotalMilliseconds);
		Console.WriteLine("Lambda logging enabled: " + lambdaTimeWithLoggingEnabled.TotalMilliseconds);
		Console.WriteLine("Logging disabled: " + timeWithLoggingDisabled.TotalMilliseconds);
		Console.WriteLine("Lambda logging disabled: " + lambdaTimeWithLoggingDisabled.TotalMilliseconds);
		Console.Read();
	}

	static TimeSpan profileCode()
	{
		return CodeProfiler.ProfileAction(() =>
		{
			Logger.Log("An error occured at " + DateTime.Now + " on computer " + Environment.MachineName + " in process" + Process.GetCurrentProcess().ProcessName + ".");
		}, 1000);
	}

	static TimeSpan profileLambdaCode()
	{
		return CodeProfiler.ProfileAction(() =>
		{
			Logger.Log(() => "An error occured at " + DateTime.Now + " on computer " + Environment.MachineName + " in process" + Process.GetCurrentProcess().ProcessName + ".");
		}, 1000);
	}
}
```

The results:

<blockquote>
Logging enabled: 1440,2764  
Lambda logging enabled: 1483,0738  
Logging disabled: 763,1717  
Lambda logging disabled: 0,6516  
</blockquote>

As we can see from the results, even with logging disabled it still costs us 763ms using the normal logging procedure. By deferring the execution of the log message we only incur an overhead of 0,65ms when logging is disabled. When logging is enabled the execution costs are ~identical.
