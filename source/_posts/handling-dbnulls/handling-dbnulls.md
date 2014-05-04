---
permalink: handling-dbnulls
title: Handling DBNulls
date: 2007-10-08
tags: [.NET]
---
Reading and writing values to the DB has always been a bit cumbersome when you had to take care of nullable types and DBNull values. Here's a way to make it easy.  Based on [this post by Peter Johnson](http://aspalliance.com/852) and [this post by Adam Anderson](http://www.falafel.com/community/blogs/techbits/archive/2006/10/13/Casting-DBNull-Gracefully-and-Elegantly-Using-Generics.aspx) I gathered a couple of ideas and combined them to make a completely generic class that will handle DBNulls for both reads and writes, as well as handling nullable types.  Let me present the code, I'll go over it afterwards:

<!-- more -->

```cs
public static class DBConvert
{
	/// <summary>
	/// Handles reading DBNull values from database in a generic fashion
	/// </summary>
	/// <typeparam name="T">The type of the value to read</typeparam>
	/// <param name="value">The input value to convert</param>
	/// <returns>A strongly typed result, null if the input value is DBNull</returns>
	public static T To<T>(object value)
	{
		if (value is DBNull)
			return default(T);
		else
			return (T)changeType(value, typeof(T));
	}

	/// <summary>
	/// Handles reading DBNull values from database in a generic fashion, simplifies frontend databinding
	/// </summary>
	/// <typeparam name="T">The type of the value to read</typeparam>
	/// <param name="ri">The Container item in a databinding operation</param>
	/// <param name="column">The dataitem to read</param>
	/// <returns>A strongly typed result, null if the input value is DBNull</returns>
	public static T To<T>(RepeaterItem ri, string column)
	{
		if (DataBinder.Eval(ri.DataItem, column) is DBNull)
			return default(T);
		else
			return (T)changeType(DataBinder.Eval(ri.DataItem, column), typeof(T));
	}

	/// <summary>
	/// Internal method that wraps Convert.ChangeType() so it handles Nullable<> types
	/// </summary>
	/// <param name="value">The value to convert</param>
	/// <param name="conversionType">The type to convert into</param>
	/// <returns>The input value converted to type conversionType</returns>
	private static object changeType(object value, Type conversionType)
	{
		if (conversionType.IsGenericType && conversionType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
		{
			if (value == null)
				return null;

			conversionType = Nullable.GetUnderlyingType(conversionType);
		}

		return Convert.ChangeType(value, conversionType);
	}

	/// <summary>
	/// Simplifies setting SqlParameter values by handling null issues
	/// </summary>
	/// <param name="value">The value to return</param>
	/// <returns>DBNull if value == null, otherwise we pass through value</returns>
	public static object From(object value)
	{
		if (value == null)
			return DBNull.Value;
		else
			return value;
	}
}
```

The first To method significantly simplifies the process of setting database values when using SqlParameters (we all do, right?).

This is how I used to handle possible DBNulls when reading into a nullable integer:

```cs
if(dr["CountryID"] is DBNull)
	c.CountryID = null;
else
	c.CountryID = Convert.ToInt32(dr["CountryID"]);
```

And this is how it's done using my DBConvert class:

```cs
c.CountryID = DBConvert.To<int?>(dr["CountryID"]);
c.Recommended = DBConvert.To<bool>(dr["Recommended"]);
d.companyMessageCreated = DBConvert.To<DateTime?>(dr["CompanyMessageCreated"]);
d.Message = DBConvert.To<string>(dr["Message"]);
d.OverallScore = DBConvert.To<int>(dr["OverallScore"]);
```

Notice how it works for both nullable ints, DateTimes and whatever other nullable types you wish. It also works for normal types like string, int and so forth. It'll automatically typecast it into the type specified as a generic parameter. However, remember that the database value must match the value being converted to, you cannot use .To("some string value"), it will fail.

The private changeType() method is a wrapper for the ChangeType() method that takes care of nullable types since the builtin Convert.ChangeType() method does not support casting into nullable types.

The second To simplifies databinding values in the frontend ASPX files. This is how I used to print a DateTime column in a ShortDateString format:

```cs
<%# Convert.ToDateTime(DataBinder.Eval(Container.DataItem, "Created")).ToShortDateString() %>
```

And this is how it can be done using the DBConvert class, generically:

```cs
<%# DBConvert.To<DateTime>(Container, "Created").ToShortDateString() %>
```

Nullable types, as well as null strings also have to be handled when assigning SqlParameter values. The usual way for both nullable types as well as strings might look like this:

```cs
if(CountryID == null)
	cmd.Parameters.Add("@CountryID", SqlDbType.Int).Value = DBNull.Value;
else
	cmd.Parameters.Add("@CountryID", SqlDbType.Int).Value = CountryID;
```

Using the DBConvert class this can be done a bit simpler:

```cs
cmd.Parameters.Add("@CountryID", SqlDbType.Int).Value = DBConvert.From(CountryID);
cmd.Parameters.Add("@CountryID", SqlDbType.NVarChar, 2048).Value = DBConvert.From(MyString);
```

This will automatically convert null strings as well as nulled nullable types to DBNull.

Enjoy :)
