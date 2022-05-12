using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Sample
{
    static class Utility
    {
        public static void SampleBegin([CallerMemberName] string sectionName = "")
        {
            Console.WriteLine("****************************************");
            Console.WriteLine(sectionName);
            Console.WriteLine("****************************************");
        }

        public static void SampleEnd([CallerMemberName] string sectionName = "")
        {
            Console.WriteLine();
            Console.WriteLine();
        }

        public static void Invoke(Action<ILoggerFactory> sampleProc, ILoggerFactory baseFactory)
        {
            Utility.SampleBegin(sampleProc.Method.Name);

            sampleProc(baseFactory);

            Utility.SampleEnd(sampleProc.Method.Name);
        }
    }
}
