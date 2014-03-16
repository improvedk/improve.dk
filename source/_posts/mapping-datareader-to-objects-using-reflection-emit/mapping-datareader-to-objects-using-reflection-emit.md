permalink: mapping-datareader-to-objects-using-reflection-emit
title: Mapping Datareader to Objects Using Reflection.Emit
date: 2008-05-02
tags: [.NET]
---
I've previously written of how to automatically [map a DataTable into a strongly typed collection of objects](/automatically-mapping-datatable-to-objects/). There's a problem though, [it's not fast](/performance-comparison-reading-data-from-the-database-strongly-typed/)... I wanted to improve on it, and this is what I ended up with.

<!-- more -->

The original method relied heavily on reflection to set the values directly. Reflection's bad in regards of speed, mkay? But reflection is not necessarily evil, you can do great good with it. Now, the problem with the original method is that each field is set using reflection for each row, that's [number of fields] * [number of rows] fields being set using reflection, resulting in pretty poor performance. If we compare it to the [manual way](/performance-comparison-reading-data-from-the-database-strongly-typed/), there's obviously some kind of a gap. If we could just somehow, in a generic way, create mapper methods like we do manually...

We'll have to create a DynamicMethod that takes in a DbDataReader, converts the current row to a generic type T. We'll call it MapDR, although it doesn't really matter what it's called.

```csharp
// Our method will take in a single parameter, a DbDataReader
Type[] methodArgs = { typeof(DbDataReader) };

// The MapDR method will map a DbDataReader row to an instance of type T
DynamicMethod dm = new DynamicMethod("MapDR", typeof(T), methodArgs, Assembly.GetExecutingAssembly().GetType().Module);
ILGenerator il = dm.GetILGenerator();
```

In this method, we'll create an instance of the generic type T and store it as a variable.

```csharp
// We'll have a single local variable, the instance of T we're mapping
il.DeclareLocal(typeof(T));

// Create a new instance of T and save it as variable 0
il.Emit(OpCodes.Newobj, typeof(T).GetConstructor(Type.EmptyTypes));
il.Emit(OpCodes.Stloc_0);
```

Then we'll look each property of the type.

```csharp
foreach (PropertyInfo pi in typeof(T).GetProperties())
```

Now we'll read the column value from the DbDataReader using the properties name. By reading it, we're pushing it onto the stack.

```csharp
// Load the T instance, SqlDataReader parameter and the field name onto the stack
il.Emit(OpCodes.Ldloc_0);
il.Emit(OpCodes.Ldarg_0);
il.Emit(OpCodes.Ldstr, pi.Name);

// Push the column value onto the stack
il.Emit(OpCodes.Callvirt, typeof(DbDataReader).GetMethod("get_Item", new Type[] { typeof(string) }));
```

Now's the ugly part. Depending on the type, there are different ways to convert it into the corresponding .NET type, i've made a switch statement handling most common types, although it does lack support for nullable types, guids and various other numeric formats. It should show to idea though. Converting the value will push the resulting correctly typed value onto the stack, and pop the original value in the process.

```csharp
// Depending on the type of the property, convert the datareader column value to the type
switch (pi.PropertyType.Name)
{
	case "Int16":
		il.Emit(OpCodes.Call, typeof(Convert).GetMethod("ToInt16", new Type[] { typeof(object) }));
		break;
	case "Int32":
		il.Emit(OpCodes.Call, typeof(Convert).GetMethod("ToInt32", new Type[] { typeof(object) }));
		break;
	case "Int64":
		il.Emit(OpCodes.Call, typeof(Convert).GetMethod("ToInt64", new Type[] { typeof(object) }));
		break;
	case "Boolean":
		il.Emit(OpCodes.Call, typeof(Convert).GetMethod("ToBoolean", new Type[] { typeof(object) }));
		break;
	case "String":
		il.Emit(OpCodes.Callvirt, typeof(string).GetMethod("ToString", new Type[] { }));
		break;
	case "DateTime":
		il.Emit(OpCodes.Call, typeof(Convert).GetMethod("ToDateTime", new Type[] { typeof(object) }));
		break;
	case "Decimal":
		il.Emit(OpCodes.Call, typeof(Convert).GetMethod("ToDecimal", new Type[] { typeof(object) }));
		break;
	default:
		// Don't set the field value as it's an unsupported type
		continue;
}
```

And finally we set the properties value, thereby popping the value from the stack.

```csharp
// Set the T instances property value
il.Emit(OpCodes.Callvirt, typeof(T).GetMethod("set_" + pi.Name, new Type[] { pi.PropertyType }));
```

After we've mapped all the properties, we'll load the T instance onto the stack and return it.

```csharp
// Load the T instance onto the stack
il.Emit(OpCodes.Ldloc_0);

// Return
il.Emit(OpCodes.Ret);
```

To improve performance, let's cache this mapper method as it'll work for the type T the next time we need it. Notice that what we're caching is not the method itself, but a delegate to the method - enabling us to actually call the method.

```csharp
private delegate T mapEntity<T>(DbDataReader dr);
private static Dictionary<Type, Delegate> cachedMappers = new Dictionary<Type, Delegate>();

// Cache the method so we won't have to create it again for the type T
cachedMappers.Add(typeof(T), dm.CreateDelegate(typeof(mapEntity<T>)));

// Get a delegate reference to the dynamic method
mapEntity<T> invokeMapEntity = (mapEntity<T>)cachedMappers[typeof(T)];
```

Now all we gotta do is loop the DbDataReader rows and return the mapped entities.

```csharp
// For each row, map the row to an instance of T and yield return it
while (dr.Read())
	yield return invokeMapEntity(dr);
```

And of course, here's the final method/class. Remember that this is more a proof of concept than a complete class. It ought to handle all necessary types. Also, it might be relevant to consider whether one should map public as well as private properties. Whether it should handle type validation errors, missing columns or not, I'm not sure. As it is now, it'll throw a normal ArgumentOutOfRangeException in cases of missing columns, and relevant type conversion errors - all those can be handled by the callee using try/catch blocks.

```csharp
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using System.Reflection.Emit;

namespace Improve.Framework.Data
{
	public class EntityMapper
	{
		private delegate T mapEntity<T>(DbDataReader dr);
		private static Dictionary<Type, Delegate> cachedMappers = new Dictionary<Type, Delegate>();

		public static IEnumerable<T> MapToEntities<T>(DbDataReader dr)
		{
			// If a mapping function from dr -> T does not exist, create and cache one
			if (!cachedMappers.ContainsKey(typeof(T)))
			{
				// Our method will take in a single parameter, a DbDataReader
				Type[] methodArgs = { typeof(DbDataReader) };

				// The MapDR method will map a DbDataReader row to an instance of type T
				DynamicMethod dm = new DynamicMethod("MapDR", typeof(T), methodArgs, Assembly.GetExecutingAssembly().GetType().Module);
				ILGenerator il = dm.GetILGenerator();
				
				// We'll have a single local variable, the instance of T we're mapping
				il.DeclareLocal(typeof(T));

				// Create a new instance of T and save it as variable 0
				il.Emit(OpCodes.Newobj, typeof(T).GetConstructor(Type.EmptyTypes));
				il.Emit(OpCodes.Stloc_0);

				foreach (PropertyInfo pi in typeof(T).GetProperties())
				{
					// Load the T instance, SqlDataReader parameter and the field name onto the stack
					il.Emit(OpCodes.Ldloc_0);
					il.Emit(OpCodes.Ldarg_0);
					il.Emit(OpCodes.Ldstr, pi.Name);

					// Push the column value onto the stack
					il.Emit(OpCodes.Callvirt, typeof(DbDataReader).GetMethod("get_Item", new Type[] { typeof(string) }));

					// Depending on the type of the property, convert the datareader column value to the type
					switch (pi.PropertyType.Name)
					{
						case "Int16":
							il.Emit(OpCodes.Call, typeof(Convert).GetMethod("ToInt16", new Type[] { typeof(object) }));
							break;
						case "Int32":
							il.Emit(OpCodes.Call, typeof(Convert).GetMethod("ToInt32", new Type[] { typeof(object) }));
							break;
						case "Int64":
							il.Emit(OpCodes.Call, typeof(Convert).GetMethod("ToInt64", new Type[] { typeof(object) }));
							break;
						case "Boolean":
							il.Emit(OpCodes.Call, typeof(Convert).GetMethod("ToBoolean", new Type[] { typeof(object) }));
							break;
						case "String":
							il.Emit(OpCodes.Callvirt, typeof(string).GetMethod("ToString", new Type[] { }));
							break;
						case "DateTime":
							il.Emit(OpCodes.Call, typeof(Convert).GetMethod("ToDateTime", new Type[] { typeof(object) }));
							break;
						case "Decimal":
							il.Emit(OpCodes.Call, typeof(Convert).GetMethod("ToDecimal", new Type[] { typeof(object) }));
							break;
						default:
							// Don't set the field value as it's an unsupported type
							continue;
					}

					// Set the T instances property value
					il.Emit(OpCodes.Callvirt, typeof(T).GetMethod("set_" + pi.Name, new Type[] { pi.PropertyType }));
				}

				// Load the T instance onto the stack
				il.Emit(OpCodes.Ldloc_0);

				// Return
				il.Emit(OpCodes.Ret);
				
				// Cache the method so we won't have to create it again for the type T
				cachedMappers.Add(typeof(T), dm.CreateDelegate(typeof(mapEntity<T>)));
			}

			// Get a delegate reference to the dynamic method
			mapEntity<T> invokeMapEntity = (mapEntity<T>)cachedMappers[typeof(T)];
			
			// For each row, map the row to an instance of T and yield return it
			while (dr.Read())
				yield return invokeMapEntity(dr);
		}

		public static void ClearCachedMapperMethods()
		{
			cachedMappers.Clear();
		}
	}
}
```
