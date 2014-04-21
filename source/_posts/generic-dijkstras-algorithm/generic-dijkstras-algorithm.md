---
permalink: generic-dijkstras-algorithm
title: Generic Dijkstra's Algorithm
date: 2008-05-13
tags: [.NET]
---
Through various projects, I've had to do some shortest-path finding in a connected graph. An efficient and straight-forward way to do this is using [Dijkstra's Algorithm](http://en.wikipedia.org/wiki/Dijkstra's_algorithm). Notice that it'll only work for graphs with non negative path weights, like 2D maps for instance. While I've used the algorithm on several occasions, it's only now that I've rewritten it in generic form

<!-- more -->

The code is pretty much self explanatory if you keep the [pseudo code implementation](http://en.wikipedia.org/wiki/Dijkstra's_algorithm#Pseudocode) next to it.

```csharp
using System;
using System.Collections.Generic;
using System.Linq;

namespace Improve.Framework.Algorithms
{
	public class Dijkstra<TNode>
	{
		/// <summary>
		/// Calculates the shortest route from a source node to a target node given a set of nodes and connections. Will only work for graphs with non-negative path weights.
		/// </summary>
		/// <param name="connections">All the nodes, as well as the list of their connections.</param>
		/// <param name="sourceNode">The node to start from.</param>
		/// <param name="targetNode">The node we should seek.</param>
		/// <param name="fnEquals">A function used for testing if two nodes are equal.</param>
		/// <param name="fnDistance">A function used for calculating the distance/weight between two nodes.</param>
		/// <returns>An ordered list of nodes from source->target giving the shortest path from the source to the target node. Returns null if no path is possible.</returns>
		public static List<TNode> ShortestPath(IDictionary<TNode, List<TNode>> connections, TNode sourceNode, TNode targetNode, Func<TNode, TNode, bool> fnEquals, Func<TNode, TNode, double> fnDistance)
		{
			// Initialize values
			Dictionary<TNode, double> distance = new Dictionary<TNode, double>(); ;
			Dictionary<TNode, TNode> previous = new Dictionary<TNode, TNode>(); ;
			List<TNode> localNodes = new List<TNode>();

			// For all nodes, copy it to our local list as well as set it's distance to null as it's unknown
			foreach (TNode node in connections.Keys)
			{
				localNodes.Add(node);
				distance.Add(node, double.PositiveInfinity);
			}

			// We know the distance from source->source is 0 by definition
			distance[sourceNode] = 0;

			while (localNodes.Count > 0)
			{
				// Return and remove best vertex (that is, connection with minimum distance
				TNode minNode = localNodes.OrderBy(n => distance[n]).First();
				localNodes.Remove(minNode);

				// Loop all connected nodes
				foreach (TNode neighbor in connections[minNode])
				{
					// The positive distance between node and it's neighbor, added to the distance of the current node
					double dist = distance[minNode] + fnDistance(minNode, neighbor);

					if (dist < distance[neighbor])
					{
						distance[neighbor] = dist;
						previous[neighbor] = minNode;
					}
				}

				// If we're at the target node, break
				if (fnEquals(minNode, targetNode))
					break;
			}

			// Construct a list containing the complete path. We'll start by looking at the previous node of the target and then making our way to the beginning.
			// We'll reverse it to get a source->target list instead of the other way around. The source node is manually added.
			List<TNode> result = new List<TNode>();
			TNode target = targetNode;
			while (previous.ContainsKey(target))
			{
				result.Add(target);
				target = previous[target];
			}
			result.Add(sourceNode);
			result.Reverse();

			if (result.Count > 1)
				return result;
			else
				return null;
		}
	}
}
```

Once we've made the list of nodes & connections, invoking the algorithm is rather simple. We just need to provide an equality function as well as a distance function:

```csharp
// Run Dijkstra's algorithm
List<Point2D> result = Dijkstra<Point2D>.ShortestPath(connections, mouse, target, (p1, p2) => p1 == p2, (p1, p2) => p1.DistanceTo(p2));
```


{% youtube ctEXUmZ5TDY %}


## Downloads

[Shortest_Path.zip - Sample code](Shortest_Path.zip)
