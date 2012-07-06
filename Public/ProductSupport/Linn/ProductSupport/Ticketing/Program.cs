namespace Linn.ProductSupport.Ticketing
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.IO;
    using System.Threading;
    using System.Xml;
    using Linn.Tickets.Resources;


    public class Program
    {
        public static void Main(string[] args)
        {
            ListOfTestResource testList = new ListOfTestResource();
            Ticket.CreateTest(testList, 0, "Test1", TestResourceResult.Pass, "Result1", "blah blah blah1");
            Ticket.CreateTest(testList, 1, "Test2", TestResourceResult.Fail, "Result2", "blah blah blah2");
            Ticket.CreateTest(testList, 2, "Test3", TestResourceResult.Warning, "Result3", "blah blah blah3");

            ListOfCategoryResource categoryList = new ListOfCategoryResource();
            var category1 = Ticket.CreateCategory(categoryList, "Audio");
            Ticket.CreateItem(category1, "Item1", "item1-1 info...");
            Ticket.CreateItem(category1, "Item2", "item1-2 info...");
            Ticket.CreateItem(category1, "Item3", "item1-3 info...");
            Ticket.CreateItem(category1, "Item4", "item1-4 info...");

            CategoryResource category2 = Ticket.CreateCategory(categoryList, "Network Connection");
            Ticket.CreateItem(category2, "Item1", "item2-1 info...");
            Ticket.CreateItem(category2, "Item2", "item2-2 info...");
            Ticket.CreateItem(category2, "Item3", "item2-3 info...");
            Ticket.CreateItem(category2, "Item4", "item2-4 info...");


            Ticket ticket = new Ticket("Eamonn", "Brady", "eamonn.brady@linn.co.uk", "01413035414", "UTC", "Any time between 12 and 1pm",
                                        "Win7", "01010101", "Kiko DSM", "3.7.1", "00:26:0f:21:b9:67",
                                        "It's broke!", "1.0.0", "Network Audio", categoryList, testList);


            if (ticket.Valid())
            {
                string response;

                if (ticket.Submit(out response))
                {
                    Console.WriteLine("Ticket posted successfully!");
                }
                else
                {
                    Console.WriteLine("Ticket post FAILED!!!");
                }
            }
            else
            {
                Console.WriteLine("Ticket is INVALID!!!");
                Console.WriteLine("\n\n\n\n" + ticket.ValidationInfo());
            }

            Thread.Sleep(3000);
        }
    }
}

