permalink: sql-server-mirroring-a-practical-approach
title: SQL Server Mirroring - A Practical Approach
date: 2008-03-23
tags: [SQL Server]
---
In this post I'll take a practical approach at talking about what SQL Server Mirroring is, the advantages and considerations that follows.

<!-- more -->

## Availability, not scalability

SQL Server Mirroring aims to increase database availability, not scalability. Boiled down, a mirrored database consists of a principal database on SQL Server instance X and an exact mirror of that database on instance Y. Everytime a transaction has occured on X, it is executed on Y as well. While this is happening, the Y instance database is in recovery mode, meaning you cannot query it directly, and thus you cannot use this as a secondary readonly database to achieve scalability.

While you can run mirroring on different SQL Server instances on the same machine, this defeats the purpose as most errors will be hardware/system based, and usually these will affect all instances on the same phyiscal server. Trying out mirroring across instances is a good way to test it out however. In my demos I will be using virtual PCs, each loaded with SQL Server 2005 Enterprise sp1.

## Operating modes

SQL Server supports [three different operating modes](http://msdn2.microsoft.com/en-us/library/ms191456.aspx).

### High performance (asynchronous)

As the name implies, maintaining high performance is the key issue in this mode. Whenever a transaction is completed on the principal, the log is sent to the mirror, but the principal does not wait for this to complete. Thus if the mirror were to die out, throw an error during execution, the databases would become out of synch. The mirror could also trail behind due to a difference in computing power or other external factors. If the principal fails, you will have to do a manual failover to the mirror.

### High safety (synchronous) - also known as "high protection"

As with the high performance mode, each time a transaction occurs on the principal, it is sent to the mirror. The principal will not commit the transaction until the mirror has committed the transaction also. Thus you will never risk your databases being out of synch. The downside is that your mirrored setup will be no faster than the slowest machine that is part of the mirror, plus the implicit overhead in server chatting. As with the high performance mode, you will have to make a manual failover in case of principal failure.

### High safety with automatic failover (synchronous)

This mode involves a third instance besides the principal and mirror, known as the witness. The witness instance constantly monitors the principal for availability, and if a problem is detected, it will automatically perform a failover. At this point, the database as a whole will still be available, but you should manually get the failed mirror up and running again and reinitiate mirroring.

## Prerequisites

There are a [couple of things that should be in place](http://msdn2.microsoft.com/en-us/library/ms366349.aspx) before attempting to setup database mirroring.

### Service pack 1

Make sure all instances have been upgraded to [service pack 1](http://www.microsoft.com/downloads/details.aspx?FamilyID=cb6c71ea-d649-47ff-9176-e7cac58fd4bc&displaylang=en). If you do not have service pack 1, you will receive the following warning when trying to start the mirror:

enablemirroring_2.jpg

### Unified SQL Server service account

If you're using Active Directory, make sure the SQL Server service accounts are running as the same user. Local System will not work as it does not have any network credentials. If you're not using Active Directory, just make sure the services are running on an account with the same name & password on each machine. Make sure you change the service account through the SQL Server Configuration application and not the services console. Alternatively you can specify user accounts that should be used for mirror replication, but having the servers run on the same domain account is the easiest way.

### Full recovery model

Make sure the database you want to mirror is setup to use the full recovery backup model, otherwise there'll be no log to ship to the mirror instance and mirroring will not be possible.

## Licensing

Mirroring is supported in the SQL Server Standard and Enterprise editions. Neither Express nor Workgroup edition will work. Enterprise supports the high performance operating mode, Standard only supports the two high safety modes. You can use the free Express version for the mirror server. Note that you [do NOT need an extra SQL Server license for the mirroring server](http://www.microsoft.com/sql/howtobuy/sqlserverlicensing.mspx), provided that it does nothing else but maintain the mirrored database - take note of the 30 days clause.

## Test setup

My demos will be using three servers, all running Windows Server 2003 Standard (fully updated) through Virtual PC. All three have SQL Server 2005 Enterprise installed. I will be using the Microsoft sample database Adventureworks. You can [download the AdventureWorks database](http://codeplex.com/SqlServerSamples#databases) at CodePlex.

The three servers are RIO, MANDALAY and MGM (yes, I like Vegas). MGM will only be used for setting up a witness, RIO and MANDALAY will both host the actual databases, MANDALAY being the initial principal and RIO being the initial mirror. All servers are completely fresh installations using SQL Server authentication.

I will be connecting to the instances from SQL Server Management Studio running on my desktop computer.

## Initial setup of the databases

The first step is to restore the AdventureWorks .bak backup file to both servers. On the principal (MANDALAY) we should make a normal restore (RESTORE WITH RECOVERY) so the database is online. On the mirror (RIO), we should restore into the recovering state so no changes can be made (RESTORE WITH NORECOVERY). You can watch how it's done here, or skip on to the next section.


{% youtube eHeVZ1NzIO0 %}


## Mirroring configuration

Now that we've got both databases setup, we're ready to setup the actual mirror. A couple of notes on what I'm doing during the setup. In the first demo, I'll setup a synchronous high safety mirror with a witness. As all the virtual PCs are running on the same machine, I'll have to use different ports for the endpoints. Whether you want to use encryption for the endpoint communication is scenario specific. Encryption will [have an overhead](http://www.microsoft.com/technet/prodtechnol/sql/2005/technologies/dbm_best_pract.mspx) - albeit a minor one - so it's up to you to determine if it's neccessary. As our SQL Services are running on the same account across the machines, we do not have to specify any custom service account names during the setup.

For some reason, SQL Server needs a fully qualified domain name for the instance addresses. If you're setting this up on a computer that is part of the domain, you should simply use the domain name, [Computer].[domain]:port. In this example my desktop is not part of the Active Directory domain and thus it'll use addresses like TCP://COMPUTER:PORT which is not accepted. I'll fix it by simply writing the machine IP addresses manually instead. The IP for MANDALAY is 192.168.0.31 and for RIO it's 192.168.0.33. Note that you should ALWAYS use FQDNs, using IPs are not recommended as it may result in configuration as well as runtime issues. See [Adam Machanics blogpost](http://sqlblog.com/blogs/adam_machanic/archive/2007/06/13/database-mirroring-fqdns-are-your-friends.aspx) on the same issue as I ran into a couple of times.


{% youtube rlAMl3D47bo %}


## Testing the mirror

Besides my DBA excesses, I'm a developer. And what's more natural than to whip together a small application that tests the mirrors availability?

mirrortester_2.jpg

It continuously attempts to connect to the databases using three different connection strings:

```csharp
string principalConnection = "Data Source=Mandalay;Connect Timeout=1;Initial Catalog=AdventureWorks;User Id=sa;Password=sadpassword;Pooling=false";
string mirrorConnection = "Data Source=Rio;Connect Timeout=1;Initial Catalog=AdventureWorks;User Id=sa;Password=sadpassword;Pooling=false";
string totalConnection = "Data Source=Mandalay;Failover Partner=Rio;Connect Timeout=1;Initial Catalog=AdventureWorks;User Id=sa;Password=sadpassword;Pooling=false";
```

The first connects directly to MANDALAY, the principal database. The second one goes to RIO, the mirror. And the last one is the mirror enabled connection string that combines the two. The principal should respond and act like any other normal database. The mirror will throw an exception as we cannot interact with a datbase in recovery mode. The combined connection will automatically connect to the current principal database, whether it be MANDALAY or RIO.

To detect a broken connection quickly, I connect to the databases every 250ms and display a green bar if the connection succeeded (and an UPDATE & SELECT went well), and red if any kind of exception arose. To detect a connection timeout in a timely fashion, I'm using my [QuickOpen](http://improve.dk/blog/2008/03/10/controlling-sqlconnection-timeouts) functionality. The SUM(SafetyStockLevel) is the result of a simple SELECT query being done on the database (the UPDATE modifies the same table, hence the changing values), just to make sure we're actually talking to a live database.

In the following test, it gets a bit more complicated to follow. I've got two SQL Server Profiler windows open, the left one is monitoring the MANDALAY server, the right one is monitoring the RIO server. The windows are so small you cannot see what actually gets logged, but that is the point. The only relevant data in this case is the bottom rightmost value, Rows:X that displays an increasing rowcount when the server is active.

I will start out by starting up the mirror testing application. We should see a red bar for the mirror database (RIO) as we cannot connect to it directly, while the principal (MANDALAY) and the mirrored connection should both show green bars. The MANDALAY profiler should also show activity, whilst the RIO profiler should not show any activity.

When it's running, I'll initiate a manual mirror failover. A failover means we're switching roles, thus RIO will become the new principal and MANDALAY will become the mirror. Effectively this should mean the combined connection still shows a green bar, MANDALAY shows red and RIO switches to green.

{% youtube W7iw58KN4Sg %}

## The TCP/IP connection retry algorithm

The failover went perfect. There's a short amount of downtime as the actual failover takes place, but shortly thereafter, we get green bars again - but one too many. When we started out, we got a red bar when trying to connect to the mirror, RIO. Shouldn't we be getting a red bar when trying to connect to MANDALAY after we've switched the roles so MANDALAY has now become the new mirror? As you can see in the profilers, only RIO is being queried, so although MANDALAY is not responding, the connection string pointing to MANDALAY is succeeding. And what's more confusing is that the new instance of the testing application showed the expected result, a green bar for RIO and red for MANDALAY - at the same time as the existing one showed all greens.

The explanation is due to the [connection retry algorithm for TCP/IP connections](http://msdn2.microsoft.com/en-us/library/ms365783.aspx). When we have a mirrored connection, the partner names are cached when used. Although we couldn't query RIO before, the partner name was cached. Thus when we make the failover and MANDALAY looses connection, it'll automatically make a retry attempt by connecting to the mirror partner, RIO. When the database comes up again, RIO is responding to both connections successfully. So although the connection string specifies MANDALAY as the endpoint, we're actually talking to RIO directly.

Now, when the cache times out, or if we start a new application (the cache is tied to the SqlClient within a specific AppDomain), the partner name has not been cached and a retry will not be attempted, and that's why the new instance shows the expected result alongside the old instance.

## When a database dies

This is the scenario we've been preparing for. But what happens when one of the databases die? In high safety mode, a transaction has to be committed on both the principal and on the mirror before it's declared successful, but in case the mirror dies (whether due to the service stopping, the physical hardware crashing or something else) the principal will enter a disconnected state, still offering full availability. When you get the mirror database up and running again, it will automatically synchronize with the principal and the mirror will continue as if nothing had happened. High performance mode will also continue unaffected with a dead mirror, and it will also automatically resynch when the mirror comes back online.

Here's a quick video demonstrating the availability of the principal when the mirror goes down (the short red bars are caused by Virtual PC pausing all VPCs when the menu bar is open).


{% youtube UQFrD3RmNrg %}


If the principal dies, we need to promote the mirror to the principal role. As soon as the mirror has undertaken the principal role, we have access to our database again. This can be done safely in the synchronous high safety operating mode as we do not risk any dataloss due to all transactions being made simultaneously in both databases. In the high performance mode thugh, we cannot do this as there could potentially be transactions that has not yet been transferred to the mirror, which would result in data loss. In this case we have to get the principal back online - or accept possible data loss, depending on what's acceptable.

In the following video I'll simulate a dead principal. I'll show the availability tester application running, using MANDALAY as the principal, RIO being the mirror. I'll the pause the MANDALAY server, effectively simulating it dropping completely off the network. You'll then notice all red bars in the tester application, as expected. To get the database up again, we have to do a manual failover to the mirror server, making it the new principal. We do that by executing the query:

```sql
ALTER DATABASE AdventureWorks SET PARTNER FORCE_SERVICE_ALLOW_DATA_LOSS
```

There is no GUI that'll execute this query. Soon after I've executed the query, we get green bars again. Notice that all bars are green, this is due to the connection retry algorithm as explained earlier - the MANDALAY server is still offline. As I refresh the database list on RIO, you'll notice that the database is now listed as "Principal, Disconnected", confirming that the RIO database has undertaken the principal role, while disconnected from the mirror. I'll then resume MANDALAY, and as I refresh the MANDALAY database list, you'll notice that the database automatically changed state into "Mirror, Suspended / Restoring" - it picked up on the role change and is now awaiting further commands. It will not automatically resynch as mirroring is suspended when we force the principiality change through the ALLOW_SERVICE_DATA_LOSS parameter. We first have to resume the mirroring functionality. After having resumed mirroring, I'll wait for the databases to synch up completely, after which I'll do a manual failover so MANDALAY becomes the principal again. There's a short downtime as the failover takes place, but after that, we've got green bars and MANDALAY returns as the principal, RIO changing to mirror.


{% youtube R73OPBzS0ag %}


And there we go, we just experienced a principal going down with minimal service unavailability.

## High security with a witness

The restoration process we just experienced can be automated if we choose to utilize a third server known as the witness. The witness server continually monitors the principal and will initiate a failover in case the principal dies, as well as restoring the mirrors functionality when it returns (that is, converting the previous principal to the new mirror). It requires a third server, MGM (IP 192.168.0.35) for the witness part. Setup is more or less as usual, we just need to include the FQDN for the witness server.

In the last video I'll show how to setup the mirroring including a witness server. I will then start the availability testing application and pause the principal server afterwards. This will immediatly result in red boxes all over, but after a short while, the RIO server (the new principal) becomes green, and a moment after, the mirrored connection itself also becomes green. The MANDALAY box is also green, but again, this is due to the retry mechanism as explained earlier. You'll then see that the previous mirror database has now changed status to "Principal, Disconnected", proving that it has overtaken the principal responsibility and that it has lost connection to the mirror. I'll then show how the MANDALAY database has changed status to mirror, and how we can do a manual failover so everything goes back to normal. This is the point when FQDNs became neccessary. Using IPs resulted in the mirror not being able to make the failover. As stated earlier, using IPs is a bad practice, you should always use FQDNs. I've added the AD hostnames to my hosts file so I can enter them from my non-AD member desktop machine.


{% youtube oa6ZYxIPNFg %}


## Conclusion

SQL Mirroring is a great way to increase your availability rate in case of database failures. You need to understand precisely where mirroring will help and where it won't make a difference. It won't help when you fire out a TRUNCATE [WRONG_TABLE] statement since it'll just be replicated on the mirror, for that you'll still have to make a rollback via the logs. It'll help you in the case of a server crashing due to either hardware, software or network failures (depending on your network setup) and so forth. It'll also enable you to [do rolling upgrades](http://msdn2.microsoft.com/en-us/library/bb497962.aspx).

While configuration is rather straight forward, mirroring will add complexity to your setup and errors may be harder to track down. You also have to consider the licensing requirements depending on the level of mirroring you're planning to use.

## Downloads

[SQL_Mirroring_Tester.zip - Test solution](http://improve.dk/wp-content/uploads/2008/03/SQL_Mirroring_Tester.zip)
