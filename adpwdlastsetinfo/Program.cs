using System;

namespace adpwdlastsetinfo
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1 || args.Length > 3)
            {
                System.Console.WriteLine($"Synopsis: {AppDomain.CurrentDomain.FriendlyName} <pwdLastSet> [expireDays] [alertDays]");
                System.Console.WriteLine($"Description: returns human readable datetime of given pwdLastSet long number and days from last set");
                System.Console.WriteLine($"             if expireDays given exitcode==2 if password expired");
                System.Console.WriteLine($"             if alertDays given and password not yet expired but alertDays left before expiration exitcode==3");
                System.Environment.Exit(1);
            }
            long age = long.Parse(args[0]);
            Console.WriteLine($"pwdLastSet: {DateTime.FromFileTimeUtc(age)}");
            var daysFromLastSet = (int)(DateTime.Now - DateTime.FromFileTimeUtc(age)).TotalDays;
            Console.WriteLine($"daysFromLastSet: {daysFromLastSet}");

            if (args.Length >= 2)
            {
                int expireDays = int.Parse(args[1]);
                Console.WriteLine($"daysToExpiration: {expireDays - daysFromLastSet}");
                var expired = daysFromLastSet > expireDays;
                if (expired) Environment.Exit(2);                

                if (args.Length == 3)
                {
                    int alertDays = int.Parse(args[2]);
                    if (expireDays - daysFromLastSet <= alertDays) Environment.Exit(3);
                }
            }
        }
    }
}
