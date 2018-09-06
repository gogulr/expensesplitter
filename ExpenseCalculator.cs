using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ExpenseSplitter
{
    class ExpenseCalculator
    {
        static void Main(string[] args)
        {
            var ledger = ExpenseParser.ParseInput("input.txt");

            foreach (var expense in ledger.Expenses)
            {
                var shareAmount = expense.SpentAmount / (expense.SpentFor.Count + 1);
                foreach (var person in expense.SpentFor)
                {
                    person.OwesTo(expense.SpentBy.Name, shareAmount);
                }
            }

            foreach (var person in ledger.People)
            {
                if (person.Owes.Count > 0)
                {
                    List<string> keys = new List<string>(person.Owes.Keys);
                    foreach (var key in keys)
                    {
                        var x = ledger.GetPerson(key);
                        if (x.Owes.ContainsKey(person.Name) && x.Owes[person.Name] > 0)
                        {
                            if(x.Owes[person.Name] > person.Owes[key])
                            {
                                x.Owes[person.Name] -= person.Owes[key];
                                person.Owes[key] = 0;
                            }
                            else
                            {
                                person.Owes[key] -= x.Owes[person.Name];
                                x.Owes[person.Name] = 0;
                            }
                        }

                        if (person.Owes[key] > 0)
                        {
                            Console.WriteLine($"{person.Name} has to give {person.Owes[key]} to {key}");
                        }
                    }
                }
            }

            Console.ReadKey();
        }

        public static class ExpenseParser
        {
            public static Ledger ParseInput(string fileName)
            {
                var ledger = new Ledger();

                using (var streamReader = new StreamReader(Path.GetFullPath(fileName)))
                {
                    while (streamReader.Peek() > 0)
                    {
                        var line = streamReader.ReadLine();
                        var split = line.Trim().Split(' ');
                        var expense = new Expense();
                        ledger.Expenses.Add(expense);
                        expense.SpentAmount = Convert.ToInt32(split[2]);
                        expense.ExpenseType = split[5];

                        expense.SpentBy = ledger.GetPerson(split[0]);

                        for (var i = 7; i < split.Length; i++)
                        {
                            if (split[i] == "and") continue;
                            var person = ledger.GetPerson(split[i][0].ToString());
                            expense.SpentFor.Add(person);
                        }
                    }

                    return ledger;
                }
            }
        }

        public class Ledger
        {
            public Ledger()
            {
                Expenses = new List<Expense>();
                People = new List<Person>();
            }

            public List<Person> People { get; set; }

            public List<Expense> Expenses { get; set; }

            public Person GetPerson(string personName)
            {
                var person = People.FirstOrDefault(x => x.Name == personName);
                if (person == null)
                {
                    person = new Person(personName);
                    People.Add(person);
                }
                return person;
            }
        }

        public class Expense
        {
            public Expense()
            {
                SpentFor = new List<Person>();
            }

            public Person SpentBy { get; set; }

            public int SpentAmount { get; set; }

            public List<Person> SpentFor { get; set; }

            public string ExpenseType { get; set; }
        }

        public class Person
        {
            public Person(string name)
            {
                Name = name;
                Owes = new Dictionary<string, int>();
            }

            public string Name { get; private set; }

            public Dictionary<string, int> Owes { get; set; }

            public void OwesTo(string personName, int amount)
            {
                if (Owes.ContainsKey(personName))
                {
                    Owes[personName] += amount;
                }
                Owes.Add(personName, amount);
            }
        }
    }
}
