---
permalink: accessing-my-privates
title: Accessing My Privates, Scope vs Encapsulation
date: 2007-10-09
tags: [.NET]
---
Recently I was developing a couple of simple ORM classes that had me confused.

<!-- more -->

```csharp
[Serializable]
public class Domain
{
	// Read
	private int domainID;
	public int DomainID { get { return domainID; } }
	
	private string domainName;
	public string DomainName { get { return domainName; } }

	// Read/Write
	public int? CompanyID;
}
```

Take this simple object as an example. It represents a website domain, it has an ID from the database aswell as a read only domain name and a belonging CompanyID.

Now, I want to create a Load() function that given a domain ID will instantiate a new instance of a Domain object and populate its values from the database. Now, as the DomainID and DomainName variables are private, I'll have to make a constructor method to pass in those values, right? It seems not, if my Load method is a static method of the Domain class itself:

```csharp
public static Domain Load(int domainID)
{
	SqlCommand cmd = new SqlCommand("SELECT Domain, CompanyID FROM tblDomains WHERE DomainID = @DomainID");
	cmd.Parameters.Add("@DomainID", SqlDbType.Int).Value = domainID;

	DataRow dr = DB.GetDR(cmd);

	if (dr == null)
		return null;
	else
	{
		Domain d = new Domain();
		
		d.domainID = domainID;
		d.domainName = DBConvert.To<string>(dr["Domain"]);
		d.CompanyID = DBConvert.To<int?>(dr["CompanyID"]);

		return d;
	}
}
```

The interesting part is in the else block. I create a new Domain instance, and I'm able to set the private field values directly. This saves me a lot of work as I don't have to create constructor methods - I'd rather not have to maintain those as well, and I think this was is a lot more readable. Anyways, I don't understand why this is possible. Granted, my static method is part of the Domain class and as such could have access to private variables, but these private variables are not static and thus they belong to the actual Domain instance (d). Since they belong to the Domain instance and I'm setting them through the instance variable d, how am I able to access them? Ought they noe be private, even though I'm writing my code inside the Domain function?

Everything compiles and runs perfectly, I just don't understand why this is possible.

Update:
    [Jakob Andersen](http://www.intellect.dk/) provided me with the answer. It's simply a matter of scope, whether the method is static or not does not matter. Also encapsulation is ignored as scope takes precedence.
