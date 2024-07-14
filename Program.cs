using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RPN
{
    class Program
    {
        static void Main(string[] args)
        {

            RPNCalculator calculator = new RPNCalculator();

            //If command line args have been entered then just run the args
            if (args.Length > 0)
            {
                foreach(string arg in args)
                {
                    //Write output to console, debatable whether we want error/warning/success messages to be handles in the calcualtor or handled here 
                    Console.WriteLine(calculator.CalculateRPN(arg, false));
                }
            }else 
            {
                //Run a quick user interface to enter and test data
                Console.WriteLine("Please provide index of RPN as a command line parameter");
                bool exit = false;
                while (!exit)
                {
                    Console.WriteLine("Enter number between 1 and 2209 to start");
                    Console.WriteLine("Enter 'q' to quit");
                    string input = Console.ReadLine();
                    switch (input)
                    {
                        case "q":
                            exit = true;
                            break;
                        default:
                            Console.WriteLine(calculator.CalculateRPN(input, false));
                            break;
                    }
                }
            }
        }
    }
}
