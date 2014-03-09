permalink: as3-numbers-get-real
title: AS3 Numbers - Get Real
date: 2008-06-12
tags: [AS/Flex/Flash]
---
Skilled developers are hard to come by these days, that includes Flash/AS3/Flex developers. As the product I'm working on is very much dependent on a Flash based frontend, I've been forced to learn & work with AS3 & Flex recently.

<!-- more -->

It's a great experience, learning a new language - especially now that Silverlight is marching forward. As the old saying goes, know your enemy. Anyways, enough with the chit chatting, on with the problems.

Today I tried making a very simple functionality, I wanted to be able to select a number of images by making them highlight when selected, and dim out when deselected:

```mxml
<?xml version="1.0" encoding="utf-8"?>
<mx:Application
	xmlns:mx="http://www.adobe.com/2006/mxml"
	layout="absolute">

	<mx:Script>
		<![CDATA[
			private function onClick():void
			{
				if(btnTest.alpha == 0.5)
					btnTest.alpha = 1;
				else
					btnTest.alpha = 0.5;
			}
		]]>
	</mx:Script>

	<mx:Button id="btnTest" click="onClick()" label="Click me!"/>

</mx:Application>
```

[A.swf - Demo](http://improve.dk/wp-content/uploads/2008/06/120.swf)

In this case the button is "selected" from the start (alpha = 1). When clicked, the alpha changes to half opaque (0.5), switches back to 1 when reclicked, and so forth. All working good.

But it's a bit hard to differentiate between selected and non selected, so let's change the alpha setting to 0.4 instead of 0.5:

```mxml
<?xml version="1.0" encoding="utf-8"?>
<mx:Application
	xmlns:mx="http://www.adobe.com/2006/mxml"
	layout="absolute">

	<mx:Script>
		<![CDATA[
			private function onClick():void
			{
				if(btnTest.alpha == 0.4)
					btnTest.alpha = 1;
				else
					btnTest.alpha = 0.4;
			}
		]]>
	</mx:Script>

	<mx:Button id="btnTest" click="onClick()" label="Click me!"/>

</mx:Application>
```

[B.swf - Demo](http://improve.dk/wp-content/uploads/2008/06/121.swf)

But what's this, now we're only able to dim it, not reselect it. Why's that? Nothing's changed other than the alpha value. The problem becomes apparent if we trace out the buttons alpha value like so:

```mxml
<?xml version="1.0" encoding="utf-8"?>
<mx:Application
	xmlns:mx="http://www.adobe.com/2006/mxml"
	layout="absolute">

	<mx:Script>
		<![CDATA[
			private function onClick():void
			{
				if(btnTest.alpha == 0.4)
					btnTest.alpha = 1;
				else
					btnTest.alpha = 0.4;

				mx.controls.Alert.show(btnTest.alpha.toString());
			}
		]]>
	</mx:Script>

	<mx:Button id="btnTest" click="onClick()" label="Click me!"/>

</mx:Application>
```

[C.swf - Demo](http://improve.dk/wp-content/uploads/2008/06/122.swf)

In case you don't have Flash 9 installed, or are just too lazy to click the button, the resulting Alert box shows the following value: 0.3984375 - obviously not quite the 0.4 we specified.

Ok, let's dumb it down a bit and do some quick testing:

```mxml
<?xml version="1.0" encoding="utf-8"?>
<mx:Application
	xmlns:mx="http://www.adobe.com/2006/mxml"
	layout="absolute"
	initialize="onInitialize()">

	<mx:Script>
		<![CDATA[

			private function onInitialize():void
			{
				// true
				trace(0.4 == 0.4);

				// true
				var zeroPointFour:Number = 0.4;
				trace(zeroPointFour == 0.4);

				// true
				var secondZeroPointFour:Number = 0.4;
				trace(zeroPointFour == secondZeroPointFour);

				// false
				var testSprite:Sprite = new Sprite();
				testSprite.alpha = 0.4;
				trace(testSprite.alpha == 0.4);

				// 0.3984375
				trace(testSprite.alpha);

				// false - duh
				trace(0.3984375 == 0.4)
			}

		]]>
	</mx:Script>

</mx:Application>
```

Now this is where things start getting weird. We can obviously store the value 0.4 in a number (which is a 64 bit double-precision format according to the [IEEE-754 spec](http://en.wikipedia.org/wiki/IEEE_754)). Furthermore, we're also able to compare two instances of Number with the value 0.4 and get the expected equality comparison result, true. Now, it would seem that as soon we set the alpha value on our Sprite, it's corrupted. Sprite inherits the alpha property from [DisplayObject](http://livedocs.adobe.com/flex/2/langref/flash/display/DisplayObject.html) - which obviously lists alpha as a value of type Number.

Why does this happen? It's no problem storing the value 0.4 in a 64 bit double precision number:

```

Sign: +1
Exponent: -2
Mantissa: 1.6
Result: sign x 2<sup>exponent</sup> x mantissa => +1 x 2<sup>-2</sup> x 1.6 = 0.4

```

It might be (and probably is) me not understanding something right. Can somebody explain to me how the Flash VM handles Numbers, and thereby, explain why this is happening? Is it perhaps not due to the VM's handling of Numbers, but instead just a simple matter of an odd implementation of the alpha property on DisplayObject?
