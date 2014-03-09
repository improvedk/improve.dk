permalink: simple-file-synchronization-using-robocopy
title: Simple File Synchronization Using Robocopy
date: 2009-10-07
tags: [Windows]
---
On numerous occations I've had a need for synchronizing directories of files &amp; subdirectories. I've used it for synchronizing work files from my stationary PC to my laptop in the pre-always-on era (today I use SVN for almost all files that needs to be in synch). Recently I needed to implement a very KISS backup solution that simply synchronized two directories once a week for offsite storing of the backup data.

While seemingly simple the only hitch was that there would be several thousands of files in binary format, so compression was out of the question. All changes would be additions and deletions - that is, no incremental updates. Finally I'd only have access to the files through a share so it wouldn't be possible to deploy a local backup solution that monitored for changes and only sent diffs.

I started out using [ViceVersa PRO](http://www.tgrmn.com/web/file_synchronization.htm) with the [VVEngine](http://www.tgrmn.com/web/vvengine/vvengine.htm) addon for scheduling. It worked great for some time although it wasn't the most speedy solution given my scenario. Some time later ViceVersa stopped working. Apparently it was simply unable to handle more than a million files (give or take a bit). Their forums are filled with people asking for solutions, though the only suggestion they have is to split the backup job into several smaller jobs. Hence I started taking backups of MyShare/A*, MyShare/B*, etc. This quickly grew out of hand as the number of files increased.

Sometime later I was introduced to [Robocopy](http://technet.microsoft.com/en-us/library/cc733145(WS.10).aspx). Robocopy ships with Vista, Win7 and Server 2008. For server 2003 and XP it can be downloaded as part of the[Windows Server 2003 Resource Kit Tools](http://www.microsoft.com/Downloads/details.aspx?FamilyID=9d467a69-57ff-4ae7-96ee-b18c4790cffd&amp;displaylang=en).

Robocopy (Robust File Copy) does one job extremely well - it copies files. It's run from the command line, though there has been made a [wrapper GUI](http://technet.microsoft.com/en-us/magazine/2006.11.utilityspotlight.aspx) for it. The basic syntax for calling robocopy is:

```
robocopy <Source> <Destination> [<File>[ ...]] [<Options>]
```

You give it a source and a destination address and it'll make sure all files &amp; directories from the source are copied to the destination. If an error occurs it'll wait for 30 seconds (configurable) before retrying, and it'll continue doing this a million times (configurable). That means robocopy will survive a network error and just resume the copying process once the network is back up again.

What makes robocopy really shine is not it's ability to copy files, but it's ability to mirror one directory into another. That means it'll not just copy files, it'll also delete any extra files in the destination directory. In comparison to ViceVersa, robocopy goes through the directory structure in a linear fashion and in doing so doesn't have any major requirements of memory. ViceVersa would initially scan the source and destination and then perform the comparison. As source and destination became larger and larger, more memory was required to perform the initial comparison - until a certain point where it'd just give up.

I ended up with the following command for mirroring the directories using robocopy:

```
robocopy \\SourceServer\Share \\DestinationServer\Share /MIR /FFT /Z /XA:H /W:5
```

<ul>
  <li>/MIR specifies that robocopy should mirror the source directory and the destination directory. Beware that this may delete files at the destination.</li>
  <li>/FFT uses fat file timing instead of NTFS. This means the granularity is a bit less precise. For across-network share operations this seems to be much more reliable - just don't rely on the file timings to be completely precise to the second.</li>
  <li>/Z ensures robocopy can resume the transfer of a large file in mid-file instead of restarting.</li>
  <li>/XA:H makes robocopy ignore hidden files, usually these will be system files that we're not interested in.</li>
  <li>/W:5 reduces the wait time between failures to 5 seconds instead of the 30 second default.</li>
</ul>

The robocopy script can be setup as a simply Scheduled Task that runs daily, hourly, weekly etc. Note that robocopy also contains a switch that'll make robocopy monitor the source for changes and invoke the synchronization script each time a configurable number of changes has been made. This may work in your scenario, but be aware that robocopy will not just copy the changes, it will scan the complete directory structure just like a normal mirroring procedure. If there's a lot of files &amp; directories, this may hamper performance.
