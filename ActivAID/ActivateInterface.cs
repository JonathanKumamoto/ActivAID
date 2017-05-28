using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


/*******************************************************************
 * Project:     Command Line Interface, ActivATE
 * File Name:   ActivateInterface.cs
 * Purpose:     Manages the interface between the command line and ActivATE
 * 
 * Revision History:    
 *  July 1st, 2015 by Jess Gillespie
 *  - Created   
 * 
 * August 19, 2015 by S. Moran v1.0.3.0
 *  - Removed ricData reference and replaced all "ricData." with  "clData.lotData."
 * 
 * November 24, 2015 by J. Gillespie v1.0.5.0
 *  - Added Simulation control function
 *
 *******************************************************************/

using System.Diagnostics;
using Racal.SystemController;
using Racal.Interfaces;
using System.IO;
using System.Threading;

namespace ActivAID
{
    [Serializable]
    class ActivateInterface : MarshalByRefObject
    {
        public SystemControl _systemControl;
        public CommandLineData clData = new CommandLineData();
        private const string ACTIVATE_CONTROL_URL = "tcp://localhost:8095/ActivATEControl";

        public List<string> CurrentLog = new List<string>();
        public List<string> DevToSim = new List<string>();


        public string file;

        public bool exitCL = false;

        /// <summary>
        /// Initialize handles to ActivATE
        /// </summary>
        /// <returns></returns>

        public bool Init()
        {
            bool retVal = true;
            try
            {
                _systemControl = Activator.GetObject(typeof(SystemControl), ACTIVATE_CONTROL_URL) as SystemControl;
                //_systemControl.CommandLineMsg += new CommandLineMsgHandler(_systemControl_CommandLineMsg);
                _systemControl.InitCLComponents();

            }
            catch (Exception e)
            {
                
                retVal = false;
            }
            Console.WriteLine("\n\nHERE\n\n");
            return retVal;
        }


        ///// <summary>
        ///// ActivATE Event Resposne
        ///// </summary>
        ///// <param name="SlotID"></param>
        ///// <param name="fn"></param>
        ///// <returns></returns>
        //public void _systemControl_CommandLineMsg(CommandLineMsgData data)
        //{
        //    try
        //    {
        //        // Add  time-stamp
        //        DateTime dt = DateTime.Now;
        //        string sDt = string.Format("{0:00}:{1:00}:{2:00}.{3:000}", dt.Hour, dt.Minute, dt.Second, dt.Millisecond);
        //        Console.WriteLine(sDt + data.msg);

        //        CurrentLog.Add(sDt + data.msg);

        //        if (data.msg.Contains("Finished"))
        //            exitCL = true;


        //    }
        //    catch (Exception e)
        //    {
        //        //Console.WriteLine("Error in Activate Response: " + e.Message);                
        //    }
        //}


        public void LaunchEditUsers()
        {
            Thread mythread = new Thread(EUThread);
            mythread.Start();
        }

        public void LaunchChangeUser()
        {
            _systemControl.CLChangeUser();
        }
        public void LaunchNewTP()
        {
            _systemControl.CLNewTestProgram();
        }
        public void EUThread(object obj)
        {
            _systemControl.CLEditUsers();
        }

    }
}
