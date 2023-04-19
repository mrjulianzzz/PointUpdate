using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Data.SqlClient;
using System.IO;
using System.Configuration;

namespace PointUpdate
{
    class Program
    {
        //static int numberOfSeconds = 300;
        static string commandString = "";
        static bool waitForSleep = false;

        static Thread PointUpdateTask;
        static void Main(string[] args)
        {
            Console.Title = "RYL Point Generator | v1.0";

            // Variables

            var SQLConnection = ConfigurationManager.AppSettings["SQLConnection"];
            var numberOfSeconds = ConfigurationManager.AppSettings["numberOfSeconds"];
            var UserDB = ConfigurationManager.AppSettings["UserDB"];
            var LoginedDB = ConfigurationManager.AppSettings["LoginedDB"];
            var Points = ConfigurationManager.AppSettings["Points"];

            //numberOfSeconds = "" + numberOfSeconds + ""; // In Seconds, to get minutes multiply by 60, to get hours multiply by another 60
            commandString = "UPDATE usertbl SET Points = Points + " + Points + " FROM " + LoginedDB + ".dbo.UserLoginInfo b WHERE b.LogoutTime < DATEADD(MINUTE, -1, GETDATE()) AND usertbl.uid = b.UID";

            waitForSleep = false;

            ///////////////////////////////



            PointUpdateTask = new Thread(() => BackgroundRun());
            string command = "";

            ShowCommands();
            while ((command = Console.ReadLine().ToLower()) != "exit")
            {
                Console.WriteLine();
                switch (command)
                {
                    case "start":
                        StartTask();
                        break;

                    case "stop":
                        StopTask();
                        break;

                    case "restart":
                        RestartTask();
                        break;

                    case "status":
                        ShowStatus();
                        break;

                    default:
                        NoCommand();
                        break;

                }

                Console.WriteLine();
                ShowCommands();

            }


            StopTask();


        }


        static void ShowCommands()
        {
            Console.ForegroundColor
            = ConsoleColor.Green;

            Console.WriteLine("Command List =======================");
            Console.WriteLine("start\t\tStart Point Update Task");
            Console.WriteLine("stop\t\tStop Point Update Task");
            Console.WriteLine("status\t\tStatus of the Point Update Task");
            Console.WriteLine("restart\t\tRestart Point Update Task");

            Console.WriteLine("exit\t\tClose Program");

            Console.WriteLine("");

            Console.WriteLine("Potato Software by julianzzz =======");
            Console.WriteLine("");
            Console.Write("Enter command: ");

        }

        static void StartTask()
        {

            if (PointUpdateTask.IsAlive || PointUpdateTask.ThreadState == ThreadState.Running)
            {
                Console.WriteLine("Task already running");
            }
            else
            {
                try
                {
                    PointUpdateTask.Start();
                }
                catch
                {
                    Console.WriteLine("Could not start task");
                }
            }
        }

        static void StopTask()
        {
            if (!PointUpdateTask.IsAlive && PointUpdateTask.ThreadState != ThreadState.Running)
            {
                Console.WriteLine("Task already stopped");
            }
            else
            {
                try
                {
                    PointUpdateTask.Abort();
                }
                catch
                {
                    Console.WriteLine("Could not stop task");
                }
            }
        }

        static void RestartTask()
        {
            StopTask();
            StartTask();

        }

        static void ShowStatus()
        {
            if (PointUpdateTask.IsAlive || PointUpdateTask.ThreadState == ThreadState.Running)
            {
                Console.WriteLine("Task is running");
            }
            else
            {
                Console.WriteLine("Task is stopped");
            }
        }

        static void NoCommand()
        {

            Console.WriteLine("Please select a valid command");
            Console.WriteLine();
        }

        static void BackgroundRun()
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.AppSettings["SQLConnection"]);

            SqlCommand command = new SqlCommand(commandString, con);


            try
            {

                while (true)
                {
                    var Points = ConfigurationManager.AppSettings["Points"];                  

                    Console.WriteLine("Earn " + Points + " ingame AFK Point every 5 minute.(Check your config file)", ConfigurationManager.AppSettings["numberOfSeconds"]);

                    if (waitForSleep == false)
                    {
                        Thread.Sleep(5000); // 300000 reading config file seems buggy , manually change the duration time here =D
                    }



                    con.Open();
                    command.ExecuteNonQuery();
                    con.Close();


                    // Do task                  

                    Console.WriteLine("Insert " + Points + " points success to online player(s) account!");
                    Console.WriteLine();

                    if (waitForSleep) { break; } // exit if the task was in test mode
                }

            }
            catch (Exception ex)
            {
                string log = DateTime.Now.ToString("ddMMYYYY_hh_mm_ss") + "_log.log";
                Console.WriteLine("Error Running Task check log for details. {0}", log);
                try
                {
                    System.IO.File.WriteAllText(log, ex.ToString());

                }
                catch { }
            }
            Console.WriteLine();

        }


    }
}
