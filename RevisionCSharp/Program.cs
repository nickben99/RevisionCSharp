using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

// see http://stackoverflow.com/questions/1559293/c-sharp-monitor-wait-pulse-pulseall
namespace RevisionCSharp
{    
    class Data
    {
        public List<int> values;
        private int maxLen; 
        public Data(int len)
        {
            values = new List<int>();
            maxLen = len;
        }

        public void Pop(out int val)
        {
            while (true)
            {
                lock (this)
                {
                    if (values.Count <= 0) // if values are empty meaning there is nothing to pop
                    {
                        Monitor.Wait(this);
                    }
                    val = values[0];
                    values.RemoveAt(0);
                    Monitor.PulseAll(this);
                    return;                    
                }
            }
        }

        public void Add(int val)
        {
            while (true)
            {
                lock (this)
                {
                    if (values.Count>= maxLen) // if values are full meaning there is no room to add anything new
                    {
                        Monitor.Wait(this);
                    }
                    values.Add(val);
                    Monitor.PulseAll(this);
                    return;
                }
            }
        }
    }

    class Producer
    {
        private Data dataRef;
        Random random;
        public Producer(Data data)
        {
            dataRef = data;
            random = new Random();
        }

        public void Tick()
        {
            while (true)
            {
                int randomNumber = random.Next(0, 100);
                dataRef.Add(randomNumber);
            }
        }
    };

    class Consumer
    {
        private Data dataRef;
        public Consumer(Data data)
        {
            dataRef = data;
        }

        public void Tick()
        {
            while (true)
            {
                int val = 0;
                dataRef.Pop(out val);
                Console.WriteLine(val);                
            }
        }
    };

    class Program
    {
        static void Main(string[] args)
        {
            int maxDataSize = 20;

            Data theData = new Data(maxDataSize);            

            Producer prod = new Producer(theData);
            Consumer cons = new Consumer(theData);

            Thread prodThread = new Thread(prod.Tick);
            Thread consThread = new Thread(cons.Tick);

            prodThread.Start();
            consThread.Start();            
        }
    }
}
