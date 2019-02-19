using System;
using System.IO;
using System.Threading.Tasks;

namespace pizzaSolution
{
    class Program
    {
        
        static void Main(string[] args)
        {
            new PizzaProblem ().Main ().GetAwaiter ().GetResult ();
        }
    }
}
