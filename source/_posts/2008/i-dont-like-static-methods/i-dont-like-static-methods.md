permalink: i-dont-like-static-methods
title: I Don't Like Static Methods
date: 2008-10-16
tags: [.NET]
---
Inspired by a recent [question on StackOverflow](http://stackoverflow.com/questions/205689/class-with-single-method-best-approach), I felt like sharing my thoughts on static methods in general.

I used to love utility classes filled up with static methods. They made a great consolidation of helper methods that would otherwise lie around causing redundancy and maintenance hell. They're very easy to use, no instantiation, no disposal, just fire'n'forget. I guess this was my first unwitting attempt at creating a service oriented architecture - lots of stateless services that just did their job and nothing else. As a system grows however, dragons be coming.

## Polymorphism

Say we have the method UtilityClass.SomeMethod that happily buzzes along. Suddenly we need to change the functionality slightly. Most of the functionality is the same, but we have to change a couple of parts nonetheless. Had it not been a static method, we could make a derivate class and change the method contents as needed. As it's a static method, we can't. Sure, if we just need to add functionality either before or after the old method, we can create a new class and call the old one inside of it - but that's just gross.

## Interface woes

Static methods cannot be defined through interfaces for logic reasons. And since we can't override static methods, static classes are useless when we need to pass them around by their interface. This renders us unable to use static classes as part of a strategy pattern. We might patch some issues up by [passing delegates instead of interfaces](http://blogs.msdn.com/kirillosenkov/archive/2008/02/06/how-to-override-static-methods.aspx).

## Testing

This basically goes hand in hand with the interface woes mentioned above. As our ability of interchanging implementations is very limited, we'll also have trouble replacing production code with test code. Again, we can wrap them up but it'll require us to change large parts of our code just to be able to accept wrappers instead of the actual objects.

## Fosters blobs

As static methods are usually used as utility methods and utility methods usually will have different purposes, we'll quickly end up with a large class filled up with non-coherent functionality - ideally, each class should have a single purpose within the system. I'd much rather have a five times the classes as long as their purposes are well defined.

## Parameter creep

To begin with, that little cute and innocent static method might take a single parameter. As functionality grows, a couple of new parameters are added. Soon further parameters are added that are optional, so we create overloads of the method (or just add default values, in languages that support them). Before long, we have a method that takes 10 parameters. Only the first three are really required, parameters 4-7 are optional. But if parameter 6 is specified, 7-9 are required to be filled in as well... Had we created a class with the single purpose of doing what this static method did, we could solve this by taking in the required parameters in the constructor, and allowing the user to set optional values through properties, or methods to set multiple interdependent values at the same time. Also, if a method has grown to this amount of complexity, it most likely needs to be in its own class anyways.

## Demanding consumers to create an instance of classes for no reason

One of the most common arguments is, why demand that consumers of our class create an instance for invoking this single method, while having no use for the instance afterwards? Creating an instance of a class is a very very cheap operation in most languages, so speed is not an issue. Adding an extra line of code to the consumer is a low cost for laying the foundation of a much more maintainable solution in the future. And finally, if you want to avoid creating instances, simply create a singleton wrapper of your class that allows for easy reuse - although this does make the requirement that your class is stateless. If it's not stateless, you can still create static wrapper methods that handle everything, while still giving you all the benefits in the long run. Finally, you could also make a class that hides the instantiation as if it was a singleton: MyWrapper.Instance is a property that just returns new MyClass();

## Only a Sith deals in absolutes

Of course, there are exceptions to my dislike of static methods. True utility classes that do not pose any risk to bloat are excellent cases for static methods - System.Convert as an example. If your project is a one-off with no requirements for future maintenance, the overall architecture really isn't very important - static or non static, doesn't really matter - development speed does, however.

## Standards, standards, standards!

Using instance methods does not inhibit you from also using static methods, and vice versa. As long as there's reasoning behind the differentiation and it's standardised. There's nothing worse than looking over a business layer sprawling with different implementation methods.
