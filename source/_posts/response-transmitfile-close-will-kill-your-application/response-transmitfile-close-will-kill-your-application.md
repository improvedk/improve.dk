---
permalink: response-transmitfile-close-will-kill-your-application
title: Response.TransmitFile + Close Will Kill Your Application
date: 2008-03-29
tags: [.NET]
---
Just before last weekend I noticed that a website I'm responsible for started spitting out "Server is busy" messages, not something you want to see on a website with millions of visitors per day. The quickfix was to recycle the application pool, and thus I solved the symptoms by setting a 15 mins recycle cycle on all the application pools. Not exactly optimal, but sometimes pissing your pants is the way to go.

<!-- more -->

The first step I made to analyze what was causing this is the Performance Monitor tool. We weren't experiencing above average traffic, so that couldn't explain it. What first struck me was the the "ASP.NETRequests Queued" queue was 0, not 5000+ as I'd expected! That meant the requests were not being queued, so the server didn't have trouble handling the requests themselves. The reason was to be found in the "ASP.NETRequests Current" counter. This was constantly rising even though the CPU, memory and disk counters looked fine. It obviously didn't look like a performance problem, more like a configuration issue. So I increased the appQueueRequestLimit to 20k and set the recycle cycle to 30 minutes, at most the "ASP.NETRequests Current" went to about 10k before being recycled and thus reset to 0.

Now, that didn't fix the problem, just the symptom. We hadn't experienced this issue previously, so I thought back at what changes had been made in the latest release version. The primary functionality of the system is to serve images, thus we have an Image.ashx file with a responsibility of serving the images as well as logging various parameters of the request. The previous system version had a funtionality like so:

* Find image path
* Response.TransmitFile()
* Logging

The disadvantage of doing it that way is that the client will not have the image served before the statistics have been logged, even though that's purely a serverside functionality. I wanted the client to receive the image as quickly as possible, and then letting the server continue its job afterwards. The obvious solution is to spawn a new thread doing the logging, but with the amount of requests we've got, I really don't want to spawn any more threads than absolutely neccessary, excessive context switching will have a negative impact when the thread count gets high enough. So the new version functioned like this:

* Find image path
* Response.TransmitFile()
* Response.Flush()
* Response.Close()
* Logging

This had the great advantage that the client receives the image immediatly while the server continues logging afterwards. We use only a single thread, the actual request thread. A friend of mine pointed out I might want to move the logging out of the ASP.NET worker process so as to not block incoming requests. The thing is, this will require new thread spawning, and I really don't mind blocking a worker process as we can easily tune the amount of concurrent worker processes, and the "Server too busy" functionality is actually there for a reason - I don't wanna end up in a situation where the server is running a million logging threads but still accepting new connections willingly - in _that_ case, I'd really like the server to block new requests.

Anyways, although this looked good, this was the sole reason for the "Server too busy" errors we were experiencing! After some testing I discovered that if you call Response.TransmitFile() and then afterwards call Response.Close(), the request process is stuck! It will simply keep on living, and thus the "ASP.NETRequests Current" counter will keep increasing. It will not be removed until a pool recycle event is fired! This does not happen if you use Response.WriteFile, Response.BinaryWrite or if you manually stream the file, only if you use TransmitFile!

### This will kill your application:

```csharp
protected void Page_Load(object sender, EventArgs e)
{
	Response.Buffer = false;
	Response.TransmitFile("Tree.jpg");
	Response.Close();
}
```

### But this won't:

```csharp
protected void Page_Load(object sender, EventArgs e)
{
	Response.WriteFile("Tree.jpg");
	Response.Flush();
	Response.Close();
}

protected void Page_Load(object sender, EventArgs e)
{
	Response.BinaryWrite(File.ReadAllBytes(Server.MapPath("Tree.jpg")));
	Response.Flush();
	Response.Close();
}

protected void Page_Load(object sender, EventArgs e)
{
	int chunkSize = 64;
	byte[] buffer = new byte[chunkSize];
	int offset = 0;
	int read = 0;
	using (FileStream fs = File.Open(Server.MapPath("Tree.jpg"), FileMode.Open, FileAccess.Read, FileShare.Read))
	{
		while ((read = fs.Read(buffer, offset, chunkSize)) > 0)
		{
			Response.OutputStream.Write(buffer, 0, read);
			Response.Flush();
		}
	}

	Response.Close();
}
```

I can replicate the exact same errors on Server 2003 with IIS running i *32 mode, Vista x64 and Server 2003 in x64 mode. It does not matter if you're running ASPX pages or ASHX HttpHandlers, same problem.

I used this code snippet to get a list of the current active requests in IIS (to verify that the "ASP.NETRequests Current" and "W3SVC_W3WPActive Requests" are not lying:

```csharp
ServerManager iisManager = new ServerManager();

foreach (WorkerProcess w3wp in iisManager.WorkerProcesses)
{
	Console.WriteLine("W3WP ({0})", w3wp.ProcessId);

	foreach (Request request in w3wp.GetRequests(0).Where(req => req.Url == "/default.aspx"))
	{
		Console.WriteLine("URL: " + request.Url);
		Console.WriteLine("TimeElapsed: " + request.TimeElapsed);
		Console.WriteLine("TimeInState: " + request.TimeInState);
		Console.WriteLine("TimeInModule: " + request.TimeInModule);
		Console.WriteLine("CurrentModule: " + request.CurrentModule);
		Console.WriteLine("PipelineState: " + request.PipelineState);
		Console.WriteLine();
	}
}
```

```
W3WP (7580)
URL: /default.aspx
TimeElapsed: 4223509
TimeInState: 4223509
TimeInModule: 4223509
CurrentModule: IsapiModule
PipelineState: ExecuteRequestHandler

URL: /default.aspx
TimeElapsed: 2529463
TimeInState: 2529463
TimeInModule: 2529463
CurrentModule: IsapiModule
PipelineState: ExecuteRequestHandler

URL: /default.aspx
TimeElapsed: 2527809
TimeInState: 2527809
TimeInModule: 2527809
CurrentModule: IsapiModule
PipelineState: ExecuteRequestHandler

URL: /default.aspx
TimeElapsed: 2521117
TimeInState: 2521117
TimeInModule: 2521117
CurrentModule: IsapiModule
PipelineState: ExecuteRequestHandler

URL: /default.aspx
TimeElapsed: 2516562
TimeInState: 2516562
TimeInModule: 2516562
CurrentModule: IsapiModule
PipelineState: ExecuteRequestHandler

URL: /default.aspx
TimeElapsed: 2515470
TimeInState: 2515470
TimeInModule: 2515470
CurrentModule: IsapiModule
PipelineState: ExecuteRequestHandler

URL: /default.aspx
TimeElapsed: 2514378
TimeInState: 2514378
TimeInModule: 2514378
CurrentModule: IsapiModule
PipelineState: ExecuteRequestHandler

URL: /default.aspx
TimeElapsed: 2291749
TimeInState: 2291749
TimeInModule: 2291749
CurrentModule: IsapiModule
PipelineState: ExecuteRequestHandler
```

So obviously the requests are there, they're just stale.

If we take a look at an [IISTrace](http://iismonitor.motobit.com/) trace, we can see all of the requests in the "Send data" state. They have all sent all the data and no further data is being sent, but they're still stuck in the "Send data" state:

iistrace_2.jpg

For all the other ways to send the file, the request exits the Send data state as soon as all processing is done (that is, not directly after Response.Close). Calling Response.End has no influence.

## Symptoms

You may be experiencing this problem without knowing it. Unless you have a some load on your site, chances are you will never actually see this problem. While the requests will go stale and continue to live, a recycle event will kill them off as the process is closed. But you will see this in your System log:

A process serving application pool 'Classic .NET AppPool' exceeded time limits during shut down. The process id was '13304'.

Since the requests continue living, recycling the pool will time out and thus force the process to shut down, and thereby generating the above event. This may lead to increased memory usage depending on your recycle settings. So unless you have more requests than the Request queue limit setting on your application pool, within the recycle period, you will not notice this problem.

## Fix

The easiest way to get around this problem (bug?) is to just spawn a new thread doing the logging so the main thread will complete right after TransmitFile. In most cases the logging operation will be rather fast so the threads will be shortlived and thus not create too many concurrent threading operations.

```csharp
Response.Buffer = false;
Response.TransmitFile("Tree.jpg");

Thread t = new Thread(delegate()
{
	// Logging
});
t.Start();
```

## Bonus code

Jonathan Gilbert posted a couple of great comments regarding spawning your own threads in the process and the possibility of extracing the actual logging process into a separate service. Since my blogs comments suck in regards to posting code, here are his code parts:

```csharp
static object log_sync = new object();
static Queue<LogData> log_queue = new Queue<LogData>();
static bool log_thread_running = false;

static void post_log_entry(LogData log_entry)
{
	lock (log_sync)
	{
		log_queue.Enqueue(log_entry);

		if (log_thread_running)
			Monitor.PulseAll(log_sync);
		else
			new Thread(log_thread_proc).Start();
	}
}

static void log_thread_proc()
{
	lock (log_sync)
	{
		if (log_thread_running)
			return;

		log_thread_running = true;

		try
		{
			while (true)
			{
				while (log_queue.Count == 0)
					Monitor.Wait(log_sync);

				LogData one_item = null;
				List<LogData> multiple_items = null;

				if (log_queue.Count == 1)
					one_item = log_queue.Dequeue();
				else
				{
					multiple_items = new List<LogData>(log_queue);
					log_queue.Clear();
				}

				// The following block: Exit; try/finally{Enter}
				// ..is the logical inverse of a lock() block. :-)
				Monitor.Exit(log_sync);

				try
				{
					if (one_item != null)
						process_log_entry(one_item);

					if (multiple_items != null)
						foreach (LogData item in multiple_items)
							process_log_entry(item);
				}
				finally
				{
					Monitor.Enter(log_sync);
				}
			}
		}
		catch (Exception e)
		{
			// TODO: log this unexpected error
		}
		finally
		{
			log_thread_running = false;
		}
	}
}
```

```csharp
static object log_sync = new object();
static BinaryFormatter log_formatter = new BinaryFormatter(); // in System.Runtime.Serialization.Formatters.Binary
static Stream log_stream;

static void post_log_entry(LogData log_entry)
{
	lock (log_sync)
	{
		if (log_writer == null)
		{
			Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

			// In practice, I would let the OS pick the port number when binding in the Windows Service
			// and write it to a central location that the ASP.NET process can read from.
			socket.Connect(new IPEndPoint(IPAddress.Loopback, SecretPortNumber));

			log_stream = new NetworkStream(socket, true);
		}

		log_formatter.Serialize(log_stream, log_entry);
	}
}
```

```csharp
class LogService : System.ServiceProcess.ServiceBase
{
	static void Main(string[] args)
	{
		if ((args.Length > 0) && string.Equals(args[0], "/console", StringComparison.InvariantCultureIgnoreCase))
		{
			LogService service = new LogService();

			service.StartDirect();
			Console.WriteLine("Press enter to stop debugging");
			Console.ReadLine();
			service.StopDirect();
		}
		else
			ServiceBase.Run(new LogService());
	}

	LogService()
	{
		ServiceName = "LogService";
		CanStop = true;
	}

	public void StartDirect()
	{
		OnStart(null);
	}

	public void StopDirect()
	{
		OnStop();
	}

	protected override void OnStart(string[] args)
	{
		socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

		// Again, in implementation, change this to bind to port 0 and then after the Bind call
		// has succeeded, read the port number back from the LocalEndPoint property and write it
		// to a place where the ASP.NET side can read it.
		socket.Bind(new IPEndPoint(IPAddress.Loopback, SecretPortNumber));

		socket.Listen(5);

		shutdown = false;

		Thread main_thread = new Thread(main_thread_proc);

		main_thread.IsBackground = true;
		main_thread.Start();
	}

	protected override void OnStop()
	{
		shutdown = true;
	}

	Socket socket;
	bool shutdown;

	void main_thread_proc()
	{
		BinaryFormatter log_formatter = new BinaryFormatter();

		using (NetworkStream log_stream = new NetworkStream(socket, true))
		{
			while (!shutdown)
			{
				LogData log_entry = (LogData)log_formatter.Deserialize(stream);

				process_log_entry(log_entry);
			}
		}
	}
}
```

## Downloads

[ResponseCloseTest.zip - Sample code](ResponseCloseTest.zip)
