using System;
using System.ComponentModel; 
using System.Runtime.InteropServices;

    public class QueryPerfCounter 
    { 
        [DllImport("KERNEL32")] 
        private static extern bool QueryPerformanceCounter( 
            out long lpPerformanceCount); 

        [DllImport("Kernel32.dll")] 
        private static extern bool QueryPerformanceFrequency(out long lpFrequency); 
        
        private long check;
        private long start; 
        private long stop; 
        private long frequency; 
        Decimal multiplier = new Decimal(1.0e9); 

        public QueryPerfCounter() 
        { 
            if (QueryPerformanceFrequency(out frequency) == false) 
            { 
                // Frequency not supported 
                throw new Win32Exception(); 
            }
 
            check = 0;

            QueryPerformanceCounter(out start);
            QueryPerformanceCounter(out stop);

            QueryPerformanceCounter(out start);
            QueryPerformanceCounter(out stop);
            check+=stop-start;
        } 

        public void Start() 
        { 
            QueryPerformanceCounter(out start); 
        } 

        public void Stop() 
        { 
            QueryPerformanceCounter(out stop); 
        } 

        public double Duration(int iterations) 
        { 
            return ((((double)(stop - start-check)* (double) multiplier) / (double) frequency)/iterations); 
        } 
    }

