permalink: automatically-mapping-datatable-to-objects
title: Automatically Mapping Datatable to Objects
date: 2007-11-19
tags: [.NET]
---
I often need to transfer data from my business layer to my presentation layers in a strongly typed way. In this example I'll use the following very struct and corresponding DataTable to represent the data I need to transfer:

<!-- more -->

```csharp
struct Test
{
	public string Name;
	public int Value;
}

using (DataTable dt = new DataTable())
{
	dt.Columns.Add("Name", typeof(string));
	dt.Columns.Add("Value", typeof(int));
}
```

Our objective is basically to transfer the DataRows in the DataTable into a List that can be transferred on to the next layer.

The fastest way possible would be doing it manually like so:

```csharp
List list = new List();
foreach (DataRow dr in dt.Rows)
{
	Test t = new Test();
	t.Name = dr["Name"].ToString();
	t.Value = Convert.ToInt32(dr["Value"]);

	list.Add(t);
}
```

But this takes a lot of time to write, especially if your objects vary a lot and you have to create a lot of them. That's where my MapList function comes into play:

```csharp
private static List MapList(DataTable dt)
{
	List list = new List();

	FieldInfo[] fields = typeof(T).GetFields();
	T t = Activator.CreateInstance();

	foreach (DataRow dr in dt.Rows)
	{
		foreach (FieldInfo fi in fields)
			fi.SetValueDirect(__makeref(t), dr[fi.Name]);

		list.Add(t);
	}

	return list;
}
```

It takes a DataTable as the sole parameter (you could easily use a DataReader if you wanted to). It retrieves the fields of the generic type by reflection. It is important to note that this includes all fields of the type, so we're expecting there to be a 1:1 map of the DataTable and the types' fields. Another important remark is that the generic type *must* be a struct - for us to be able to move the type instantiation outside of the loop (for performance), it must be a struct (since adding it to the List will create a copy). If it were a class, we would overwrite the already existing objects each time we iterated a new row in the DataTable.

The SetValueDirect() method is somewhat faster than SetValue(). Caching the TypedReference for t by creating it outside the loop actually decreased performance, I'll probably have to look into the IL code to identify why. I've also tried caching the DataRow ordinals, there is no performance gain to be seen unless we're talking several millions of datarows, and in that case - this function is not the way to go.

I would really like to obtain a pointer to the struct fields and set the values directly using some unsafe pointer magic - anyone know how to obtain a pointer to the field hiding behind the FieldInfo type we get by reflection?

So what's the catch as opposed to doing it manually? Performance. Here's a graph that shows the performance hit in sets of 1 to 1.000.000 iterations. Note that at 1000 iterations the timing shifts from ticks to milliseconds and that the graph is using a base 10 logarithmic scale.

maplistperf_2.jpg

Obviously there's a performance hit - rather consistently, a factor of 10 -, but depending on the situation it is to be used in, mapping the objects by reflection may easily be a viable solution.

[ConsoleApplication1.rar - Sample solution](http://improve.dk/wp-content/uploads/2007/11/ConsoleApplication1.rar)
