| Total memory traffic | 257,392 Mbytes || LoH memory usage | 0 :-) |
| Setting values | 4,566 ms |
| Getting values | 2,617 ms |
| Total duration | 7,184 ms |

# What is LightCollections used for?

The .Net Framework provides powerful generic collections (List<T>, Dictionary<TKey, TValue>) but their memory management can become really bad when dealing with high volumes of data.

These implementations are internally using arrays whose size is multiplied by two (or the first primary value after multiplying the existing size by two for dictionaries) when the size turns out to be insufficient.

This leads to two issues:
* Many arrays are created-and-replaced. The Garbage Collector has many instances to collect as this creates large memory traffic,
* Worst thing is that when the collection starts being large (> 85k, or even before if stored elements are structures in many cases) the instances are stored in the Large Object Heap. This causes the memory to become more fragmented.

This package is an attempt to avoid creating arrays that will be droped when the size of the collection become insufficient and make sure that nothing will be stored in the Large Object Heap.

# How?

To avoid droping arrays when they become too small, the idea is to add additional arrays to store the additional values. The size of these arrays is computed in such a way that they won't be stored in the Large Object Heap.

Here is how the standard dictionary behaves: starts by creating small internal bucket & value arrays. If more values are required, new bigger arrays are created and are replacing older ones. Here is a typical growth: 7, 17, 37, 89, 197, 431, 919, 1931, 4049, 8419, 17519, 36353, 75431, 156437, 324449, 672827, 1395263, 2893249, 5999471, 11998949 
The light dictionary only drops arrays when they keep being small. Once they reach the LoH limit then fixed size arrays are appended. The max size depends on the array types.


# Actual performance results

I did some benchmarks to compare the performance between LightDictionary<string, int> and Dictionary. The test consists of setting 10,000,000 values and getting all the values.

LightDictionary:
| Total memory traffic | 257,392 Mbytes |
| LoH memory usage | 0 :-) |
| Setting values | 4,566 ms |
| Getting values | 2,617 ms |
| Total duration | 7,184 ms |

Dictionary:
Total memory traffic: 472,063 Mbytes
LoH memory usage: 471,481 Mbytes - largest array needs 50 Mbytes contiguous memory
Setting values: 4,129 ms
Getting values: 2,298 ms
Total duration: 6,428 ms

This shows that LightDictionary doesn't consume any memory in the Large Object Heap anymore. Furthermore the global memory traffice is twice as small. The drawback is that the LightDictionary is around 11% slower than the standard Dictionary.

# Plan for improving performance

I did many tunings to have the best performance. According to the profiler, the remaining reason why the LightDictionary is slow is because of the additional indexed array access: entries[n] is now replaced with entries[x][y].

Any help on that would be much appreciated as I can't see any way of improving this...
