using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Math
{
    public class MathHelper
    {
        public decimal Divide(decimal a, decimal b)
        {
            if (b == 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            return a / b;
        }

        public void DoSomething(int a)
        {
            if (a < 10)
            {
                int test = 4;

                if (a < 5)
                {
                    test = 4;

                    if (a < 3)
                    {
                        test = 6;
                    }
                }
            }
            else
            {
                int ab = 4;
                if (a > 15)
                {
                    int aa = 4;
                }
                else
                {
                    return;
                }
            }
        }
    }
}
