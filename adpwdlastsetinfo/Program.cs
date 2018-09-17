using System;

namespace adpwdlastsetinfo
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1 || args.Length > 2)
            {
                System.Console.WriteLine($"Synopsis: {AppDomain.CurrentDomain.FriendlyName} <pwdLastSet> [expireDays]");
                System.Console.WriteLine($"Description: returns human readable datetime of given pwdLastSet long number and days from last set");
                System.Console.WriteLine($"             if expireDays given exitcode==2 if password expired");
                System.Environment.Exit(1);
            }
            long age = long.Parse(args[0]);
            Console.WriteLine($"pwdLastSet: {DateTime.FromFileTimeUtc(age)}");
            var daysFromLastSet = (int)(DateTime.Now - DateTime.FromFileTimeUtc(age)).TotalDays;
            Console.WriteLine($"daysFromLastSet: {daysFromLastSet}");
            
            if (args.Length == 2)
            {
                int expireDays = int.Parse(args[1]);
                Console.WriteLine($"daysToExpiration: {expireDays - daysFromLastSet}");
                Environment.Exit((daysFromLastSet <= expireDays) ? 0 : 2);
            }            
        }
    }
}
