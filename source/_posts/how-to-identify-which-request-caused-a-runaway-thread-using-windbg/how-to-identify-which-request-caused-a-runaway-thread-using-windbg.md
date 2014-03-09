permalink: how-to-identify-which-request-caused-a-runaway-thread-using-windbg
title: How to Identify Which Request Caused a Runaway Thread, Using Windbg
date: 2013-05-07
tags: [.NET, IIS, Windbg]
---
When your w3wp process is stuck at 100% like, [like when I used a non-thread-safe Dictionary concurrently](/debugging-in-production-part-1-analyzing-100-cpu-usage-using-windbg/), you may want to identify what request the runaway thread is actually serving. Let me show you how to identify which request caused a runaway thread, using windbg.

<!-- more -->

First you'll want to identify the process ID (PID) of the w3wp process. In my case, that's **102600**:

Taskmgr.png

Next you'll want to start up Windbg (make sure to use the correct bitness (x86 vs x64) that corresponds to the bitness of your process). Once started, press **F6** to open up the *Attach to Process* dialog. Once open, enter your process ID and click OK.

Attach-to-Process.png

Doing so should bring up the Command window, ready for your command:

Windbg11.png

As the first thing, start out by loading the [Son of Strike](http://msdn.microsoft.com/en-us/library/bb190764.aspx) extension, allowing us to debug managed code.

```csharp
0:039> .loadby sos clr
```

Then continue by running the !runaway command to get a list of runaway (basically threads using lots of CPU) threads:

```csharp
0:039> !runaway

 User Mode Time
  Thread       Time
  20:14930      0 days 0:21:44.261
  21:15204      0 days 0:21:00.878
  27:19d48      0 days 0:04:23.860
  32:18748      0 days 0:02:59.260
  31:18bcc      0 days 0:02:19.277
  30:19d80      0 days 0:01:44.083
  25:19ec0      0 days 0:01:32.446
  24:16534      0 days 0:01:31.135
  29:19a80      0 days 0:01:08.297
  23:19110      0 days 0:00:30.591
   6:19b40      0 days 0:00:00.109
  26:18a14      0 days 0:00:00.015
   0:19dcc      0 days 0:00:00.015
  39:16fa8      0 days 0:00:00.000
  ...
```

Threads 20 & 21 seem to be the interesting ones. Let's start out by selecting thread #20 as the active thread:

```csharp
0:039> ~20s

000007fe`913a15d9 3bc5            cmp     eax,ebp
```

Once selected, we can analyze the stack and its parameters by running the !CLRStack command with the -p parameter:

```csharp
0:020> !CLRStack -p

OS Thread Id: 0x14930 (20)
        Child SP               IP Call Site
000000000dccdb00 000007fe913a15d9 System.Collections.Generic.Dictionary`2[[System.Int16, mscorlib],[System.__Canon, mscorlib]].FindEntry(Int16)
    PARAMETERS:
        this = <no data>
        key = <no data>

000000000dccdb50 000007fe913a14c0 System.Collections.Generic.Dictionary`2[[System.Int16, mscorlib],[System.__Canon, mscorlib]].get_Item(Int16)
    PARAMETERS:
        this = <no data>
        key = <no data>

000000000dccdb80 000007fe91421cbb iPaper.BL.Backend.Modules.Languages.LanguageCache.GetLanguageByID(Int32, iPaper.BL.Backend.Infrastructure.PartnerConfiguration.IPartnerConfig) [e:\iPaperCMS\BL\Backend\Modules\Languages\LanguageCache.cs @ 44]
    PARAMETERS:
        languageID (0x000000000dccdc20) = 0x0000000000000001
        partnerConfig (0x000000000dccdc28) = 0x00000000fffc3e50

000000000dccdc20 000007fe91421dfa iPaper.BL.Backend.Modules.Languages.Language.GetFontFileForLanguage(Int32, iPaper.BL.Backend.Infrastructure.PartnerConfiguration.IPartnerConfig) [e:\iPaperCMS\BL\Backend\Modules\Languages\Language.cs @ 37]
    PARAMETERS:
        languageID (0x000000000dccdc70) = 0x0000000000000001
        partnerConfig (0x000000000dccdc78) = 0x00000000fffc3e50

000000000dccdc70 000007fe91417400 iPaper.Web.FlexFrontend.BL.Common.CachedUrlInformation.GetFromUrlDirectoryPath(System.String, System.String, iPaper.BL.Backend.Infrastructure.PartnerConfiguration.IPartnerConfig) [e:\iPaperCMS\Frontend\BL\Common\CachedUrlInformation.cs @ 89]
    PARAMETERS:
        url (0x000000000dccde80) = 0x00000003fff27e30
        host (0x000000000dccde88) = 0x00000003fff29618
        partnerConfig (0x000000000dccde90) = 0x00000000fffc3e50

000000000dccde80 000007fe91417576 iPaper.Web.FlexFrontend.BL.Common.CachedUrlInformation.GetFromHttpContext(System.String, System.Web.HttpContext, iPaper.BL.Backend.Infrastructure.PartnerConfiguration.IPartnerConfig) [e:\iPaperCMS\Frontend\BL\Common\CachedUrlInformation.cs @ 122]
    PARAMETERS:
        paperPath (0x000000000dcce010) = 0x00000003fff27e30
        context (0x000000000dcce018) = 0x00000000fffa6040
        partnerConfig (0x000000000dcce020) = 0x00000000fffc3e50

000000000dcce010 000007fe91415529 iPaper.Web.FlexFrontend.BL.RequestHandler.RequestHandler.loadFrontendContext(System.String) [e:\iPaperCMS\Frontend\BL\RequestHandler\RequestHandler.cs @ 469]
    PARAMETERS:
        this (0x000000000dcce260) = 0x00000000fffa9590
        paperPath (0x000000000dcce268) = 0x00000003fff27e30

000000000dcce260 000007fe91414b73 iPaper.Web.FlexFrontend.BL.RequestHandler.RequestHandler.context_PostAcquireRequestState(System.Object, System.EventArgs) [e:\iPaperCMS\Frontend\BL\RequestHandler\RequestHandler.cs @ 95]
    PARAMETERS:
        this (0x000000000dcce5f0) = 0x00000000fffa9590
        sender (0x000000000dcce5f8) = 0x00000000fffa8a50
        e (0x000000000dcce600) = 0x00000000fffaebb0

000000000dcce5f0 000007fedb72c520 System.Web.HttpApplication+SyncEventExecutionStep.System.Web.HttpApplication.IExecutionStep.Execute()
    PARAMETERS:
        this = <no data>

000000000dcce650 000007fedb70b745 System.Web.HttpApplication.ExecuteStep(IExecutionStep, Boolean ByRef)
    PARAMETERS:
        this (0x000000000dcce6f0) = 0x00000000fffa8a50
        step (0x000000000dcce6f8) = 0x00000000fffabc28
        completedSynchronously (0x000000000dcce700) = 0x000000000dcce77a

000000000dcce6f0 000007fedb72a4e1 System.Web.HttpApplication+PipelineStepManager.ResumeSteps(System.Exception)
    PARAMETERS:
        this (0x000000000dcce7d0) = 0x00000000fffac718
        error = <no data>

000000000dcce7d0 000007fedb70b960 System.Web.HttpApplication.BeginProcessRequestNotification(System.Web.HttpContext, System.AsyncCallback)
    PARAMETERS:
        this = <no data>
        context = <no data>
        cb = <no data>

000000000dcce820 000007fedb704c8e System.Web.HttpRuntime.ProcessRequestNotificationPrivate(System.Web.Hosting.IIS7WorkerRequest, System.Web.HttpContext)
    PARAMETERS:
        this (0x000000000dcce8c0) = 0x00000000fff3fb20
        wr (0x000000000dcce8c8) = 0x00000000fffa5eb0
        context (0x000000000dcce8d0) = 0x00000000fffa6040

000000000dcce8c0 000007fedb70e771 System.Web.Hosting.PipelineRuntime.ProcessRequestNotificationHelper(IntPtr, IntPtr, IntPtr, Int32)
    PARAMETERS:
        rootedObjectsPointer = <no data>
        nativeRequestContext (0x000000000dccea58) = 0x0000000000ccccc0
        moduleData = <no data>
        flags = <no data>

000000000dccea50 000007fedb70e2c2 System.Web.Hosting.PipelineRuntime.ProcessRequestNotification(IntPtr, IntPtr, IntPtr, Int32)
    PARAMETERS:
        rootedObjectsPointer = <no data>
        nativeRequestContext = <no data>
        moduleData = <no data>
        flags = <no data>

000000000dcceaa0 000007fedbe6b461 DomainNeutralILStubClass.IL_STUB_ReversePInvoke(Int64, Int64, Int64, Int32)
    PARAMETERS:
        <no data>
        <no data>
        <no data>
        <no data>

000000000dccf298 000007fef0a9334e [InlinedCallFrame: 000000000dccf298] System.Web.Hosting.UnsafeIISMethods.MgdIndicateCompletion(IntPtr, System.Web.RequestNotificationStatus ByRef)
000000000dccf298 000007fedb7b9c4b [InlinedCallFrame: 000000000dccf298] System.Web.Hosting.UnsafeIISMethods.MgdIndicateCompletion(IntPtr, System.Web.RequestNotificationStatus ByRef)
000000000dccf270 000007fedb7b9c4b DomainNeutralILStubClass.IL_STUB_PInvoke(IntPtr, System.Web.RequestNotificationStatus ByRef)
    PARAMETERS:
        <no data>
        <no data>

000000000dccf340 000007fedb70e923 System.Web.Hosting.PipelineRuntime.ProcessRequestNotificationHelper(IntPtr, IntPtr, IntPtr, Int32)
    PARAMETERS:
        rootedObjectsPointer = <no data>
        nativeRequestContext = <no data>
        moduleData = <no data>
        flags = <no data>

000000000dccf4d0 000007fedb70e2c2 System.Web.Hosting.PipelineRuntime.ProcessRequestNotification(IntPtr, IntPtr, IntPtr, Int32)
    PARAMETERS:
        rootedObjectsPointer = <no data>
        nativeRequestContext = <no data>
        moduleData = <no data>
        flags = <no data>

000000000dccf520 000007fedbe6b461 DomainNeutralILStubClass.IL_STUB_ReversePInvoke(Int64, Int64, Int64, Int32)
    PARAMETERS:
        <no data>
        <no data>
        <no data>
        <no data>

000000000dccf768 000007fef0a935a3 [ContextTransitionFrame: 000000000dccf768]
```

This returns the full stack with a lot of frames that we're not really interested in. What we're looking for is the first instance of an HttpContext. If we start from the bottom and work our way up, this seems to be the first time an HttpContext is present:

```csharp
000000000dcce820 000007fedb704c8e System.Web.HttpRuntime.ProcessRequestNotificationPrivate(System.Web.Hosting.IIS7WorkerRequest, System.Web.HttpContext)
    PARAMETERS:
        this (0x000000000dcce8c0) = 0x00000000fff3fb20
        wr (0x000000000dcce8c8) = 0x00000000fffa5eb0
        context (0x000000000dcce8d0) = 0x00000000fffa6040
```

Knowing that the HttpContext contains a reference to an HttpRequest, and that HttpRequest contains the RawUrl string value, we'll start digging in. Start out by dumping the HttpContext object using the !do command:

```csharp
0:020> !do 0x00000000fffa6040

Name:        System.Web.HttpContext
MethodTable: 000007fedb896398
EEClass:     000007fedb4882e0
Size:        416(0x1a0) bytes
File:        C:\Windows\Microsoft.Net\assembly\GAC_64\System.Web\v4.0_4.0.0.0__b03f5f7f11d50a3a\System.Web.dll
Fields:
              MT    Field   Offset                 Type VT     Attr            Value Name
000007fedb897c80  40010a3        8 ...IHttpAsyncHandler  0 instance 0000000000000000 _asyncAppHandler
000007fedb88e618  40010a4      158         System.Int32  1 instance                0 _asyncPreloadModeFlags
000007feef9fdc30  40010a5      168       System.Boolean  1 instance                0 _asyncPreloadModeFlagsSet
000007fedb895610  40010a6       10 ...b.HttpApplication  0 instance 00000000fffa8a50 _appInstance
000007fedb897ce8  40010a7       18 ....Web.IHttpHandler  0 instance 00000003fff28c20 _handler
000007fedb898170  40010a8       20 ...m.Web.HttpRequest  0 instance 00000000fffa61f8 _request
000007fedb898550  40010a9       28 ....Web.HttpResponse  0 instance 00000000fffa6378 _response
000007fedb893cb0  40010aa       30 ...HttpServerUtility  0 instance 00000003fff27ed8 _server
000007feefa05ac0  40010ab       38 ...Collections.Stack  0 instance 0000000000000000 _traceContextStack
000007fedb8a41d8  40010ac       40 ....Web.TraceContext  0 instance 0000000000000000 _topTraceContext
000007feefa00548  40010ad       48 ...ections.Hashtable  0 instance 00000000fffab198 _items
000007feef9f85e0  40010ae       50 ...ections.ArrayList  0 instance 0000000000000000 _errors
000007feef9fc588  40010af       58     System.Exception  0 instance 0000000000000000 _tempError
...
```

This contains a lot of fields (some of which I've snipped out). The interesting part however, is this line:

```csharp
000007fedb898170  40010a8       20 ...m.Web.HttpRequest  0 instance 00000000fffa61f8 _request
```

This contains a pointer to the HttpRequest instance. Let's try dumping that one:

```csharp
0:020> !do 00000000fffa61f8 

Name:        System.Web.HttpRequest
MethodTable: 000007fedb898170
EEClass:     000007fedb488c00
Size:        384(0x180) bytes
File:        C:\Windows\Microsoft.Net\assembly\GAC_64\System.Web\v4.0_4.0.0.0__b03f5f7f11d50a3a\System.Web.dll
Fields:
              MT    Field   Offset                 Type VT     Attr            Value Name
000007fedb89aa30  4001150        8 ...HttpWorkerRequest  0 instance 00000000fffa5eb0 _wr
000007fedb896398  4001151       10 ...m.Web.HttpContext  0 instance 00000000fffa6040 _context
...
000007fee6e1dc48  4001165       90           System.Uri  0 instance 00000003fff29588 _url
000007fee6e1dc48  4001166       98           System.Uri  0 instance 0000000000000000 _referrer
000007fedb900718  4001167       a0 ...b.HttpInputStream  0 instance 0000000000000000 _inputStream
000007fedb8c43d0  4001168       a8 ...ClientCertificate  0 instance 0000000000000000 _clientCertificate
000007feefa07e90  4001169       b0 ...l.WindowsIdentity  0 instance 0000000000000000 _logonUserIdentity
000007fedb8d7fd0  400116a       b8 ...ng.RequestContext  0 instance 0000000000000000 _requestContext
000007feef9fc358  400116b       c0        System.String  0 instance 00000000fffa64f0 _rawUrl
000007feefa008b8  400116c       c8     System.IO.Stream  0 instance 0000000000000000 _readEntityBodyStream
000007fedb8d5ac8  400116d      160         System.Int32  1 instance                0 _readEntityBodyMode
000007fedb8bbcb0  400116e       d0 ...atedRequestValues  0 instance 00000003fff27fe8 _unvalidatedRequestValues
...
```

Once again there are a lot of fields that we don't care about. The interesting one is this one:

```csharp
000007feef9fc358  400116b       c0        System.String  0 instance 00000000fffa64f0 _rawUrl
```

Dumping the RawUrl property reveals the actual URL that made the request which eventually ended up causing a runaway thread:

```csharp
0:020> !do 00000000fffa64f0 

Name:        System.String
MethodTable: 000007feef9fc358
EEClass:     000007feef363720
Size:        150(0x96) bytes
File:        C:\Windows\Microsoft.Net\assembly\GAC_64\mscorlib\v4.0_4.0.0.0__b77a5c561934e089\mscorlib.dll
String:      /Catalogs/SomeClient/Uge45/Image.ashx?PageNumber=1&ImageType=Thumb
Fields:
              MT    Field   Offset                 Type VT     Attr            Value Name
000007feef9ff108  40000aa        8         System.Int32  1 instance               62 m_stringLength
000007feef9fd640  40000ab        c          System.Char  1 instance               2f m_firstChar
000007feef9fc358  40000ac       18        System.String  0   shared           static Empty
                                 >> Domain:Value  0000000001ec80e0:NotInit  0000000001f8e840:NotInit
```

And there we go! The offending URL seems to be:

```csharp
/Catalogs/SomeClient/Uge45/Image.ashx?PageNumber=1&ImageType=Thumb
```

If you want the complete URL, including hostname, you could dig your way into the _url field on the HttpRequest object and work your way from there. In just the same way you can dig into pretty much any object, whether it's in your code or in the IIS codebase.
