---
permalink: evolution-of-the-simple-genetic-algorithm
title: Evolution of The Simple Genetic Algorithm
date: 2009-05-01
tags: [.NET]
---
Based on my previous post on how to [implement a simple genetic algorithm](http://www.improve.dk/blog/2009/04/29/implementing-a-simple-genetic-algorithm), I got some great comments pointing out that the algorithm might not be the most pure form of a genetic algorithm. I won't disagree, though I will point out that evolution also does occur due to mutation alone, so genetic algorithms may come in different forms.

<!-- more -->

## Making it more "genetic"

The basic point is that I was only mutating my chromosomes, I wasn't actually pairing them up and reproducing via a [crossover function](http://en.wikipedia.org/wiki/Crossover_(genetic_algorithm)).

I've changed my GeneticAlgorithm class slightly to give more control to the subclasses of how reproduction works, whether it be by mutation and/or crossover. What I've done is to change the Mutate method into a Reproduce method, taking in the complete survivor enumeration of IChromosome's instead. That way, the implementing subclass has full control over the selection and breeding of the survivors.

```csharp
/// <summary>
/// Creates an arbitrary number of mutated chromosomes, based on the input chromosome.
/// </summary>
protected abstract IEnumerable<IChromosome<T>> Reproduce(IEnumerable<IChromosome<T>> survivors);
```

The PerformEvolution method has been changed as well. Instead of selecting the survivor to mutate here, we let it be up to the subclass. Thus, we just loop until we've reached our ChromosomePopulationSize, while passing the survivors into the Reproduce method.

```csharp
/// <summary>
/// Performs an evolution by picking up the generation survivors and mutating them.
/// </summary>
public void PerformEvolution()
{
	IList<IChromosome<T>> newGeneration = new List<IChromosome<T>>();

	// Get the survivors of the last generation
	IEnumerable<IChromosome<T>> survivors = GetGenerationSurvivors();

	// Add the survivors of the previous generation
	foreach (var survivor in survivors)
		newGeneration.Add(survivor);

	// Until the population is full, add a new mutation of any two survivors, selected by weighted random based on their fitness.
	Random rnd = new Random();

	while (newGeneration.Count < ChromosomePopulationSize)
		foreach (var offspring in Reproduce(survivors))
		{
			newGeneration.Add(offspring);

			if (newGeneration.Count == ChromosomePopulationSize)
				break;
		}

	// Overwrite current population
	ChromosomePopulation = newGeneration;

	// Increment the current generation
	CurrentGenerationNumber++;
}
```

Instead of overriding Mutate() in the RgbGuesser class, we're now overriding Reproduce. Given that we have three basic genes in each of our chromosomes (R, G & B), we can produce 2^3 different combinations, including the original two combinations AAA and BBB. Each time Reproduce is called, we'll create all eight different combinations and mutate them slightly, before returning them to the GeneticAlgorithm. This way we're producing eight children of each parent pair of chromosomes, and thus, as Matt^2 pointed out, allowing the chromosomes of developing partial solutions themselves more efficiently.

```csharp
protected override IEnumerable<IChromosome<Rgb>> Reproduce(IEnumerable<IChromosome<Rgb>> survivors)
{
	// Get two random chromosomes from the survivors
	var chromA = survivors
		.OrderBy(c => random.NextDouble() * c.Fitness)
		.First();

	var chromB = survivors
		.OrderBy(c => random.NextDouble() * c.Fitness)
		.Where(c => c != chromA)
		.First();

	// Now generate and return each different combination based on the two parents, with slight mutation
	var aaa = new RgbChromosome(chromA.ChromosomeValue.R, chromA.ChromosomeValue.G, chromA.ChromosomeValue.B);
	mutateChromosome(aaa);
	yield return aaa;

	var aab = new RgbChromosome(chromA.ChromosomeValue.R, chromA.ChromosomeValue.G, chromB.ChromosomeValue.B);
	mutateChromosome(aab);
	yield return aab;

	var abb = new RgbChromosome(chromA.ChromosomeValue.R, chromB.ChromosomeValue.G, chromB.ChromosomeValue.B);
	mutateChromosome(abb);
	yield return abb;

	var aba = new RgbChromosome(chromA.ChromosomeValue.R, chromB.ChromosomeValue.G, chromA.ChromosomeValue.B);
	mutateChromosome(aba);
	yield return aba;

	var baa = new RgbChromosome(chromB.ChromosomeValue.R, chromA.ChromosomeValue.G, chromA.ChromosomeValue.B);
	mutateChromosome(baa);
	yield return baa;

	var bba = new RgbChromosome(chromB.ChromosomeValue.R, chromB.ChromosomeValue.G, chromA.ChromosomeValue.B);
	mutateChromosome(bba);
	yield return bba;

	var bab = new RgbChromosome(chromB.ChromosomeValue.R, chromA.ChromosomeValue.G, chromB.ChromosomeValue.B);
	mutateChromosome(bab);
	yield return bab;

	var bbb = new RgbChromosome(chromB.ChromosomeValue.R, chromB.ChromosomeValue.G, chromB.ChromosomeValue.B);
	mutateChromosome(bbb);
	yield return bbb;
}

private void mutateChromosome(IChromosome<Rgb> chromosome)
{
	chromosome.ChromosomeValue.R = Math.Min(255, Math.Max(0, chromosome.ChromosomeValue.R + random.Next(-5, 6)));
	chromosome.ChromosomeValue.G = Math.Min(255, Math.Max(0, chromosome.ChromosomeValue.G + random.Next(-5, 6)));
	chromosome.ChromosomeValue.B = Math.Min(255, Math.Max(0, chromosome.ChromosomeValue.B + random.Next(-5, 6)));
}
```

## Reducing the chance of mutation

As weenie points out in the comments, reducing the chance of mutation will actually improve results. By changing the mutateChromosome slightly, we only mutate it for every 1/3 reproductions.

At this point we've got several variables to tune - population size, survivor count, mutation probability & mutation severeness. This sounds like an optimal problem for an evolutionary algorithm :)

```csharp
private void mutateChromosome(IChromosome<Rgb> chromosome)
{
	if (random.Next(1, 4) == 1)
	{
		chromosome.ChromosomeValue.R = Math.Min(255, Math.Max(0, chromosome.ChromosomeValue.R + random.Next(-5, 6)));
		chromosome.ChromosomeValue.G = Math.Min(255, Math.Max(0, chromosome.ChromosomeValue.G + random.Next(-5, 6)));
		chromosome.ChromosomeValue.B = Math.Min(255, Math.Max(0, chromosome.ChromosomeValue.B + random.Next(-5, 6)));
	}
}
```

## Downloads

[GeneticTesting2.zip - Sample code](GeneticTesting2.zip)
