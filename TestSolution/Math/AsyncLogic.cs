using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Math
{
    public class AsyncLogic
    {
        public Task<int> DoSomethingAsync(int a,int b)
        {

            return Task.Factory.StartNew(() =>
            {
                System.Threading.Thread.Sleep(2000);
                return a + b;
            });
        }
    }
}
