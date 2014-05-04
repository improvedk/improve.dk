---
permalink: copying-a-sql-server-database-file-thatrsquos-in-use-using
title: Copying a SQL Server Database File That's in Use Using Volume Shadow Copy
date: 2011-06-21
tags: [.NET]
---
When working on [OrcaMDF](https://github.com/improvedk/OrcaMDF) I usually setup a test database, force a checkpoint and then perform my tests on the MDF file. Problem is, you can't open the MDF file for reading, nor copy it, as long as the database is online in SQL Server. I could shut down SQL Server temporarily while copying the file, but that quickly becomes quite a hassle.

<!-- more -->

## Leveraging Volume Shadow Copy (VSS) through AlphaVSS

[AlphaVSS](http://www.alphaleonis.com/2008/08/alphavss-bringing-windows-shadow-copy-service-vss-to-net/) is an excellent library for invoking VSS through .NET. While it can do much more, I'm using it to create a snapshot of a single active file, copy it and then dispose of the snapshot afterwards.

The following class presents a single static method that'll copy any file (locked or not) and copy it to the desired destination. It would be easy to adapt upon this sample to copy multiple files, directories, etc. Note that while a copy file progress clalback is supported, I don't really care about the progress and am there sending a null reference when calling [CopyFileEx](http://msdn.microsoft.com/en-us/library/aa363852(v=vs.85).aspx).

```cs
class VssHelper
{
	[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
	private static extern bool CopyFileEx(string lpExistingFileName, string lpNewFileName, CopyProgressRoutine lpProgressRoutine, int lpData, ref int pbCancel, uint dwCopyFlags);
	private delegate uint CopyProgressRoutine(long TotalFileSize, long TotalBytesTransferred, long StreamSize, long StreamBytesTransferred, uint dwStreamNumber, uint dwCallbackReason, IntPtr hSourceFile, IntPtr hDestinationFile, IntPtr lpData);

	public static void CopyFile(string source, string destination)
	{
		var oVSSImpl = VssUtils.LoadImplementation();

		using (var vss = oVSSImpl.CreateVssBackupComponents())
		{
			vss.InitializeForBackup(null);

			vss.SetBackupState(false, true, VssBackupType.Full, false);

			using (var async = vss.GatherWriterMetadata())
				async.Wait();

			vss.StartSnapshotSet();
			string volume = new FileInfo(source).Directory.Root.Name;
			var snapshot = vss.AddToSnapshotSet(volume, Guid.Empty);

			using (var async = vss.PrepareForBackup())
				async.Wait();

			using (var async = vss.DoSnapshotSet())
				async.Wait();

			var props = vss.GetSnapshotProperties(snapshot);
			string vssFile = source.Replace(volume, props.SnapshotDeviceObject + @"");

			int cancel = 0;
			CopyFileEx(vssFile, destination, null, 0, ref cancel, 0);
		}
	}
}
```
