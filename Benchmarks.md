<h1 align="center">
  <a>Benchmarks</a>
</h1>

Any benchmarks are merely specific use cases provided as examples. Don't take them at face value. Сheck the code to see if it meets your expectations. If they don't match how you plan to use the library, run your own benchmark.

<h1 align="center">
  <a>GetMemoryCacheBench</a>
</h1>

[Code](https://github.com/SoftStoneDevelop/L1CacheSystem/blob/docs/benchmarks/Src/Benchmarks/Cases/Entity/GetMemoryCacheBench.cs)

| Method                      | Runtime        | Size   | Mean        | Error      | StdDev     | Median      | Ratio | RatioSD | Allocated | Alloc Ratio |
|---------------------------- |--------------- |------- |------------:|-----------:|-----------:|------------:|------:|--------:|----------:|------------:|
| **&#39;Get IMemoryCache&#39;**          | **.NET 11.0**      | **10000**  |   **350.17 μs** |   **3.101 μs** |   **2.590 μs** |   **350.30 μs** |  **1.00** |    **0.00** |  **240000 B** |        **1.00** |
| &#39;Get CacheSystem Reference&#39; | .NET 11.0      | 10000  |    34.01 μs |   0.525 μs |   0.466 μs |    33.95 μs |  0.10 |    0.00 |         - |        0.00 |
|                             |                |        |             |            |            |             |       |         |           |             |
| &#39;Get IMemoryCache&#39;          | NativeAOT 11.0 | 10000  |   392.62 μs |   4.761 μs |   4.220 μs |   393.65 μs |  1.00 |    0.00 |  240064 B |       1.000 |
| &#39;Get CacheSystem Reference&#39; | NativeAOT 11.0 | 10000  |    57.16 μs |   0.788 μs |   0.658 μs |    57.10 μs |  0.15 |    0.00 |      64 B |       0.000 |
|                             |                |        |             |            |            |             |       |         |           |             |
| **&#39;Get IMemoryCache&#39;**          | **.NET 11.0**      | **25000**  |   **914.24 μs** |  **33.250 μs** |  **94.324 μs** |   **865.70 μs** |  **1.00** |    **0.00** |  **600000 B** |        **1.00** |
| &#39;Get CacheSystem Reference&#39; | .NET 11.0      | 25000  |   130.10 μs |  15.137 μs |  41.690 μs |   115.10 μs |  0.14 |    0.05 |         - |        0.00 |
|                             |                |        |             |            |            |             |       |         |           |             |
| &#39;Get IMemoryCache&#39;          | NativeAOT 11.0 | 25000  | 1,079.01 μs |  28.081 μs |  81.912 μs | 1,054.95 μs |  1.00 |    0.00 |  600064 B |       1.000 |
| &#39;Get CacheSystem Reference&#39; | NativeAOT 11.0 | 25000  |   200.11 μs |  24.233 μs |  69.138 μs |   172.05 μs |  0.19 |    0.07 |      64 B |       0.000 |
|                             |                |        |             |            |            |             |       |         |           |             |
| **&#39;Get IMemoryCache&#39;**          | **.NET 11.0**      | **50000**  | **2,231.94 μs** |  **59.040 μs** | **171.287 μs** | **2,236.00 μs** |  **1.00** |    **0.00** | **1200000 B** |        **1.00** |
| &#39;Get CacheSystem Reference&#39; | .NET 11.0      | 50000  |   661.91 μs |  13.707 μs |  39.108 μs |   657.35 μs |  0.30 |    0.03 |         - |        0.00 |
|                             |                |        |             |            |            |             |       |         |           |             |
| &#39;Get IMemoryCache&#39;          | NativeAOT 11.0 | 50000  | 2,556.48 μs |  69.872 μs | 202.712 μs | 2,573.80 μs |  1.00 |    0.00 | 1200064 B |       1.000 |
| &#39;Get CacheSystem Reference&#39; | NativeAOT 11.0 | 50000  |   978.40 μs |  69.368 μs | 204.534 μs |   921.30 μs |  0.39 |    0.09 |      64 B |       0.000 |
|                             |                |        |             |            |            |             |       |         |           |             |
| **&#39;Get IMemoryCache&#39;**          | **.NET 11.0**      | **100000** | **5,084.11 μs** | **128.906 μs** | **369.855 μs** | **5,081.90 μs** |  **1.00** |    **0.00** | **2400000 B** |        **1.00** |
| &#39;Get CacheSystem Reference&#39; | .NET 11.0      | 100000 | 1,480.74 μs |  37.293 μs | 107.001 μs | 1,487.30 μs |  0.29 |    0.03 |         - |        0.00 |
|                             |                |        |             |            |            |             |       |         |           |             |
| &#39;Get IMemoryCache&#39;          | NativeAOT 11.0 | 100000 | 5,645.18 μs | 111.902 μs | 302.533 μs | 5,642.90 μs |  1.00 |    0.00 | 2400064 B |       1.000 |
| &#39;Get CacheSystem Reference&#39; | NativeAOT 11.0 | 100000 | 1,804.99 μs |  55.886 μs | 162.137 μs | 1,807.90 μs |  0.32 |    0.03 |      64 B |       0.000 |


<h1 align="center">
  <a>SetMemoryCacheBench</a>
</h1>

| Method             | Runtime        | Size   | Mean        | Error       | StdDev      | Median      | Ratio | RatioSD | Gen0      | Gen1      | Gen2      | Allocated | Alloc Ratio |
|------------------- |--------------- |------- |------------:|------------:|------------:|------------:|------:|--------:|----------:|----------:|----------:|----------:|------------:|
| **&#39;Set IMemoryCache&#39;** | **.NET 11.0**      | **10000**  |    **979.1 μs** |    **19.57 μs** |    **52.57 μs** |    **983.4 μs** |  **1.00** |    **0.00** |         **-** |         **-** |         **-** |   **3.53 MB** |        **1.00** |
| &#39;Set CacheSystem&#39;  | .NET 11.0      | 10000  |    927.0 μs |    43.41 μs |   126.63 μs |    909.3 μs |  0.95 |    0.14 |         - |         - |         - |   2.77 MB |        0.78 |
|                    |                |        |             |             |             |             |       |         |           |           |           |           |             |
| &#39;Set IMemoryCache&#39; | NativeAOT 11.0 | 10000  |  1,081.3 μs |    24.91 μs |    72.66 μs |  1,056.3 μs |  1.00 |    0.00 |         - |         - |         - |   3.53 MB |        1.00 |
| &#39;Set CacheSystem&#39;  | NativeAOT 11.0 | 10000  |  1,036.4 μs |    39.22 μs |   115.04 μs |  1,021.0 μs |  0.96 |    0.12 |         - |         - |         - |   2.77 MB |        0.78 |
|                    |                |        |             |             |             |             |       |         |           |           |           |           |             |
| **&#39;Set IMemoryCache&#39;** | **.NET 11.0**      | **25000**  |  **7,741.2 μs** |   **391.35 μs** | **1,153.91 μs** |  **7,450.0 μs** |  **1.00** |    **0.00** | **1000.0000** |         **-** |         **-** |   **8.54 MB** |        **1.00** |
| &#39;Set CacheSystem&#39;  | .NET 11.0      | 25000  |  2,190.6 μs |    90.28 μs |   264.78 μs |  2,188.6 μs |  0.29 |    0.05 |         - |         - |         - |   6.63 MB |        0.78 |
|                    |                |        |             |             |             |             |       |         |           |           |           |           |             |
| &#39;Set IMemoryCache&#39; | NativeAOT 11.0 | 25000  |  7,629.6 μs |   175.97 μs |   496.33 μs |  7,655.5 μs |  1.00 |    0.00 | 1000.0000 |         - |         - |   8.54 MB |        1.00 |
| &#39;Set CacheSystem&#39;  | NativeAOT 11.0 | 25000  |  2,551.0 μs |    50.27 μs |   101.55 μs |  2,512.7 μs |  0.34 |    0.03 |         - |         - |         - |   6.63 MB |        0.78 |
|                    |                |        |             |             |             |             |       |         |           |           |           |           |             |
| **&#39;Set IMemoryCache&#39;** | **.NET 11.0**      | **50000**  | **15,699.1 μs** |   **310.74 μs** |   **648.62 μs** | **15,592.1 μs** |  **1.00** |    **0.00** | **2000.0000** | **1000.0000** |         **-** |  **17.25 MB** |        **1.00** |
| &#39;Set CacheSystem&#39;  | .NET 11.0      | 50000  |  8,664.4 μs |   158.67 μs |   256.22 μs |  8,592.8 μs |  0.55 |    0.03 | 1000.0000 |         - |         - |  13.43 MB |        0.78 |
|                    |                |        |             |             |             |             |       |         |           |           |           |           |             |
| &#39;Set IMemoryCache&#39; | NativeAOT 11.0 | 50000  | 15,700.1 μs |   312.83 μs |   737.38 μs | 15,623.6 μs |  1.00 |    0.00 | 2000.0000 | 1000.0000 |         - |  17.25 MB |        1.00 |
| &#39;Set CacheSystem&#39;  | NativeAOT 11.0 | 50000  |  9,174.6 μs |   177.80 μs |   224.86 μs |  9,143.0 μs |  0.59 |    0.03 | 1000.0000 |         - |         - |  13.43 MB |        0.78 |
|                    |                |        |             |             |             |             |       |         |           |           |           |           |             |
| **&#39;Set IMemoryCache&#39;** | **.NET 11.0**      | **100000** | **57,309.8 μs** | **1,134.39 μs** | **3,066.89 μs** | **57,461.8 μs** |  **1.00** |    **0.00** | **5000.0000** | **4000.0000** | **1000.0000** |  **34.84 MB** |        **1.00** |
| &#39;Set CacheSystem&#39;  | .NET 11.0      | 100000 | 37,122.0 μs | 1,445.08 μs | 4,215.36 μs | 38,687.0 μs |  0.65 |    0.08 | 4000.0000 | 3000.0000 | 1000.0000 |  27.21 MB |        0.78 |
|                    |                |        |             |             |             |             |       |         |           |           |           |           |             |
| &#39;Set IMemoryCache&#39; | NativeAOT 11.0 | 100000 | 56,212.1 μs | 1,107.77 μs | 2,186.64 μs | 56,685.8 μs |  1.00 |    0.00 | 5000.0000 | 4000.0000 | 1000.0000 |  34.84 MB |        1.00 |
| &#39;Set CacheSystem&#39;  | NativeAOT 11.0 | 100000 | 39,963.6 μs |   790.02 μs |   909.79 μs | 40,263.6 μs |  0.71 |    0.03 | 4000.0000 | 3000.0000 | 1000.0000 |  27.21 MB |        0.78 |


[Code](https://github.com/SoftStoneDevelop/L1CacheSystem/blob/docs/benchmarks/Src/Benchmarks/Cases/Entity/SetMemoryCacheBench.cs)

<h1 align="center">
  <a>DeleteMemoryCacheBench</a>
</h1>

| Method                | Runtime        | Size   | Mean       | Error     | StdDev    | Median     | Ratio | RatioSD | Allocated | Alloc Ratio |
|---------------------- |--------------- |------- |-----------:|----------:|----------:|-----------:|------:|--------:|----------:|------------:|
| **&#39;Delete IMemoryCache&#39;** | **.NET 11.0**      | **10000**  |   **460.2 μs** |   **5.23 μs** |   **4.64 μs** |   **460.3 μs** |  **1.00** |    **0.00** |  **240000 B** |        **1.00** |
| &#39;Delete CacheSystem&#39;  | .NET 11.0      | 10000  |   202.0 μs |   2.20 μs |   1.84 μs |   201.7 μs |  0.44 |    0.01 |         - |        0.00 |
|                       |                |        |            |           |           |            |       |         |           |             |
| &#39;Delete IMemoryCache&#39; | NativeAOT 11.0 | 10000  |   518.6 μs |   9.93 μs |   8.29 μs |   518.0 μs |  1.00 |    0.00 |  240064 B |       1.000 |
| &#39;Delete CacheSystem&#39;  | NativeAOT 11.0 | 10000  |   221.9 μs |   3.64 μs |   3.04 μs |   221.2 μs |  0.43 |    0.01 |      64 B |       0.000 |
|                       |                |        |            |           |           |            |       |         |           |             |
| **&#39;Delete IMemoryCache&#39;** | **.NET 11.0**      | **25000**  | **1,196.2 μs** |  **36.12 μs** | **103.04 μs** | **1,146.1 μs** |  **1.00** |    **0.00** |  **600000 B** |        **1.00** |
| &#39;Delete CacheSystem&#39;  | .NET 11.0      | 25000  |   531.4 μs |  10.57 μs |  24.07 μs |   524.9 μs |  0.45 |    0.04 |         - |        0.00 |
|                       |                |        |            |           |           |            |       |         |           |             |
| &#39;Delete IMemoryCache&#39; | NativeAOT 11.0 | 25000  | 1,405.1 μs |  31.97 μs |  92.25 μs | 1,391.2 μs |  1.00 |    0.00 |  600064 B |       1.000 |
| &#39;Delete CacheSystem&#39;  | NativeAOT 11.0 | 25000  |   693.8 μs |  46.21 μs | 136.24 μs |   628.4 μs |  0.50 |    0.10 |      64 B |       0.000 |
|                       |                |        |            |           |           |            |       |         |           |             |
| **&#39;Delete IMemoryCache&#39;** | **.NET 11.0**      | **50000**  | **2,902.4 μs** |  **95.68 μs** | **266.71 μs** | **2,859.8 μs** |  **1.00** |    **0.00** | **1200000 B** |        **1.00** |
| &#39;Delete CacheSystem&#39;  | .NET 11.0      | 50000  | 1,166.1 μs |  23.22 μs |  41.28 μs | 1,161.2 μs |  0.40 |    0.04 |         - |        0.00 |
|                       |                |        |            |           |           |            |       |         |           |             |
| &#39;Delete IMemoryCache&#39; | NativeAOT 11.0 | 50000  | 3,153.7 μs |  74.47 μs | 211.25 μs | 3,110.9 μs |  1.00 |    0.00 | 1200064 B |       1.000 |
| &#39;Delete CacheSystem&#39;  | NativeAOT 11.0 | 50000  | 1,519.1 μs |  38.13 μs | 104.37 μs | 1,490.6 μs |  0.48 |    0.05 |      64 B |       0.000 |
|                       |                |        |            |           |           |            |       |         |           |             |
| **&#39;Delete IMemoryCache&#39;** | **.NET 11.0**      | **100000** | **8,773.0 μs** | **241.51 μs** | **689.04 μs** | **8,683.0 μs** |  **1.00** |    **0.00** | **2400000 B** |        **1.00** |
| &#39;Delete CacheSystem&#39;  | .NET 11.0      | 100000 | 2,453.4 μs |  48.63 μs |  59.72 μs | 2,436.2 μs |  0.28 |    0.02 |         - |        0.00 |
|                       |                |        |            |           |           |            |       |         |           |             |
| &#39;Delete IMemoryCache&#39; | NativeAOT 11.0 | 100000 | 6,601.5 μs | 131.59 μs | 188.73 μs | 6,550.9 μs |  1.00 |    0.00 | 2400064 B |       1.000 |
| &#39;Delete CacheSystem&#39;  | NativeAOT 11.0 | 100000 | 3,032.1 μs |  59.15 μs |  52.44 μs | 3,014.3 μs |  0.46 |    0.01 |      64 B |       0.000 |


[Code](https://github.com/SoftStoneDevelop/L1CacheSystem/blob/docs/benchmarks/Src/Benchmarks/Cases/Entity/DeleteMemoryCacheBench.cs)

<h1 align="center">
  <a>UpdateMemoryCacheBench</a>
</h1>

| Method                | Runtime        | Size   | Mean        | Error     | StdDev      | Ratio | RatioSD | Gen0      | Gen1      | Allocated | Alloc Ratio |
|---------------------- |--------------- |------- |------------:|----------:|------------:|------:|--------:|----------:|----------:|----------:|------------:|
| **&#39;Update IMemoryCache&#39;** | **.NET 11.0**      | **10000**  |    **997.4 μs** |  **11.58 μs** |     **9.67 μs** |  **1.00** |    **0.00** |         **-** |         **-** |    **3.4 MB** |        **1.00** |
| &#39;Update CacheSystem&#39;  | .NET 11.0      | 10000  |    683.9 μs |  13.42 μs |    30.83 μs |  0.69 |    0.03 |         - |         - |   1.75 MB |        0.52 |
|                       |                |        |             |           |             |       |         |           |           |           |             |
| &#39;Update IMemoryCache&#39; | NativeAOT 11.0 | 10000  |  1,136.3 μs |  30.09 μs |    81.35 μs |  1.00 |    0.00 |         - |         - |    3.4 MB |        1.00 |
| &#39;Update CacheSystem&#39;  | NativeAOT 11.0 | 10000  |    687.5 μs |  13.57 μs |    31.71 μs |  0.61 |    0.05 |         - |         - |   1.75 MB |        0.52 |
|                       |                |        |             |           |             |       |         |           |           |           |             |
| **&#39;Update IMemoryCache&#39;** | **.NET 11.0**      | **25000**  |  **2,599.9 μs** |  **72.44 μs** |   **205.50 μs** |  **1.00** |    **0.00** |         **-** |         **-** |   **8.11 MB** |        **1.00** |
| &#39;Update CacheSystem&#39;  | .NET 11.0      | 25000  |  1,656.5 μs |  27.04 μs |    22.58 μs |  0.64 |    0.05 |         - |         - |   4.39 MB |        0.54 |
|                       |                |        |             |           |             |       |         |           |           |           |             |
| &#39;Update IMemoryCache&#39; | NativeAOT 11.0 | 25000  |  2,897.3 μs |  57.48 μs |   124.96 μs |  1.00 |    0.00 |         - |         - |   8.11 MB |        1.00 |
| &#39;Update CacheSystem&#39;  | NativeAOT 11.0 | 25000  |  1,979.7 μs |  39.42 μs |    88.99 μs |  0.68 |    0.04 |         - |         - |   4.39 MB |        0.54 |
|                       |                |        |             |           |             |       |         |           |           |           |             |
| **&#39;Update IMemoryCache&#39;** | **.NET 11.0**      | **50000**  |  **9,618.0 μs** | **191.80 μs** |   **374.09 μs** |  **1.00** |    **0.00** | **1000.0000** |         **-** |  **16.44 MB** |        **1.00** |
| &#39;Update CacheSystem&#39;  | .NET 11.0      | 50000  | 10,034.7 μs | 478.26 μs | 1,410.15 μs |  1.04 |    0.15 | 1000.0000 |         - |   8.77 MB |        0.53 |
|                       |                |        |             |           |             |       |         |           |           |           |             |
| &#39;Update IMemoryCache&#39; | NativeAOT 11.0 | 50000  | 10,998.2 μs | 218.55 μs |   291.76 μs |  1.00 |    0.00 | 1000.0000 |         - |  16.44 MB |        1.00 |
| &#39;Update CacheSystem&#39;  | NativeAOT 11.0 | 50000  | 10,280.2 μs | 323.27 μs |   948.08 μs |  0.94 |    0.09 | 1000.0000 |         - |   8.77 MB |        0.53 |
|                       |                |        |             |           |             |       |         |           |           |           |             |
| **&#39;Update IMemoryCache&#39;** | **.NET 11.0**      | **100000** | **29,731.6 μs** | **807.97 μs** | **2,292.08 μs** |  **1.00** |    **0.00** | **3000.0000** | **2000.0000** |  **33.18 MB** |        **1.00** |
| &#39;Update CacheSystem&#39;  | .NET 11.0      | 100000 | 21,749.2 μs | 855.22 μs | 2,494.72 μs |  0.74 |    0.10 | 2000.0000 | 1000.0000 |  17.55 MB |        0.53 |
|                       |                |        |             |           |             |       |         |           |           |           |             |
| &#39;Update IMemoryCache&#39; | NativeAOT 11.0 | 100000 | 33,390.3 μs | 665.49 μs | 1,644.93 μs |  1.00 |    0.00 | 3000.0000 | 2000.0000 |  33.18 MB |        1.00 |
| &#39;Update CacheSystem&#39;  | NativeAOT 11.0 | 100000 | 23,309.1 μs | 685.61 μs | 1,967.15 μs |  0.70 |    0.07 | 2000.0000 | 1000.0000 |  17.55 MB |        0.53 |


[Code](https://github.com/SoftStoneDevelop/L1CacheSystem/blob/docs/benchmarks/Src/Benchmarks/Cases/Entity/UpdateMemoryCacheBench.cs)

<h1 align="center">
  <a>GetIntColumnBench</a>
</h1>

[Code](https://github.com/SoftStoneDevelop/L1CacheSystem/blob/docs/benchmarks/Src/Benchmarks/Cases/Entity/GetIntColumnBench.cs)

<h1 align="center">
  <a>GetViewBench</a>
</h1>

[Code](https://github.com/SoftStoneDevelop/L1CacheSystem/blob/docs/benchmarks/Src/Benchmarks/Cases/Entity/GetViewBench.cs)

<h1 align="center">
  <a>SetViewBench</a>
</h1>

[Code](https://github.com/SoftStoneDevelop/L1CacheSystem/blob/docs/benchmarks/Src/Benchmarks/Cases/Entity/SetViewBench.cs)

<h1 align="center">
  <a>UpdateColumnBench</a>
</h1>

[Code](https://github.com/SoftStoneDevelop/L1CacheSystem/blob/docs/benchmarks/Src/Benchmarks/Cases/Entity/UpdateColumnBench.cs)

<h1 align="center">
  <a>ViewInContextBench</a>
</h1>

[Code](https://github.com/SoftStoneDevelop/L1CacheSystem/blob/docs/benchmarks/Src/Benchmarks/Cases/Entity/ViewInContextBench.cs)
