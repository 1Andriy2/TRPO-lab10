using System.Diagnostics;

const int n = 16;
double speed, efficiency; 
double[] a = new double[n];
double[] b = new double[n];
double[] res = new double[n];
double[] sums = new double[4];
Thread[] threads = new Thread[4];
double temptime = 0.0, consistenttime;
Stopwatch stopwatchall = new();
stopwatchall.Start();
Console.WriteLine("---Modification cascad scheme---");
Stopwatch stopwatch = new();
stopwatch.Start();
for (int i = 0; i < threads.Length; i++)
{
    int start = i * (n / threads.Length) + 1;
    int end = (i + 1) * (n / threads.Length);
    int index = i;
    threads[i] = new Thread(() => CascadProduct(start, end, res, ref sums[index]));
    threads[i].Start();
}

for (int i = 0; i < threads.Length; i++)
{
    threads[i].Join();
}
stopwatch.Stop();
Console.WriteLine("Paralel part of modification cascad scheme: {0}", stopwatch.Elapsed.TotalMilliseconds);
temptime += stopwatch.Elapsed.TotalMilliseconds;
stopwatch = new();
stopwatch.Start();
double suma = 0;
for (int i = 0; i < sums.Length; i++)
{
    suma += sums[i];
}
stopwatch.Stop();
Console.WriteLine("Сonsistent part of modification cascad scheme: {0}", stopwatch.Elapsed.TotalMilliseconds);
Console.WriteLine("Scalar product: {0}", suma);
speed = (n - 1) * 1.0 / (2 * Math.Log2(n));
efficiency = (n - 1) * 1.0 / (2*n);
Console.WriteLine("Speed cascad scheme: {0}", speed);
Console.WriteLine("Efficiency cascad scheme: {0}", efficiency);
Console.WriteLine("");

Console.WriteLine("---Parallel Linear Reduction 1st of order---");
Console.WriteLine("Product reduction: {0}", ParallelLinearReduction());

speed = (2 * n) / (3 * Math.Log2(n));
efficiency = 2.0 / (3 * Math.Log2(n));
Console.WriteLine("Speed parallel linear reduction: {0}", speed);
Console.WriteLine("Efficiency parallel linear reduction: {0}", efficiency);
Console.WriteLine("");
stopwatchall.Stop();

Console.WriteLine("---Other assessments---");
consistenttime = 1 - (temptime / stopwatchall.Elapsed.TotalMilliseconds);
Console.WriteLine("Estimation of maximum speed based on Amdahl's law: {0}", AmdahlSpeed(n, consistenttime));
Console.WriteLine("Estimation of maximum speed based on Gustavson-Barsis law: {0}", GustafsonSpeed(n, consistenttime));

void ExampleProduct(int start, int end, double[] res)
{
    for (int i = start; i <= end; i++)
    {
        res[i - 1] = Math.Pow(-1, (i + 1)) * (i + 1) / (1.25 * Math.Pow(i, 2.5)) * (Math.Cos(Math.Pow(3, (-10 * i))) - (Math.Pow(2, (i + 2)) / Factorial(i + 2)));
    }
}
void CascadProduct(int start, int end, double[] res, ref double sum)
{
    ExampleProduct(start, end, res);
    for (int i = start; i <= end; i++)
    {
        sum += res[i - 1];
    }
}
int Factorial(int x)
{
    if(x == 0) return 1;
    if(x == 1) return 1;
    return x * Factorial(x-1);
}

double ParallelLinearReduction()
{
    int numOfThreads = Environment.ProcessorCount;
    int size = a.Length / numOfThreads;
    double[] results = new double[numOfThreads];

    Stopwatch stopwatch = new();
    stopwatch.Start();

    Parallel.For(0, numOfThreads, i =>
    {
        int startIndex = i * size + 1;
        int endIndex = i == numOfThreads - 1 ? a.Length : (i + 1) * size + 1;

        double sum = 0;
        double[] temp = new double[n];
        ExampleProduct(startIndex, endIndex, temp);
        for (int j = startIndex; j < endIndex; j++)
        {
            sum += temp[j];
        }
        results[i] = sum;
    });
    stopwatch.Stop();
    Console.WriteLine("Reduction of the 1st order is a parallel implementation: {0}", stopwatch.Elapsed.TotalMilliseconds);
    temptime += stopwatch.Elapsed.TotalMilliseconds;

    stopwatch = new();
    stopwatch.Start();
    double res = 0;
    for (int i = 0; i < numOfThreads; i++)
    {
        res += results[i];
    }
    stopwatch.Stop();
    Console.WriteLine("Reduction of the 1st order is a sequential implementation: {0}", stopwatch.Elapsed.TotalMilliseconds);
    return res;
}

double AmdahlSpeed(double n, double p)
{
    return 1 / ((1 - p) / n + p );
}


double GustafsonSpeed(double p, double g)
{
    return p + (1 - p) * g;
}