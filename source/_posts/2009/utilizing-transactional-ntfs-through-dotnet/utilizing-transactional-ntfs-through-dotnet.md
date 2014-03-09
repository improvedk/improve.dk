permalink: utilizing-transactional-ntfs-through-dotnet
title: Utilizing Transactional NTFS Through .NET
date: 2009-02-15
tags: [.NET]
---
We're used to using transactions when dealing with the database layer. Transactions ensure we can perform multiple queries as one atomic event, either they all succed or they all fail, obeying the rules of [ACIDity](http://en.wikipedia.org/wiki/ACID). Until Vista, performing transactional file operations haven't been possible.

Transaction NTFS (or TxF) is available from Vista and onwards, which means Server 2008 is also capable. XP and Server 2003 do not support TxF and there are currently no plans of adding TxF support in systems previous to Vista.

So what is the benefit of using TxF? The benefit is that we can now perform ACID operations in the file level, meaning we can perform several file operations (whether that be moves, deletions, creations etc) and make sure all of them are committed atomically. It also provides isolation from/for other processes, so whenever a transaction has been started, we will always see a consistent view of a view until we have committed the transaction, even though it has been modified otherwhere. Surendra Verma has a great video on Channel 9 [explaining TxF](http://channel9.msdn.com/shows/Going+Deep/Surendra-Verma-Vista-Transactional-File-System/). Jon Cargille and Christian Allred has another video on Channel 9 that goes even more in-depth on the [inner workings on TxF and the Vista KTM](http://channel9.msdn.com/shows/Going+Deep/Transactional-Vista-Kernel-Transaction-Manager-and-friends-TxF-TxR/).

Why hasn't TxF gotten more momentum? Most likely because it's not part of the BCL! To utilize TxF we have to call Win32 API functions, which is a big step away from lazily utilizing database transactions by just wrapping our code inside of a [TransactionScope](http://msdn.microsoft.com/en-us/library/system.transactions.transactionscope.aspx).

Using TxF is actually quite simple once we've made a couple of necessary managed wrapper classes. Let me present you to KtmTransactionHandle:

```csharp
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Transactions;
using Microsoft.Win32.SafeHandles;

namespace TxFTest
{
	public class KtmTransactionHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		/// <summary>
		/// http://msdn.microsoft.com/en-us/library/aa344210(VS.85).aspx
		/// </summary>
		[ComImport]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		[Guid("79427A2B-F895-40e0-BE79-B57DC82ED231")]
		private interface IKernelTransaction
		{
			void GetHandle([Out] out IntPtr handle);
		}

		/// <summary>
		/// http://msdn.microsoft.com/en-us/library/ms724211.aspx
		/// </summary>
		[DllImport("kernel32")]
		private static extern bool CloseHandle(IntPtr handle);

		private KtmTransactionHandle(IntPtr handle): base(true)
		{
			this.handle = handle;
		}

		/// <summary>
		/// http://msdn.microsoft.com/en-us/library/cc303707.aspx
		/// </summary>
		public static KtmTransactionHandle CreateKtmTransactionHandle()
		{
			if (Transaction.Current == null)
				throw new InvalidOperationException("Cannot create a KTM handle without Transaction.Current");

			return KtmTransactionHandle.CreateKtmTransactionHandle(Transaction.Current);
		}

		/// <summary>
		/// http://msdn.microsoft.com/en-us/library/cc303707.aspx
		/// </summary>
		public static KtmTransactionHandle CreateKtmTransactionHandle(Transaction managedTransaction)
		{
			IKernelTransaction tx = (IKernelTransaction)TransactionInterop.GetDtcTransaction(Transaction.Current);
			IntPtr txHandle;
			tx.GetHandle(out txHandle);

			if (txHandle == IntPtr.Zero)
				throw new Win32Exception("Could not get KTM transaction handle.");

			return new KtmTransactionHandle(txHandle);
		}

		protected override bool ReleaseHandle()
		{
			return CloseHandle(handle);
		}
	}
}
```

The KtmTransactionHandle represents a specific transaction going on inside of the [KTM](http://en.wikipedia.org/wiki/Kernel_Transaction_Manager). In the code, there's references for further reading of the specific fucntions, most of them stemming from MSDN. Note that the CreateTransactionHandle method assumes there's already a current transaction, if there is not, an exception will be thrown.

The second class we need is called TransactedFile. I basically made this to be used as a direct replacement of System.IO.File. It does not include all of the functionality of System.IO.File, but it does have the two most important ones, Open and Delete - most of the other functions are just wrappers of these two, so they are easily appended later on.

```csharp
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace TxFTest
{
	public class TransactedFile
	{
		/// <summary>
		/// http://msdn.microsoft.com/en-us/library/aa363916(VS.85).aspx
		/// </summary>
		[DllImport("kernel32", SetLastError=true)]
		private static extern bool DeleteFileTransactedW(
			[MarshalAs(UnmanagedType.LPWStr)]string file,
			KtmTransactionHandle transaction);

		/// <summary>
		/// http://msdn.microsoft.com/en-us/library/aa363859(VS.85).aspx
		/// </summary>
		[DllImport("kernel32", SetLastError=true)]
		private static extern SafeFileHandle CreateFileTransactedW(
			[MarshalAs(UnmanagedType.LPWStr)]string lpFileName,
			NativeFileAccess dwDesiredAccess,
			NativeFileShare dwShareMode,
			IntPtr lpSecurityAttributes,
			NativeFileMode dwCreationDisposition,
			int dwFlagsAndAttributes,
			IntPtr hTemplateFile,
			KtmTransactionHandle hTransaction,
			IntPtr pusMiniVersion,
			IntPtr pExtendedParameter);

		[Flags]
		private enum NativeFileShare
		{
			FILE_SHARE_NONE = 0x00,
			FILE_SHARE_READ = 0x01,
			FILE_SHARE_WRITE = 0x02,
			FILE_SHARE_DELETE = 0x04
		}

		[Flags]
		private enum NativeFileMode
		{
			CREATE_NEW = 1,
			CREATE_ALWAYS = 2,
			CREATE_EXISTING = 3,
			OPEN_ALWAYS = 4,
			TRUNCATE_EXISTING = 5
		}

		[Flags]
		private enum NativeFileAccess
		{
			GENERIC_READ = unchecked((int)0x80000000),
			GENERIC_WRITE = 0x40000000
		}

		/// <summary>
		/// Transaction aware implementation of System.IO.File.Open
		/// </summary>
		/// <param name="path"></param>
		/// <param name="mode"></param>
		/// <param name="access"></param>
		/// <param name="share"></param>
		/// <returns></returns>
		public static FileStream Open(string path, FileMode mode, FileAccess access, FileShare share)
		{
			using (KtmTransactionHandle ktmHandle = KtmTransactionHandle.CreateKtmTransactionHandle())
			{
				SafeFileHandle fileHandle = CreateFileTransactedW(
					path,
					TranslateFileAccess(access),
					TranslateFileShare(share),
					IntPtr.Zero,
					TranslateFileMode(mode),
					0,
					IntPtr.Zero,
					ktmHandle,
					IntPtr.Zero,
					IntPtr.Zero);

				if (fileHandle.IsInvalid)
					throw new Win32Exception(Marshal.GetLastWin32Error());

				return new FileStream(fileHandle, access);
			}
		}

		/// <summary>
		/// Reads all text from a file as part of a transaction
		/// </summary>
		/// <param name="path"></param>
		/// <param name="contents"></param>
		/// <returns></returns>
		public static string ReadAllText(string path)
		{
			using (StreamReader reader = new StreamReader(Open(path, FileMode.Open, FileAccess.Read, FileShare.Read)))
			{
				return reader.ReadToEnd();
			}
		}

		/// <summary>
		/// Writes text to a file as part of a transaction
		/// </summary>
		/// <param name="path"></param>
		/// <param name="contents"></param>
		public static void WriteAllText(string path, string contents)
		{
			using (StreamWriter writer = new StreamWriter(Open(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None)))
			{
				writer.Write(contents);
			}
		}

		/// <summary>
		/// Deletes a file as part of a transaction
		/// </summary>
		/// <param name="file"></param>
		public static void Delete(string file)
		{
			using (KtmTransactionHandle ktmHandle = KtmTransactionHandle.CreateKtmTransactionHandle())
			{
				if (!DeleteFileTransactedW(file, ktmHandle))
					throw new Exception("Unable to perform transacted file delete.");
			}
		}

		/// <summary>
		/// Managed -> Native mapping
		/// </summary>
		/// <param name="mode"></param>
		/// <returns></returns>
		private static NativeFileMode TranslateFileMode(FileMode mode)
		{
			if (mode != FileMode.Append)
				return (NativeFileMode)(int)mode;
			else
				return (NativeFileMode)(int)FileMode.OpenOrCreate;
		}

		/// <summary>
		/// Managed -> Native mapping
		/// </summary>
		/// <param name="access"></param>
		/// <returns></returns>
		private static NativeFileAccess TranslateFileAccess(FileAccess access)
		{
			if (access == FileAccess.Read)
				return NativeFileAccess.GENERIC_READ;
			else
				return NativeFileAccess.GENERIC_WRITE;
		}

		/// <summary>
		/// Direct Managed -> Native mapping
		/// </summary>
		/// <param name="share"></param>
		/// <returns></returns>
		private static NativeFileShare TranslateFileShare(FileShare share)
		{
			return (NativeFileShare)(int)share;
		}
	}
}
```

The primary two API functions used are [DeleteFileTransactedW](http://msdn.microsoft.com/en-us/library/aa363916(VS.85).aspx) and [CreateFileTransactedW](http://msdn.microsoft.com/en-us/library/aa363859(VS.85).aspx). Note that these functions are the 'W' versions, accepting unicode paths for the files. To send the strings as null terminated unicode, we have to add the MarshalAs(UnmanagedType.LPWstr) attribute to the 'path' parameter.

The BCL FileMode, FileShare and FileAccess all have native counterparts. The constant values are in the Microsoft.Win32.NativeMethods class, but unfortunately it's internal so we'll have to make our own. There are three helper functions for translating between the managed and native versions of FileMode, FileShare and FileAccess.

The Open and Delete methods both try to obtain a KTM transaction handle as their first action. If a current transaction does not exist, they will throw an exception since KtmTransactionHandle assumes one exists. We could modify these to either perform a transacted operation or non transacted, depending on the availability of a current transaction, but in this case we're explicitly assuming a transaction will be available.

Next up the Delete operation will attempt to delete the file using the DeleteFileTransactedW function, passing in the KTM transaction handle. The Open function first tries to obtain a [SafeFileHandle](http://msdn.microsoft.com/en-us/library/microsoft.win32.safehandles.safefilehandle.aspx) for the file, which is basically a wrapper class around a normal file handle. Using the SafeFileHandle, we can create a new FileStream, passing in the file handle as a parameter.

Using these two classes, we can now perform transactional file operations:

```csharp
using System;
using System.Data.SqlClient;
using System.Transactions;

namespace TxFTest
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				using (var ts = new TransactionScope())
				{
					TransactedFile.WriteAllText("test.txt", "hello world");
				}

				// At this point test.txt does not exist since we didn't ts.complete()

				using (var ts = new TransactionScope())
				{
					TransactedFile.WriteAllText("test.txt", "hello world");

					// The transaction hasn't been committed, so the file is still not logically available outisde
					// of the transaction, but it is available inside of the transaction
					Console.WriteLine(TransactedFile.ReadAllText("test.txt"));

					// After the transaction is committed, the file is available outside of the transaction
					ts.Complete();
				}

				// Since the TransactionScope works for both database and files, we can combine the two. This is great for ensuring
				// integrity when we store database related files
				using (var ts = new TransactionScope())
				{
					SqlCommand cmd = new SqlCommand("INSERT INTO SomeTable (A, B) VALUES ('a', 'b'); SELECT @@IDENTITY");
					int insertedID = Convert.ToInt32(cmd.ExecuteScalar());

					TransactedFile.WriteAllText(insertedID + ".txt", "Blob file related to inserted database row");

					ts.Complete();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}

			Console.Write("Done");
			Console.Read();
		}
	}
}
```

Note that the KTM transaction is able to participate in a distributed transaction using the MS DTC service. That means we can both perform database and file operations inside of a transaction scope and have all of them performed ACIDically.

Using transactions comes at a cost - performance. Since the system has to guarantee the ACID properties are respected, there will be administrative overhead as well as the possibility of extra disk activity. Whenever we modify an existing file, the original file is left untouched until the new file has been written to disk, otherwise we might risk destryoying the original file if the computer were to crash halfways through a write procedure.

There are of course [limitations in TxF](http://msdn.microsoft.com/en-us/library/aa365738(VS.85).aspx), as there are in all good things. Most notably it'll only work for local volumes, you can't use TxF on file shares as it's not supported by the CIFS/SMB protocols.
