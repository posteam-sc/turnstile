/**
* WGController32 2015-04-30 17:40:43 karl CSN Chen Shaoning $
*
* Access Controller Short Message Protocol Test Case
* V2.6 version 2015-11-03 20:25:53 V6.60 driver version Communication password test.
* Modify the retry operation of the communication
* V2.5 version 2015-04-29 20:41:30 Adopt V6.56 driver version Model changed from 0x19 to 0x17
* Basic function: Query controller status
* Read date and time
* Set date and time
* Get the record with the specified index number
* Set the record index number that has been read
* Get the record index number that has been read
* Open the door remotely
* Add or modify permissions
* Privilege deletion (single deletion)
* Permission clear (all cleared)
* Total number of permissions read
* Permission query
* Set the door control parameters (online / delay)
* Read door control parameters (online / delay)

* Set the IP and port of the receiving server
* Read the IP and port of the receiving server
*
*
* Receive server implementation (receive data on port 61005) -- This feature must pay attention to the firewall settings must be allowed to receive data.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace ZTS.Forms
{
    public partial class FunctionsTest : Form
    {
        /// <summary>
        /// 
        /// </summary>
        public FunctionsTest()
        {
            InitializeComponent();
        }

        Boolean bStopWatchServer = false; //2015-05-05 17:35:07 Stop receiving the server
        Boolean bStopBasicFunction = false; //2015-06-10 09:04:52 Basic test
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            bStopWatchServer = true;
            bStopBasicFunction = true;  //2015-06-10 09:04:52 Basic test
        }


        private void button1_Click(object sender, EventArgs e)
        {
            this.txtInfo.Text = "";

            //停止接收服务器标识 / / Stop receiving server ID
            bStopWatchServer = true;
            bStopBasicFunction = false;  //2015-06-10 09:04:52 Basic test

            //' 'This case is not for search controller and IP setting work (directly by IP setting tool)
            //' 'Test description in this case
            //' 'Controller SN = 229999901
            //' 'Controller IP = 192.168.168.123
            //' 'Computer IP = 192.168.168.101
            //' 'Used as the receiving server IP (this computer IP 192.168.168.101), receiving server port (61005)

            // Basic function test
            //txtSN.Text controller 9-digit sequence SN
            //txtIP.Text Controller IP address, default is 192.168.168.123 [Can use Controller to modify controller IP]
            testBasicFunction(txtIP.Text, long.Parse(txtSN.Text));


            //txtWatchServerIP.Text receives the IP of the server. The default is computer IP 192.168.168.101 [You can also use Search Controller to modify the settings]
            //txtWatchServerPort.Text Receive server PORT, default 61005
            testWatchingServer(txtIP.Text, long.Parse(txtSN.Text), txtWatchServerIP.Text, int.Parse(this.txtWatchServerPort.Text)); //Receive server settings

            bStopWatchServer = false;
            WatchingServerRuning(txtWatchServerIP.Text, int.Parse(this.txtWatchServerPort.Text)); //Server is running....
            bStopWatchServer = true;
        }


        private void button2_Click(object sender, EventArgs e)
        {
            bStopWatchServer = true;
            bStopBasicFunction = true;  //2015-06-10 09:04:52 Basic test
        }

        private void button3_Click(object sender, EventArgs e) //2015-05-05 17:35:35 Search controller
        {
            try
            {
                ProcessStartInfo pInfo = new ProcessStartInfo();
                pInfo.FileName = Environment.CurrentDirectory + "\\IPCon2015_V2.17.exe";
                pInfo.UseShellExecute = true;
                Process p = Process.Start(pInfo);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                MessageBox.Show(ex.ToString());

            }
        }
        private void button4_Click(object sender, EventArgs e)
        {
            bStopWatchServer = false;
            WatchingServerRuning(txtWatchServerIP.Text, int.Parse(this.txtWatchServerPort.Text)); //Server is running....
            bStopWatchServer = true;
        }

        /// <summary>
        // / short report
        /// </summary>
        class WGPacketShort
        {
            public const int WGPacketSize = 64;			   //Message length
            //2015-04-29 22:22:41 const static unsigned char	 Type = 0x19;					//Types of
            public const int Type = 0x17;		//2015-04-29 22:22:50			//Types of
            public const int ControllerPort = 60000;        // controller port
            public const long SpecialFlag = 0x55AAAA55;     // Special logo to prevent misuse

            public int functionID;		                   // function number
            public long iDevSn;                             //Device serial number 4 bytes, 9 digits
            public string IP;                                // controller's IP address

            public byte[] data = new byte[56];               //56 bytes of data [including serial number]
            public byte[] recv = new byte[WGPacketSize];    // Received data

            public WGPacketShort()
            {
                Reset();
            }
            public void Reset()  // Data reset
            {
                for (int i = 0; i < 56; i++)
                {
                    data[i] = 0;
                }
            }
            static long sequenceId;     //serial number	
            public byte[] toByte() // Generate a 64-byte instruction package
            {
                byte[] buff = new byte[WGPacketSize];
                sequenceId++;

                buff[0] = (byte)Type;
                buff[1] = (byte)functionID;
                Array.Copy(System.BitConverter.GetBytes(iDevSn), 0, buff, 4, 4);
                Array.Copy(data, 0, buff, 8, data.Length);
                Array.Copy(System.BitConverter.GetBytes(sequenceId), 0, buff, 40, 4);
                return buff;
            }

            WG3000_COMM.Core.wgMjController controller = new WG3000_COMM.Core.wgMjController();
            public int run()  // Send instructions Receive return information
            {
                byte[] buff = toByte();

                int tries = 3;
                int errcnt = 0;
                controller.IP = IP;
                controller.PORT = ControllerPort;
                do
                {
                    if (controller.ShortPacketSend(buff, ref recv) < 0)
                    {
                        //2015-11-03 20:26:52 Enter retry return -1;
                    }
                    else
                    {
                        //serial number
                        long sequenceIdReceived = 0;
                        for (int i = 0; i < 4; i++)
                        {
                            long lng = recv[40 + i];
                            sequenceIdReceived += (lng << (8 * i));
                        }

                        if ((recv[0] == Type)                       // Type consistent
                            && (recv[1] == functionID)              //Consistent function number
                            && (sequenceIdReceived == sequenceId))  //Serial number correspondence
                        {
                            return 1;
                        }
                        else
                        {
                            errcnt++;
                        }
                    }
                } while (tries-- > 0); //Retry three times

                return -1;
            }
            /// <summary>
            /// Last issued serial number
            /// </summary>
            /// <returns></returns>
            public static long sequenceIdSent()// 
            {
                return sequenceId; // Last issued serial number
            }
            /// <summary>
            /// shut down
            /// </summary>
            public void close()
            {
                controller.Dispose();
            }
        }

        void log(string info)  //Log information
        {
            //txtInfo.Text += string.Format("{0}\r\n", info);
            //txtInfo.AppendText(string.Format("{0}\r\n", info));
            txtInfo.AppendText(string.Format("{0} {1}\r\n", DateTime.Now.ToString("HH:mm:ss"), info)); //2015-11-03 20:55:49 display time
            txtInfo.ScrollToCaret();//Scroll to the cursor
            Application.DoEvents();
        }

        /// <summary>
        /// 4 bytes into an integer (before the low, after the high)
        /// </summary>
        /// <param name="buff">Byte array</param>
        /// <param name="start">Start index bit (counting from 0)</param>
        /// <param name="len">length</param>
        /// <returns>Integer</returns>
        long byteToLong(byte[] buff, int start, int len)
        {
            long val = 0;
            for (int i = 0; i < len && i < 4; i++)
            {
                long lng = buff[i + start];
                val += (lng << (8 * i));
            }
            return val;
        }

        /// <summary>
        /// Convert integers to 4-byte arrays
        /// </summary>
        /// <param name="outBytes">Array</param>
        /// <param name="startIndex">Start index bit (counting from 0)</param>
        /// <param name="val">Numerical value</param>
        void LongToBytes(ref byte[] outBytes, int startIndex, long val)
        {
            Array.Copy(System.BitConverter.GetBytes(val), 0, outBytes, startIndex, 4);
        }
        /// <summary>
        ///Get Hex value, mainly used for datetime format
        /// </summary>
        /// <param name="val">Numerical value</param>
        /// <returns>Hex value</returns>
        int GetHex(int val)
        {
            return ((val % 10) + (((val - (val % 10)) / 10) % 10) * 16);
        }

        /// <summary>
        /// Display record information
        /// </summary>
        /// <param name="recv"></param>
        void displayRecordInformation(byte[] recv)
        {
            //8-11 index number of the record
            //(=0 means no record) 4 0x00000000
            int recordIndex = 0;
            recordIndex = (int)byteToLong(recv, 8, 4);

            //12 record type ********************************************* *
            //0=No record
            //1=swipe record
            //2=door magnet, button, device start, remote door open record
            //3=Alarm record 1
            //0xFF= indicates that the record of the specified index bit has been overwritten. Please use index 0 to retrieve the index value of the earliest record.
            int recordType = recv[12];

            //13 validity (0 means no pass, 1 means pass) 1
            int recordValid = recv[13];

            //14 door number (1, 2, 3, 4) 1
            int recordDoorNO = recv[14];

            //15 Entry/Exit (1 means entry, 2 means exit) 1 0x01
            int recordInOrOut = recv[15];

            //16-19 card number (type is when swiping records)
            // or number (other types of records) 4
            long recordCardNO = 0;
            recordCardNO = byteToLong(recv, 16, 4);

            //20-26 Swipe time:
            //year, month, day, hour, minute, and second (using BCD code) see the instructions for setting the time section
            string recordTime = "2000-01-01 00:00:00";
            recordTime = string.Format("{0:X2}{1:X2}-{2:X2}-{3:X2} {4:X2}:{5:X2}:{6:X2}",
                recv[20], recv[21], recv[22], recv[23], recv[24], recv[25], recv[26]);
            //2012.12.11 10:49:59 7
            //27 Record the reason code (you can check the "RessonNO" of the "Swipe Record Description.xls" file)
            // Use complex information to use 1
            int reason = recv[27];


            //0=No record
            //1=swipe record
            //2=door magnet, button, device start, remote door open record
            //3=Alarm record 1
            //0xFF= indicates that the record of the specified index bit has been overwritten. Please use index 0 to retrieve the index value of the earliest record.
            if (recordType == 0)
            {
                log(string.Format("Index bit = {0} no record", recordIndex));
            }
            else if (recordType == 0xff)
            {
                log(" The record of the specified index bit has been overwritten. Please use index 0 to retrieve the index value of the oldest record.");
            }
            else if (recordType == 1) //2015-06-10 08:49:31 Display data with record type as card number
            {
                //card number
                log(string.Format("index bit={0} ", recordIndex));
                log(string.Format(" card number = {0}", recordCardNO));
                log(string.Format(" gate number = {0}", recordDoorNO));
                log(string.Format("In and Out = {0}", recordInOrOut == 1 ? "Into the door" : "Go out"));
                log(string.Format(" Valid = {0}", recordValid == 1 ? "PASS" : "Forbidden"));
                log(string.Format(" time = {0}", recordTime));
                log(string.Format(" Description = {0}", getReasonDetailEnglish(reason)));
            }
            else if (recordType == 2)
            {
                //Other processing
                //door magnetic, button, device startup, remote door opening record
                log(string.Format("index bit={0} non-swipe record", recordIndex));
                log(string.Format(" number = {0}", recordCardNO));
                log(string.Format(" gate number = {0}", recordDoorNO));
                log(string.Format(" time = {0}", recordTime));
                log(string.Format(" Description = {0}", getReasonDetailEnglish(reason)));
            }
            else if (recordType == 3)
            {
                //Other processing
                //alarm record
                log(string.Format("index bit={0} alarm record", recordIndex));
                log(string.Format(" number = {0}", recordCardNO));
                log(string.Format(" gate number = {0}", recordDoorNO));
                log(string.Format(" time = {0}", recordTime));
                log(string.Format(" Description = {0}", getReasonDetailEnglish(reason)));
            }
        }

        string[] RecordDetails =
        {
// Record reason (SwipePass in the type means pass; SwipeNOPass means pass prohibited; ValidEvent valid event (such as button door magnetic super password open door; Warn alarm event)
//Code Type English Description Chinese Description
"1","SwipePass","Swipe","刷卡开门",
"2","SwipePass","Swipe Close","刷卡关",
"3","SwipePass","Swipe Open","刷卡开",
"4","SwipePass","Swipe Limited Times","刷卡开门(带限次)",
"5","SwipeNOPass","Denied Access: PC Control","刷卡禁止通过: 电脑控制",
"6","SwipeNOPass","Denied Access: No PRIVILEGE","刷卡禁止通过: 没有权限",
"7","SwipeNOPass","Denied Access: Wrong PASSWORD","刷卡禁止通过: 密码不对",
"8","SwipeNOPass","Denied Access: AntiBack","刷卡禁止通过: 反潜回",
"9","SwipeNOPass","Denied Access: More Cards","刷卡禁止通过: 多卡",
"10","SwipeNOPass","Denied Access: First Card Open","刷卡禁止通过: 首卡",
"11","SwipeNOPass","Denied Access: Door Set NC","刷卡禁止通过: 门为常闭",
"12","SwipeNOPass","Denied Access: InterLock","刷卡禁止通过: 互锁",
"13","SwipeNOPass","Denied Access: Limited Times","刷卡禁止通过: 受刷卡次数限制",
"14","SwipeNOPass","Denied Access: Limited Person Indoor","刷卡禁止通过: 门内人数限制",
"15","SwipeNOPass","Denied Access: Invalid Timezone","刷卡禁止通过: 卡过期或不在有效时段",
"16","SwipeNOPass","Denied Access: In Order","刷卡禁止通过: 按顺序进出限制",
"17","SwipeNOPass","Denied Access: SWIPE GAP LIMIT","刷卡禁止通过: 刷卡间隔约束",
"18","SwipeNOPass","Denied Access","刷卡禁止通过: 原因不明",
"19","SwipeNOPass","Denied Access: Limited Times","刷卡禁止通过: 刷卡次数限制",
"20","ValidEvent","Push Button","按钮开门",
"21","ValidEvent","Push Button Open","按钮开",
"22","ValidEvent","Push Button Close","按钮关",
"23","ValidEvent","Door Open","门打开[门磁信号]",
"24","ValidEvent","Door Closed","门关闭[门磁信号]",
"25","ValidEvent","Super Password Open Door","超级密码开门",
"26","ValidEvent","Super Password Open","超级密码开",
"27","ValidEvent","Super Password Close","超级密码关",
"28","Warn","Controller Power On","控制器上电",
"29","Warn","Controller Reset","控制器复位",
"30","Warn","Push Button Invalid: Disable","按钮不开门: 按钮禁用",
"31","Warn","Push Button Invalid: Forced Lock","按钮不开门: 强制关门",
"32","Warn","Push Button Invalid: Not On Line","按钮不开门: 门不在线",
"33","Warn","Push Button Invalid: InterLock","按钮不开门: 互锁",
"34","Warn","Threat","胁迫报警",
"35","Warn","Threat Open","胁迫报警开",
"36","Warn","Threat Close","胁迫报警关",
"37","Warn","Open too long","门长时间未关报警[合法开门后]",
"38","Warn","Forced Open","强行闯入报警",
"39","Warn","Fire","火警",
"40","Warn","Forced Close","强制关门",
"41","Warn","Guard Against Theft","防盗报警",
"42","Warn","7*24Hour Zone","烟雾煤气温度报警",
"43","Warn","Emergency Call","紧急呼救报警",
"44","RemoteOpen","Remote Open Door","操作员远程开门",
"45","RemoteOpen","Remote Open Door By USB Reader","发卡器确定发出的远程开门"
        };

        string getReasonDetailChinese(int Reason) //Chinese
        {
            if (Reason > 45)
            {
                return "";
            }
            if (Reason <= 0)
            {
                return "";
            }
            return RecordDetails[(Reason - 1) * 4 + 3]; //Chinese information
        }

        string getReasonDetailEnglish(int Reason) //English description
        {
            if (Reason > 45)
            {
                return "";
            }
            if (Reason <= 0)
            {
                return "";
            }
            return RecordDetails[(Reason - 1) * 4 + 2]; //English information
        }
        /// <summary>
        /// Basic function test
        /// </summary>
        /// <param name="ControllerIP">Controller IP Address</param>
        /// <param name="controllerSN"> controller serial number </param>
        /// <returns> is less than or equal to 0 failure, 1 means success</returns>
        int testBasicFunction(String ControllerIP, long controllerSN)
        {
            int ret = 0;
            int success = 0;  //0 Failure, 1 means success


            // Create a short message pkt
            WGPacketShort pkt = new WGPacketShort();
            pkt.iDevSn = controllerSN;
            pkt.IP = ControllerIP;

            //1.4 Query controller status [function number: 0x20] (for real-time monitoring) ******************************** **************************************************
            pkt.Reset();
            pkt.functionID = 0x20;
            ret = pkt.run();

            success = 0;
            if (ret == 1)
            {
                //Read information successfully...
                success = 1;
                log("1.4 Query controller status succeeded...");

                // last recorded information
                displayRecordInformation(pkt.recv); //2015-06-09 20:01:21

                //	other information	
                int[] doorStatus = new int[4];
                //28 Door No. 1 (0 means closed, 1 means open) 1 0x00
                doorStatus[1 - 1] = pkt.recv[28];
                //29 No. 2 door magnet (0 means closed, 1 means open) 1 0x00
                doorStatus[2 - 1] = pkt.recv[29];
                //30 No. 3 door magnet (0 means closed, 1 means open) 1 0x00
                doorStatus[3 - 1] = pkt.recv[30];
                //31 No. 4 door magnet (0 means closed, 1 means open) 1 0x00
                doorStatus[4 - 1] = pkt.recv[31];

                int[] pbStatus = new int[4];
                //32 Door 1 button (0 means loose, 1 means pressed) 1 0x00
                pbStatus[1 - 1] = pkt.recv[32];
                //33 Door 2 button (0 means loose, 1 means pressed) 1 0x00
                pbStatus[2 - 1] = pkt.recv[33];
                //34 Gate 3 button (0 means loose, 1 means pressed) 1 0x00
                pbStatus[3 - 1] = pkt.recv[34];
                //35 Gate 4 button (0 means loose, 1 means pressed) 1 0x00
                pbStatus[4 - 1] = pkt.recv[35];

                //36 fault number
                // equals 0 no fault
                //Not equal to 0, there is a fault (reset the time first, if there is still a problem, then return to the factory maintenance) 1
                int errCode = pkt.recv[36];

                //37 controller current time
                //hour 1 0x21
                //38 points 1 0x30
                //39 seconds 1 0x58

                //40-43 serial number 4
                long sequenceId = 0;
                sequenceId = byteToLong(pkt.recv, 40, 4);

                //48
                // Special information 1 (return according to actual use)
                // Keyboard button information 1


                //49 Relay status 1 [0 means the door is locked, 1 means the door is unlocked. When the normal door is locked, the value is 0000]
                int relayStatus = pkt.recv[49];
                if ((relayStatus & 0x1) > 0)
                {
                    //One door unlocked
                }
                else
                {
                    //One door is locked
                }
                if ((relayStatus & 0x2) > 0)
                {
                    //Second door unlocked
                }
                else
                {
                    //Second door locked
                }
                if ((relayStatus & 0x4) > 0)
                {
                    //The third door unlocks
                }
                else
                {
                    //The third door is locked
                }
                if ((relayStatus & 0x8) > 0)
                {
                    //fourth door unlock
                }
                else
                {
                    //The fourth door is locked
                }

                //50 8-15 bit of the magnetic state [fire alarm / forced lock]
                //Bit0 forcibly locks the door
                //Bit1 fire alarm
                int otherInputStatus = pkt.recv[50];
                if ((otherInputStatus & 0x1) > 0)
                {
                    // Forced to lock the door
                }
                if ((otherInputStatus & 0x2) > 0)
                {
                    //Fire alarm
                }

                //51 V5.46 version support controller current year 1 0x13
                //52 ​​V5.46 version support month 1 0x06
                //53 V5.46 version support day 1 0x22

                String controllerTime = "2000-01-01 00:00:00"; //Controller current time
                controllerTime = string.Format("{0:X2}{1:X2}-{2:X2}-{3:X2} {4:X2}:{5:X2}:{6:X2}",
                    0x20, pkt.recv[51], pkt.recv[52], pkt.recv[53], pkt.recv[37], pkt.recv[38], pkt.recv[39]);

            }
            else
            {
                log("1.4 Querying Controller Status Failed?????...");
                return -1;
            }

            //1.5 Read date time (function number: 0x32) ************************************* *********************************************
            pkt.Reset();
            pkt.functionID = 0x32;
            ret = pkt.run();
            success = 0;
            if (ret > 0)
            {

                String controllerTime = "2000-01-01 00:00:00"; //Controller current time
                controllerTime = string.Format("{0:X2}{1:X2}-{2:X2}-{3:X2} {4:X2}:{5:X2}:{6:X2}",
                    pkt.recv[8], pkt.recv[9], pkt.recv[10], pkt.recv[11], pkt.recv[12], pkt.recv[13], pkt.recv[14]);

                log("1.5 read date time success...");
                success = 1;
            }

            //1.6 Set date and time [Function number: 0x30] ************************************** ********************************************
            // Press the current time of the computer to calibrate the controller .....
            pkt.Reset();
            pkt.functionID = 0x30;

            DateTime ptm = DateTime.Now;
            pkt.data[0] = (byte)GetHex((ptm.Year - ptm.Year % 100) / 100);
            pkt.data[1] = (byte)GetHex((int)((ptm.Year) % 100)); //st.GetMonth());
            pkt.data[2] = (byte)GetHex(ptm.Month);
            pkt.data[3] = (byte)GetHex(ptm.Day);
            pkt.data[4] = (byte)GetHex(ptm.Hour);
            pkt.data[5] = (byte)GetHex(ptm.Minute);
            pkt.data[6] = (byte)GetHex(ptm.Second);
            ret = pkt.run();
            success = 0;
            if (ret > 0)
            {
                Boolean bSame = true;
                for (int i = 0; i < 7; i++)
                {
                    if (pkt.data[i] != pkt.recv[8 + i])
                    {
                        bSame = false;
                        break;
                    }
                }
                if (bSame)
                {
                    log("1.6 set date and time success...");
                    success = 1;
                }
            }

            //1.7 Get the record with the specified index number [Function No.: 0xB0] *********************************** ***********************************************
            // (take the record number 0x00000001)
            long recordIndexToGet = 0;
            pkt.Reset();
            pkt.functionID = 0xB0;
            pkt.iDevSn = controllerSN;

            // (special
            // If =0, then retrieve the oldest record information
            // If 0xffffffff then retrieve the information of the last record)
            // Record index number is normally incremented, up to 0xffffff = 16,777,215 (more than 10 million). Due to limited storage space, only the most recent 200,000 records will be kept on the controller. When the index number exceeds 200,000 After that, the records of the old index number bits will be overwritten, so when querying the records of these index numbers, the returned record type will be 0xff, indicating that it does not exist.
            recordIndexToGet = 1;
            LongToBytes(ref pkt.data, 0, recordIndexToGet);

            ret = pkt.run();
            success = 0;
            if (ret > 0)
            {
                log("1.7 Get indexed information for record 1 success...");
                // Index is the information recorded on the 1st
                displayRecordInformation(pkt.recv); //2015-06-09 20:01:21

                success = 1;
            }

            //. Send a message (take the earliest record by index number 0x00000000) [This command is suitable for use in environments with more than 200,000 credit card records]
            pkt.Reset();
            pkt.functionID = 0xB0;
            recordIndexToGet = 0;
            LongToBytes(ref pkt.data, 0, recordIndexToGet);

            ret = pkt.run();
            success = 0;
            if (ret > 0)
            {
                log("1.7 Get the information of the earliest record successfully...");
                // The earliest recorded information
                displayRecordInformation(pkt.recv); //2015-06-09 20:01:21

                success = 1;
            }

            // Send a message (take the latest record through the index 0xffffffff)
            pkt.Reset();
            pkt.functionID = 0xB0;
            recordIndexToGet = 0xffffffff;
            LongToBytes(ref pkt.data, 0, recordIndexToGet);
            ret = pkt.run();
            success = 0;
            if (ret > 0)
            {
                log("1.7 Get the latest record of information successfully...");
                // The latest record information
                displayRecordInformation(pkt.recv); //2015-06-09 20:01:21
                success = 1;
            }

            ////1.8 Set the recorded index number [function number: 0xB2] ****************************** ************************************************** **
            //pkt.Reset();
            //pkt.functionID = 0xB2;
            //// (Set the recorded index number to 5)
            //int recordIndexGot = 0x5;
            //LongToBytes(ref pkt.data, 0, recordIndexGot);

            ////12 logo (to prevent incorrect setting) 1 0x55 [fixed]
            //LongToBytes(ref pkt.data, 4, WGPacketShort.SpecialFlag);

            //ret = pkt.run();
            //success = 0;
            //if (ret > 0)
            //{
            // if (pkt.recv[8] == 1)
            // {
            // log("1.8 sets the record index number that has been read successfully...");
            // success = 1;
            // }
            //}

            ////1.9 Get the index number of the record that has been read [Function No.: 0xB4] ****************************** ************************************************** **
            //pkt.Reset();
            //pkt.functionID = 0xB4;
            //int recordIndexGotToRead = 0x0;
            //ret = pkt.run();
            //success = 0;
            //if (ret > 0)
            //{
            // recordIndexGotToRead = (int)byteToLong(pkt.recv, 8, 4);
            // log("1.9 Get the index number of the record that has been read successfully...");
            // success = 1;
            //}

            ////1.8 Set the recorded index number [function number: 0xB2] ****************************** ************************************************** **
            ////Recover the extracted records, prepare for the complete extraction operation of 1.9 -- in actual use, restore only when there is a problem, normal without recovery...
            //pkt.Reset();
            //pkt.functionID = 0xB2;
            //// (Set the recorded index number to 5)
            //int recordIndexGot = 0x0;
            //LongToBytes(ref pkt.data, 0, recordIndexGot);
            ////12 logo (to prevent incorrect setting) 1 0x55 [fixed]
            //LongToBytes(ref pkt.data, 4, WGPacketShort.SpecialFlag);

            //ret = pkt.run();
            //success = 0;
            //if (ret > 0)
            //{
            // if (pkt.recv[8] == 1)
            // {
            // log("1.8 sets the record index number that has been read successfully...");
            // success = 1;
            // }
            //}


            //1.9 Extract record operation
            //1. Get the record index number recorded by 0xB4 command recordIndex
            //2. Get the record with the specified index number through the 0xB0 instruction. Extract the record from recordIndex + 1 until the record is empty.
            //3. Set the record index number that has been read by the 0xB2 instruction. The set value is the last read card record index number.
            // After the above three steps, the entire operation of extracting records is completed
            log("1.9 extracting record operation starts...");
            pkt.Reset();
            pkt.functionID = 0xB4;
            ret = pkt.run();
            success = 0;
            if (ret > 0)
            {
                long recordIndexGotToRead = 0x0;
                recordIndexGotToRead = (long)byteToLong(pkt.recv, 8, 4);
                pkt.Reset();
                pkt.functionID = 0xB0;
                pkt.iDevSn = controllerSN;
                long recordIndexToGetStart = recordIndexGotToRead + 1;
                long recordIndexValidGet = 0;
                int cnt = 0;
                do
                {
                    if (bStopBasicFunction)
                    {
                        return 0; //2015-06-10 09:08:14 Stop
                    }
                    LongToBytes(ref pkt.data, 0, recordIndexToGetStart);
                    ret = pkt.run();
                    success = 0;
                    if (ret > 0)
                    {
                        success = 1;

                        //12 record type
                        //0=No record
                        //1=swipe record
                        //2=door magnet, button, device start, remote door open record
                        //3=Alarm record 1
                        //0xFF= indicates that the record of the specified index bit has been overwritten. Please use index 0 to retrieve the index value of the earliest record.
                        int recordType = pkt.recv[12];
                        if (recordType == 0)
                        {
                            break; //no more records
                        }
                        if (recordType == 0xff) // this index number is invalid Reset the index value
                        {
                            // Take the index bit of the earliest record
                            pkt.Reset();
                            pkt.functionID = 0xB0;
                            recordIndexToGet = 0;
                            LongToBytes(ref pkt.data, 0, recordIndexToGet);

                            ret = pkt.run();
                            success = 0;
                            if (ret > 0)
                            {
                                log("1.7 Get the information of the earliest record successfully...");
                                recordIndexGotToRead = (int)byteToLong(pkt.recv, 8, 4);
                                recordIndexToGetStart = recordIndexGotToRead;
                                continue;
                            }
                            success = 0;
                            break;
                        }
                        recordIndexValidGet = recordIndexToGetStart;

                        displayRecordInformation(pkt.recv); //2015-06-09 20:01:21

                        //.......Storing the received records
                        //*****
                        //###############
                    }
                    else
                    {
                        // extraction failed
                        break;
                    }
                    recordIndexToGetStart++;
                } while (cnt++ < 200000);
                if (success > 0)
                {
                    // Set the record index number that has been read by the 0xB2 instruction. The set value is the last read card record index number.
                    pkt.Reset();
                    pkt.functionID = 0xB2;
                    LongToBytes(ref pkt.data, 0, recordIndexValidGet);

                    //12 logo (to prevent incorrect setting) 1 0x55 [fixed]
                    LongToBytes(ref pkt.data, 4, WGPacketShort.SpecialFlag);

                    ret = pkt.run();
                    success = 0;
                    if (ret > 0)
                    {
                        if (pkt.recv[8] == 1)
                        {
                            // Complete extraction success....
                            log("1.9 Full extraction succeeded successfully...");
                            success = 1;
                        }
                    }

                }
            }

            //1.10 Remote opening [Function No.: 0x40] *************************************** *******************************************
            int doorNO = 1;
            pkt.Reset();
            pkt.functionID = 0x40;
            pkt.data[0] = (byte)(doorNO & 0xff); //2013-11-03 20:56:33
            ret = pkt.run();
            success = 0;
            if (ret > 0)
            {
                if (pkt.recv[8] == 1)
                {
                    //Open the door effectively.....
                    log("1.10 remote opening successfully...");
                    success = 1;
                }
            }

            //1.11 Privilege Add or Modify [Function Number: 0x50] ************************************* *********************************************
            // Increase the card number 0D D7 37 00, through all the doors of the current controller
            pkt.Reset();
            pkt.functionID = 0x50;
            //0D D7 37 00 Card number in the permission to be added or modified = 0x0037D70D = 3659533 (decimal)
            long cardNOOfPrivilege = 0x0037D70D;
            LongToBytes(ref pkt.data, 0, cardNOOfPrivilege);

            //20 10 01 01 Start date: January 01, 2010 (must be greater than 2001)
            pkt.data[4] = 0x20;
            pkt.data[5] = 0x10;
            pkt.data[6] = 0x01;
            pkt.data[7] = 0x01;
            //20 29 12 31 Deadline: December 31, 2029
            pkt.data[8] = 0x20;
            pkt.data[9] = 0x29;
            pkt.data[10] = 0x12;
            pkt.data[11] = 0x31;
            //01 Permitted through Gate 1 [Valid for single, double and four-door controllers]
            pkt.data[12] = 0x01;
            //01 Allowed to pass Gate 2 [Valid for two-door, four-door controller]
            pkt.data[13] = 0x01; //If gate 2 is disabled, just set to 0x00
            //01 Permitted through Gate 3 [Valid for four-door controller]
            pkt.data[14] = 0x01;
            //01 Permitted through Gate 4 [Valid for four-door controller]
            pkt.data[15] = 0x01;

            ret = pkt.run();
            success = 0;
            if (ret > 0)
            {
                if (pkt.recv[8] == 1)
                {
                    // At this time, the card number is = 0x0037D70D = 3659533 (decimal) card, the 1st door relay action.
                    log("1.11 permission added or modified successfully...");
                    success = 1;
                }
            }

            //1.12 Privilege deletion (single deletion) [Function number: 0x52] *********************************** ***********************************************
            pkt.Reset();
            pkt.functionID = 0x52;
            pkt.iDevSn = controllerSN;
            // permission card number to be deleted 0D D7 37 00 = 0x0037D70D = 3659533 (decimal)
            long cardNOOfPrivilegeToDelete = 0x0037D70D;
            LongToBytes(ref pkt.data, 0, cardNOOfPrivilegeToDelete);

            ret = pkt.run();
            success = 0;
            if (ret > 0)
            {
                if (pkt.recv[8] == 1)
                {
                    //At this time, the card with the card number = 0x0037D70D = 3659533 (decimal) will not operate.
                    log("1.12 Privilege Delete (Single Delete) Success...");
                    success = 1;
                }
            }

            //1.13 Permission Clear (all cleared) [Function Number: 0x54] ********************************** ************************************************
            pkt.Reset();
            pkt.functionID = 0x54;
            pkt.iDevSn = controllerSN;
            LongToBytes(ref pkt.data, 0, WGPacketShort.SpecialFlag);

            ret = pkt.run();
            success = 0;
            if (ret > 0)
            {
                if (pkt.recv[8] == 1)
                {
                    //This time it cleared successfully.
                    log("1.13 privilege emptied (all cleared) success...");
                    success = 1;
                }
            }

            //1.14 Total number of permissions read [Function number: 0x58] ************************************* *********************************************
            pkt.Reset();
            pkt.functionID = 0x58;
            ret = pkt.run();
            success = 0;
            if (ret > 0)
            {
                int privilegeCount = 0;
                privilegeCount = (int)byteToLong(pkt.recv, 8, 4);
                log("1.14 Total number of permissions read successfully...");

                success = 1;
            }

            // Add again for the query operation 1.11 permissions to add or modify [function number: 0x50] ******************************** **************************************************
            // Increase the card number 0D D7 37 00, through all the doors of the current controller
            pkt.Reset();
            pkt.functionID = 0x50;
            //0D D7 37 00 Card number in the permission to be added or modified = 0x0037D70D = 3659533 (decimal)
            cardNOOfPrivilege = 0x0037D70D;
            LongToBytes(ref pkt.data, 0, cardNOOfPrivilege);
            //20 10 01 01 Start date: January 01, 2010 (must be greater than 2001)
            pkt.data[4] = 0x20;
            pkt.data[5] = 0x10;
            pkt.data[6] = 0x01;
            pkt.data[7] = 0x01;
            //20 29 12 31 Deadline: December 31, 2029
            pkt.data[8] = 0x20;
            pkt.data[9] = 0x29;
            pkt.data[10] = 0x12;
            pkt.data[11] = 0x31;
            //01 Permitted through Gate 1 [Valid for single, double and four-door controllers]
            pkt.data[12] = 0x01;
            //01 Allowed to pass Gate 2 [Valid for two-door, four-door controller]
            pkt.data[13] = 0x01; //If gate 2 is disabled, just set to 0x00
            //01 Permitted through Gate 3 [Valid for four-door controller]
            pkt.data[14] = 0x01;
            //01 Permitted through Gate 4 [Valid for four-door controller]
            pkt.data[15] = 0x01;

            ret = pkt.run();
            success = 0;
            if (ret > 0)
            {
                if (pkt.recv[8] == 1)
                {
                    // At this time, the card number is = 0x0037D70D = 3659533 (decimal) card, the 1st door relay action.
                    log("1.11 permission added or modified successfully...");
                    success = 1;
                }
            }

            //1.15 permission query [function number: 0x5A] *************************************** *******************************************
            pkt.Reset();
            pkt.functionID = 0x5A;
            pkt.iDevSn = controllerSN;
            // (Check the card number is 0D D7 37 00 permissions)
            long cardNOOfPrivilegeToQuery = 0x0037D70D;
            LongToBytes(ref pkt.data, 0, cardNOOfPrivilegeToQuery);

            ret = pkt.run();
            success = 0;
            if (ret > 0)
            {

                long cardNOOfPrivilegeToGet = 0;
                cardNOOfPrivilegeToGet = byteToLong(pkt.recv, 8, 4);
                if (cardNOOfPrivilegeToGet == 0)
                {
                    //When there is no permission: (The card number is 0)
                    log("1.15 does not have permission information: (the card number part is 0)");
                }
                else
                {
                    // Specific permission information...
                    log("1.15 has permission information...");
                }
                log("1.15 permission query succeeded...");
                success = 1;
            }

            //1.16 Get permission for the specified index number [Function number: 0x5C] *********************************** ***********************************************
            pkt.Reset();
            pkt.functionID = 0x5C;
            pkt.iDevSn = controllerSN;
            long QueryIndex = 1; // Index number (starting from 1);
            LongToBytes(ref pkt.data, 0, QueryIndex);

            ret = pkt.run();
            success = 0;
            if (ret > 0)
            {

                long cardNOOfPrivilegeToGet = 0;
                cardNOOfPrivilegeToGet = byteToLong(pkt.recv, 8, 4);
                if (4294967295 == cardNOOfPrivilegeToGet) //FFFFFFFF corresponds to 4294967295
                {
                    log("1.16 does not have permission information: (permission deleted)");
                }
                else if (cardNOOfPrivilegeToGet == 0)
                {
                    //When there is no permission: (The card number is 0)
                    log("1.16 does not have permission information: (the card number part is 0) - there is no permission after this index number");
                }
                else
                {
                    // Specific permission information...
                    log("1.16 has permission information...");
                }
                log("1.16 Get permission for the specified index number succeeded...");
                success = 1;
            }


            //1.17 Set the door control parameters (online/delay) [Function number: 0x80] ******************************* ************************************************** *
            pkt.Reset();
            pkt.functionID = 0x80;
            // (Set the number 2 door online door opening delay 3 seconds)
            pkt.data[0] = 0x01; //door 2
            pkt.data[1] = 0x03; //online
            pkt.data[2] = 0x06; //Open the gate delay

            ret = pkt.run();
            success = 0;
            if (ret > 0)
            {
                if (pkt.data[0] == pkt.recv[8] && pkt.data[1] == pkt.recv[9] && pkt.data[2] == pkt.recv[10])
                {
                    // When successful, the return value is consistent with the setting
                    log("1.17 Set Gate Control Parameters Successful...");
                    success = 1;
                }
                else
                {
                    //failure
                }
            }

            //1.21 Permissions are added in order from small to large [Function No.: 0x56] Applicable to the number of permissions over 1000, less than 80,000*********************** ************************************************** *********
            // This function is implemented to completely update all permissions, the user does not need to clear the previous permissions. Just upload the permission sequence from the first to the last upload completed. If interrupted in the middle, still the original permissions
            // Suggest that the number of permissions is updated more than 50, you can use this command

            log("1.21 permissions are added in order of small to large [function number: 0x56] start...");
            log("10,000 privilege...");

            // Take 10000 card numbers as an example. The simplified sorting here is directly 10000 cards starting with 50001. The user sorts the card numbers to be uploaded as needed.
            int cardCount = 10000; //2015-06-09 20:20:20 total number of cards
            long[] cardArray = new long[cardCount];
            for (int i = 0; i < cardCount; i++)
            {
                cardArray[i] = 50001 + i;
            }

            for (int i = 0; i < cardCount; i++)
            {
                if (bStopBasicFunction)
                {
                    return 0;  //2015-06-10 09:08:14 stop
                }
                pkt.Reset();
                pkt.functionID = 0x56;

                cardNOOfPrivilege = cardArray[i];
                LongToBytes(ref pkt.data, 0, cardNOOfPrivilege);

                // Other parameters are simplified when unified, can be modified according to each card
                //20 10 01 01 Start date: January 01, 2010 (must be greater than 2001)
                pkt.data[4] = 0x20;
                pkt.data[5] = 0x10;
                pkt.data[6] = 0x01;
                pkt.data[7] = 0x01;
                //20 29 12 31 Deadline: December 31, 2029
                pkt.data[8] = 0x20;
                pkt.data[9] = 0x29;
                pkt.data[10] = 0x12;
                pkt.data[11] = 0x31;
                //01 Permitted through Gate 1 [Valid for single, double and four-door controllers]
                pkt.data[12] = 0x01;
                //01 Allowed to pass Gate 2 [Valid for two-door, four-door controller]
                pkt.data[13] = 0x01; //If gate 2 is disabled, just set to 0x00
                //01 Permitted through Gate 3 [Valid for four-door controller]
                pkt.data[14] = 0x01;
                //01 Permitted through Gate 4 [Valid for four-door controller]
                pkt.data[15] = 0x01;

                LongToBytes(ref pkt.data, 32 - 8, cardCount); //Total number of permissions
                LongToBytes(ref pkt.data, 35 - 8, i + 1);//index of current permissions (starting at 1)

                ret = pkt.run();
                success = 0;
                if (ret > 0)
                {
                    if (pkt.recv[8] == 1)
                    {
                        success = 1;
                    }
                    if (pkt.recv[8] == 0xE1)
                    {
                        log("1.21	Permissions are added in order from small to large [Function number: 0x56] =0xE1 indicates that the card number is not sorted from small to large...???");
                        success = 0;
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            if (success == 1)
            {
                log("1.21 permissions are added in order of small to large [function number: 0x56] success...");
            }
            else
            {
                log("1.21 permissions are added in order of small to large [function number: 0x56] failed...????");
            }

            //Other instructions ********************************************* ************************************


            // ************************************************ **********************************

            //End*********************************************** ***********************************
            pkt.close(); //Close communication
            return success;
        }

        /// <summary>
        /// Receive server setup test
        /// </summary>
        /// <param name="ControllerIP">Set the controller IP address</param>
        /// <param name="controllerSN">Controller serial number set </param>
        /// <param name="watchServerIP">Server IP to be set</param>
        /// <param name="watchServerPort">Port to set up</param>
        /// <returns>0 failed, 1 means success</returns>
        int testWatchingServer(string ControllerIP, long controllerSN, string watchServerIP, int watchServerPort)  //接收服务器测试 -- 设置
        {
            int ret = 0;
            int success = 0;  //0 Failure, 1 means success

            WGPacketShort pkt = new WGPacketShort();
            pkt.iDevSn = controllerSN;
            pkt.IP = ControllerIP;

            //1.18 Set the IP and port of the receiving server [Function No.: 0x90] ********************************** ************************************************
            //(If you don't want the controller to send data, just set the receiving server's IP to 0.0.0.0.)
            // Receive server port: 61005
            //Send every 5 seconds: 05
            pkt.Reset();
            pkt.functionID = 0x90;
            string[] strIP = watchServerIP.Split('.');
            if (strIP.Length == 4)
            {
                pkt.data[0] = byte.Parse(strIP[0]);
                pkt.data[1] = byte.Parse(strIP[1]);
                pkt.data[2] = byte.Parse(strIP[2]);
                pkt.data[3] = byte.Parse(strIP[3]);
            }
            else
            {
                return 0;
            }

            // Receive server port: 61005
            pkt.data[4] = (byte)((watchServerPort & 0xff));
            pkt.data[5] = (byte)((watchServerPort >> 8) & 0xff);

            //Send every 5 seconds: 05 (The period of timed upload information is 5 seconds [send every 5 seconds during normal operation, send immediately when swiping]]
            pkt.data[6] = 5;

            ret = pkt.run();
            success = 0;
            if (ret > 0)
            {
                if (pkt.recv[8] == 1)
                {
                    log("1.18 Set the IP and port of the receiving server. Success...");
                    success = 1;
                }
            }


            //1.19 Read the IP and port of the receiving server [function number: 0x92] ********************************* *************************************************
            pkt.Reset();
            pkt.functionID = 0x92;

            ret = pkt.run();
            success = 0;
            if (ret > 0)
            {
                log("1.19 Read the IP and Port of the Receiving Server Success...");
                success = 1;
            }
            pkt.close();
            return success;
        }


        /// <summary>
        /// Open the receiving server to receive data (note that the firewall should allow all packets on this port to enter)
        /// </summary>
        /// <param name="watchServerIP">Receive server IP (generally current computer IP)</param>
        /// <param name="watchServerPort">Receive server port</param>
        /// <returns>1 means success, otherwise it fails</returns>
        int WatchingServerRuning(string watchServerIP, int watchServerPort)
        {
            // Pay attention to the firewall to allow all the packets of this port to enter
            try
            {
                WG3000_COMM.Core.wgUdpServerCom udpserver = new WG3000_COMM.Core.wgUdpServerCom(watchServerIP, watchServerPort);

                if (!udpserver.IsWatching())
                {
                    log("Enter the receiving server monitoring status....Failed");
                    return -1;
                }
                log("Enter the receiving server monitoring status....");
                long recordIndex = 0;
                int recv_cnt;
                while (!bStopWatchServer)
                {
                    recv_cnt = udpserver.receivedCount();
                    if (recv_cnt > 0)
                    {
                        byte[] buff = udpserver.getRecords();
                        if (buff[1] == 0x20) //
                        {
                            long sn;
                            long recordIndexGet;
                            sn = byteToLong(buff, 4, 4);
                            log(string.Format("Received a packet from controller SN = {0}..\r\n", sn));

                            recordIndexGet = byteToLong(buff, 8, 4);

                            if (recordIndex < recordIndexGet)
                            {
                                recordIndex = recordIndexGet;

                                displayRecordInformation(buff); //2015-06-09 20:01:21

                            }
                        }

                    }
                    else
                    {
                        System.Threading.Thread.Sleep(10);  //'延时10ms
                        Application.DoEvents();

                    }
                }
                udpserver.Close();
                return 1;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                MessageBox.Show(ex.ToString());
                // throw;
            }
            return 0;
        }

        private void FunctionsTest_Load(object sender, EventArgs e)
        {

        }

        private void FunctionsTest_FormClosing(object sender, FormClosingEventArgs e)
        {
            new WGPacketShort().close();
        }



    }
}
