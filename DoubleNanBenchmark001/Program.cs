using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Drawing;
using System.Runtime.InteropServices;


namespace DoubleNanBenchmark001
{

    /// <summary>
    /// .NET 8
    /// </summary>
    internal class Program
    {
       
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<NanTest>();
        }

       
    }

    public class NanTest
    {
        private static double _notNaN = 0.0;
        private static double _NaN = double.NaN;
        [Benchmark(Description = "WPFDoubleUtil.IsNaN")]
        public void CustomNanCheck()
        {
            var b1 = WPFDoubleUtil.IsNaN(_NaN);
            var b2 = WPFDoubleUtil.IsNaN(_notNaN);
        }

        [Benchmark(Description = "System.Double.IsNaN")]
        public void SystemNanCheck()
        {
            var b1 = double.IsNaN(_NaN);
            var b2 = double.IsNaN(_notNaN);
        }
    }



    internal static class WPFDoubleUtil
    {
        // Const values come from sdk\inc\crt\float.h
        internal const double DBL_EPSILON = 2.2204460492503131e-016; /* smallest such that 1.0+DBL_EPSILON != 1.0 */
        internal const float FLT_MIN = 1.175494351e-38F; /* Number close to zero, where float.MinValue is -float.MaxValue */


#if !PBTCOMPILER

        [StructLayout(LayoutKind.Explicit)]
        private struct NanUnion
        {
            [FieldOffset(0)] internal double DoubleValue;
            [FieldOffset(0)] internal UInt64 UintValue;
        }

        // The standard CLR double.IsNaN() function is approximately 100 times slower than our own wrapper,
        // so please make sure to use WPFDoubleUtil.IsNaN() in performance sensitive code.
        // PS item that tracks the CLR improvement is DevDiv Schedule : 26916.
        // IEEE 754 : If the argument is any value in the range 0x7ff0000000000001L through 0x7fffffffffffffffL 
        // or in the range 0xfff0000000000001L through 0xffffffffffffffffL, the result will be NaN.         
        public static bool IsNaN(double value)
        {
            NanUnion t = new NanUnion();
            t.DoubleValue = value;

            UInt64 exp = t.UintValue & 0xfff0000000000000;
            UInt64 man = t.UintValue & 0x000fffffffffffff;

            return (exp == 0x7ff0000000000000 || exp == 0xfff0000000000000) && (man != 0);
        }
#endif
    }
}
