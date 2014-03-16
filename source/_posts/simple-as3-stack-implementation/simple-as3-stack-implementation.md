permalink: simple-as3-stack-implementation
title: Simple AS3 Stack Implementation
date: 2010-04-21
tags: [AS/Flex/Flash]
---
Recently I was doing some experimental AS3 development. Much to my surprise, simple collection classes like Stack/Queue are not available in the framework - guess I'm spoiled being used to the .NET Framework.

<!-- more -->

I ended up implementing a simple [stack](http://en.wikipedia.org/wiki/Stack_(data_structure)) using an internal [linked list](http://en.wikipedia.org/wiki/Linked_list). There's nothing exciting about the implementation but I thought others might be able to use it, so here it is :)

### StackNode.as

```actionscript
package dk.improve.collections
{
    internal final class StackNode
    {
        public var value:Object;
        public var next:StackNode;

        public function StackNode(value:Object):void
        {
            this.value = value;
        }
    }
}
```

### Stack.as

```actionscript
package dk.improve.collections
{
    public class Stack
    {
        private var head:StackNode;

        public function push(obj:Object):void
        {
            var newNode:StackNode = new StackNode(obj);

            if(head == null)
                head = newNode;
            else
            {
                newNode.next = head;
                head = newNode;
            }
        }

        public function pop():Object
        {
            if(head != null)
            {
                var result:Object = head.value;
                head = head.next;

                return result;
            }
            else
                return null;
        }

        public function peek():Object
        {
            if(head != null)
                return head.value;
            else
                return null;
        }
    }
}
```