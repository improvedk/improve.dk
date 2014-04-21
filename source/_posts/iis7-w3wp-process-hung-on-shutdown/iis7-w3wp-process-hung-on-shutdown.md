---
permalink: iis7-w3wp-process-hung-on-shutdown
title: IIS 7 w3wp Process Hung on Shutdown
date: 2009-09-25
tags: [IIS, Windbg]
---
A single server has started to sometime leave zombie w3wp.exe processes when trying to recycle. A new process is spawned properly and everything seems to work, except the old processes are still present and take up memory. Task manager reports there's only a single thread left, far from the active ones that have between 40 and 70 threads usually.  Using [ProcDump](http://technet.microsoft.com/en-us/sysinternals/dd996900.aspx) I've taken a full memory dump to analyze further in [WinDbg](http://www.microsoft.com/whdc/DevTools/Debugging/default.mspx).  The machine is a Server 2008 R2 x64 8 core machine as stated by WinDbg:

<!-- more -->

```
Windows 7 Version 7600 MP (8 procs) Free x64
```

After loading sos a printout of the managed threads reveals the following:

```
0:000> !threads
ThreadCount: 19
UnstartedThread: 0
BackgroundThread: 19
PendingThread: 0
DeadThread: 0
Hosted Runtime: no
                                              PreEmptive                                                Lock
       ID OSID        ThreadOBJ     State   GC     GC Alloc Context                  Domain           Count APT Exception
XXXX    1  9d0 000000000209b4c0      8220 Enabled  0000000000000000:0000000000000000 000000000208e770     0 Ukn
XXXX    2  c60 00000000020c3130      b220 Enabled  000000013fbe5ed0:000000013fbe7da8 000000000208e770     0 MTA (Finalizer)
XXXX    3  a24 00000000020f0d60   880a220 Enabled  0000000000000000:0000000000000000 000000000208e770     0 MTA (Threadpool Completion Port)
XXXX    4  97c 0000000002105180    80a220 Enabled  0000000000000000:0000000000000000 000000000208e770     0 MTA (Threadpool Completion Port)
XXXX    5  c28 000000000210bfe0      1220 Enabled  0000000000000000:0000000000000000 000000000208e770     0 Ukn
XXXX    6  d40 00000000053f9080   180b220 Enabled  00000001bfe75d20:00000001bfe767c8 000000000208e770     0 MTA (Threadpool Worker)
XXXX    7  c18 00000000053f9b30   180b220 Enabled  00000000fff95880:00000000fff97210 000000000208e770     0 MTA (Threadpool Worker)
XXXX    8  f7c 00000000053fa5e0   180b220 Enabled  000000011fbea268:000000011fbea920 000000000208e770     0 MTA (Threadpool Worker)
XXXX    9  91c 00000000053fb090   180b220 Enabled  00000001dfc39138:00000001dfc39670 000000000208e770     0 MTA (Threadpool Worker)
XXXX    a  fb0 00000000053fbd20   180b220 Enabled  00000000fff922b0:00000000fff93210 000000000208e770     0 MTA (Threadpool Worker)
XXXX    b  fc8 00000000053fc9b0   180b220 Enabled  0000000160053ea0:0000000160054778 000000000208e770     0 MTA (Threadpool Worker)
XXXX    c  538 00000000053fd460   180b220 Enabled  000000017fd8fc98:000000017fd911f8 000000000208e770     0 MTA (Threadpool Worker)
XXXX    d  604 00000000053fdf10   180b220 Enabled  000000019fd7aa78:000000019fd7c648 000000000208e770     0 MTA (Threadpool Worker)
   0    f  2cc 0000000005514c60       220 Enabled  0000000000000000:0000000000000000 000000000208e770     0 Ukn
XXXX   10  9bc 00000000020a90c0       220 Enabled  0000000000000000:0000000000000000 000000000208e770     0 Ukn
XXXX   11  9c0 00000000056b7a00       220 Enabled  0000000000000000:0000000000000000 000000000208e770     0 Ukn
XXXX    e  9d4 00000000056b7fd0       220 Enabled  0000000000000000:0000000000000000 000000000208e770     0 Ukn
XXXX   12  9d8 00000000056b85a0       220 Enabled  0000000000000000:0000000000000000 000000000208e770     0 Ukn
XXXX   13  cb8 00000000056b8b70       220 Enabled  0000000000000000:0000000000000000 000000000208e770     0 Ukn
```

Of more interest however is probably the output of a stack backtrace for the single unmanaged thread remaining:

```
0:000> ~* kb 2000

.  0  Id: 85c.2cc Suspend: -1 Teb: 000007ff`fffd3000 Unfrozen
RetAddr           : Args to Child                                                           : Call Site
000007fe`fdcc1843 : 00000000`00fd6b60 00000000`00fd6b60 ffffffff`ffffffff 00000000`77bc04a0 : ntdll!ZwClose+0xa
00000000`77ab2c41 : 00000000`77bc1670 00000000`00000000 00000000`77bc04a0 7fffffff`ffffffff : KERNELBASE!CloseHandle+0x13
000007fe`f56537c6 : 00000000`00000000 00000000`00000000 00000000`012da080 000007fe`f5442eac : kernel32!CloseHandleImplementation+0x3d
000007fe`f54443d2 : 00000000`00000007 000007fe`f5443d3c 00000000`00000000 00000000`77bc9997 : httpapi!HttpCloseRequestQueue+0xa
000007fe`f54444c3 : 00000000`00000000 00000000`012e6900 00000000`00000000 00000000`77bd5afa : w3dt!UL_APP_POOL::Cleanup+0x62
000007fe`f549384a : 00000000`012da080 00000000`00c93a28 00000000`012e6900 00000000`00000000 : w3dt!WP_CONTEXT::CleanupOutstandingRequests+0x83
000007fe`f549417a : 00000000`00000000 00000000`0000ffff 00000000`00000000 00000000`77bcf9fd : iiscore!W3_SERVER::StopListen+0x4a
000007fe`f562b5bf : 00000000`012d2f30 00000000`00000000 00000000`00000000 00000000`0000ffff : iiscore!IISCORE_PROTOCOL_MANAGER::StopListenerChannel+0x5a
000007fe`f5626e8f : 00000000`012d2f30 00000000`00000000 00000000`00424380 00000000`00000000 : w3wphost!LISTENER_CHANNEL_STOP_WORKITEM::ExecuteWorkItem+0x7b
00000000`77bcf8eb : 00000000`021782b0 00000000`021782b0 00000000`00000000 00000000`00000001 : w3wphost!W3WP_HOST::ExecuteWorkItem+0xf
00000000`77bc9d9f : 00000000`00000000 00000000`012d2f30 00000000`00424380 00000000`010aa528 : ntdll!RtlpTpWorkCallback+0x16b
00000000`77aaf56d : 00000000`00000000 00000000`00000000 00000000`00000000 00000000`00000000 : ntdll!TppWorkerThread+0x5ff
00000000`77be3281 : 00000000`00000000 00000000`00000000 00000000`00000000 00000000`00000000 : kernel32!BaseThreadInitThunk+0xd
00000000`00000000 : 00000000`00000000 00000000`00000000 00000000`00000000 00000000`00000000 : ntdll!RtlUserThreadStart+0x1d
```

From the stack trace it's obvious that the w3wp process is trying to shut down and perform its cleanup tasks, but for some reason ntdll!ZwClose is hanging up. It's been hung for several days without change - and without apparent side effects besides an increased amount of memory usage.

The w3wp processes do not hang up all the time, I have yet to find a reproducible pattern. In the meantime, any suggestions for further debugging?
