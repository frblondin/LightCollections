# What is LightCollections used for?

The .Net Framework provides powerful generic collections (List<T>, Dictionary<TKey, TValue>) but their memory management can become really bad when dealing with high volumes of data.

These implementations are internally using arrays whose size is multiplied by two (or the first primary value after multiplying the existing size by two for dictionaries) when the size turns out to be insufficient.

This leads to two issues:
* Many arrays are created-and-replaced. The Garbage Collector has many instances to collect,
* Worst thing is that when the collection starts being large (> 85k, or even before if stored elements are value-types in many cases) the instances are stored in the Large Object Heap. This causes the memory to become more fragmented.

This package is an attempt to avoid creating arrays that will be droped when the size of the collection become insufficient and make sure that nothing will be stored in the Large Object Heap.

# How?

To avoid droping arrays when they become too small, the idea is to add additional arrays to store the additional values. The size of these arrays is computed in such a way that they won't be stored in the Large Object Heap.

# Actual performance results

TODO :-)
