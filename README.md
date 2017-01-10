# What is LightCollections used for?

The .Net Framework provides powerful generic collections (List<T>, Dictionary<TKey, TValue>) but their memory management can become really bad when dealing with high volumes of data.

These implementations are internally using arrays whose size is multiplied by two (or the first primary value after multiplying the existing size by two for dictionaries) when the size turns out to be insufficient.

This leads to two issues:
* Many arrays are created-and-replaced. The Garbage Collector has many instances to collect as this creates large memory traffic,
* Worst thing is that when the collection starts being large (> 85k, or even before if stored elements are structures in many cases) the instances are stored in the Large Object Heap. This causes the memory to become more fragmented.

This package is an attempt to avoid creating arrays that will be droped when the size of the collection become insufficient and make sure that nothing will be stored in the Large Object Heap.

The package contains mainly two types:
* JaggedDictionary: used to store data on multiple dimensions,
* LightDictionary: used to store large data without impacting the LoH.

# Zoom on LightDictionary

## How?

To avoid droping arrays when they become too small, the idea is to add additional arrays to store the additional values. The size of these arrays is computed in such a way that they won't be stored in the Large Object Heap.

Here is how the standard dictionary behaves: starts by creating small internal bucket & value arrays. If more values are required, new bigger arrays are created and are replacing older ones. Here is a typical growth: 7, 17, 37, 89, 197, 431, 919, 1931, 4049, 8419, 17519, 36353, 75431, 156437, 324449, 672827, 1395263, 2893249, 5999471, 11998949 
The light dictionary only drops arrays when they keep being small. Once they reach the LoH limit then fixed size arrays are appended. The max size depends on the array types.

## Actual performance results

I did some benchmarks to compare the performance between LightDictionary<string, int> and Dictionary. The test consists of setting 10,000,000 values and getting all the values.

| Type | Total memory traffic | LoH memory usage | Setting values | Getting values | Total duration |
|---|---|---|---|---|---|
|LightDictionary| 257,392 Mbytes | 0 :-) | 4,566 ms | 2,617 ms | 7,184 ms |
|Dictionary| 472,063 Mbytes | 471,481 Mbytes - largest array needs 50 Mbytes contiguous memory | 4,129 ms | 2,298 ms | 6,428 ms |

This shows that LightDictionary doesn't consume any memory in the Large Object Heap anymore. Furthermore the global memory traffice is twice as small. The drawback is that the LightDictionary is around 11% slower than the standard Dictionary.

## Plan for improving performance

I did many tunings to have the best performance. According to the profiler, the remaining reason why the LightDictionary is slow is because of the additional indexed array access: entries[n] is now replaced with entries[x][y].

Any help on that would be much appreciated as I can't see any way of improving this...

# Build

This project is built on appveyor: [![Build status](https://ci.appveyor.com/api/projects/status/exbcgj2qrtc3is9q?svg=true)](https://ci.appveyor.com/project/frblondin/lightcollections)

And available on nuget: [Nuget](https://www.nuget.org/packages/Blondin.LightCollections/)

This library depends on ExpressionReflection/StaticReflection library: https://github.com/mtranter/StaticReflection/

# License

This library is licensed under MIT License. See LICENSE file
