using Gate5.Watch.DataAccess;
using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace Gate5.Watch
{
    public partial class WatchTurnstiles : Form
    {

        Turnstile currentController = new Turnstile();
        Boolean bStopWatchServer = false;
        public string ServerIP { get; set; }
        public string ServerPort { get; set; }

        public WatchTurnstiles()
        {
            InitializeComponent();
        }
        #region LibiaryFunctions
        class WGPacketShort
        {
            public const int WGPacketSize = 64;			   //Message length
            //2015-04-29 22:22:41 const static unsigned char	 Type = 0x19;					//Types of
            public const int Type = 0x17;       //2015-04-29 22:22:50			//Types of

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
                controller.PORT = 60000;        // controller port
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
        #endregion
        #region Functions


        void open(String ControllerIP, long controllerSN, int doorNo)
        {
            int ret = 0;
            //int success = 0;  //0 Failure, 1 means success


            // Create a short message pkt
            WGPacketShort pkt = new WGPacketShort();
            pkt.iDevSn = controllerSN;
            pkt.IP = ControllerIP;

            //1.4 Query controller status [function number: 0x20] (for real-time monitoring) ******************************** **************************************************
            pkt.Reset();
            pkt.functionID = 0x40;
            pkt.data[0] = (byte)(doorNo & 0xff); //2013-11-03 20:56:33
            ret = pkt.run();
            //success = 0;
            if (ret > 0)
            {
                if (pkt.recv[8] == 1)
                {
                    //Open the door effectively.....
                    //success = 1;
                }
            }
        }

        int WatchingServerRuning()
        {
            // Pay attention to the firewall to allow all the packets of this port to enter
            try
            {
                WG3000_COMM.Core.wgUdpServerCom udpserver = new WG3000_COMM.Core.wgUdpServerCom(ServerIP, Int32.Parse(ServerPort));

                if (!udpserver.IsWatching())
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        log("Enter the receiving server monitoring status....Failed");
                    });
                    return -1;

                }


                this.Invoke((MethodInvoker)delegate
                {
                    log("Enter the receiving server monitoring status....");
                });
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
                            this.Invoke((MethodInvoker)delegate
                            {
                                log(string.Format("Received a packet from controller SN = {0}..\r\n", sn));
                            });

                            recordIndexGet = byteToLong(buff, 8, 4);

                            if (recordIndex < recordIndexGet)
                            {
                                recordIndex = recordIndexGet;
                                this.Invoke((MethodInvoker)delegate
                                {
                                    displayRecordInformation(buff); //2015-06-09 20:01:21
                                });

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
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                MessageBox.Show(ex.ToString());
                btnWatch.Enabled = true;
                // throw;
            }
            return 0;

        }
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
            string recordCardNO = byteToLong(recv, 16, 4).ToString();

            DateTime todayDate = DateTime.Now.Date;

            recordCardNO = "CT" + recordCardNO.Insert(1, todayDate.ToString("yyMM"));


            if (recordCardNO != null && recordCardNO.Count() >= 10)
            {
                //DateTime today = DateTime.Now.Date;
                //DateTime qrdate = Convert.ToDateTime(recordCardNO.ToString().Substring(0, 7)).Date;
                //if (today!=qrdate)
                //{
                //    log("Entry Date for Ticket " + recordCardNO.ToString() + " has been expired!!");
                //    return;
                //}

                if (DateTime.Now.Hour < 18)
                {
                    GateEntities db = new GateEntities();

                    Ticket itick = db.Tickets.Where(x => x.TicketNo == recordCardNO && System.Data.Objects.EntityFunctions.TruncateTime((DateTime)x.CreatedDate) == todayDate && x.Status == false).OrderByDescending(x => x.Id).FirstOrDefault();
                    if (itick != null && itick.Id > 0)
                    {
                        open(currentController.IP, currentController.SerialNo, currentController.door);
                        itick.Status = true;
                        itick.EnteranceDate = DateTime.Now;
                        itick.ReadCount = 1;
                        db.SaveChanges();
                    }
                }
                else
                {
                    log("Time over! Zoo is closed!");
                    return;
                }


            }
            else
            {
                log("Ticket No " + recordCardNO.ToString() + " is not valid.");
                return;
            }

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
                log(string.Format(" Index bit={0} ", recordIndex));
                log(string.Format(" Ticket number = {0}", recordCardNO));
                log(string.Format(" Gate Type = {0}", recordDoorNO));
                log(string.Format(" In and Out = {0}", recordInOrOut == 1 ? "In" : "Out"));
                //log(string.Format(" Valid = {0}", recordValid == 1 ? "PASS" : "Forbidden"));
                log(string.Format(" time = {0}", recordTime));
                //log(string.Format(" Description = {0}", getReasonDetailEnglish(reason)));
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
        #region Records Messages
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
        #endregion

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
        void log(string info)  //Log information
        {
            txtInfo.AppendText(string.Format("{0} {1}\r\n", DateTime.Now.ToString("HH:mm:ss"), info)); //2015-11-03 20:55:49 display time
            txtInfo.ScrollToCaret();//Scroll to the cursor
            Application.DoEvents();
        }
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
        #endregion

        System.Threading.Thread t;

        private void WatchTurnstiles_Load(object sender, EventArgs e)
        {
            GateEntities db = new GateEntities();
            var server = db.TurnStileServers.Find(5);
            ServerIP = server.ServerIP;
            ServerPort = server.Port.ToString();
            gno.Text = server.Description;
            currentController = db.Turnstiles.Where(x => x.ServerID == server.Id).FirstOrDefault();

            svrip.Text = ServerIP + " : " + ServerPort;
            btnWatch.Enabled = false;
            bStopWatchServer = false;
            t = new System.Threading.Thread(onloadconnet);
            t.Start();
        }

        public void onloadconnet()
        {
            WatchingServerRuning(); //Server is running....
        }

        private void btnWatch_Click(object sender, EventArgs e)
        {
            GateEntities db = new GateEntities();
            var server = db.TurnStileServers.Find(5);
            ServerIP = server.ServerIP;
            ServerPort = server.Port.ToString();
            btnWatch.Enabled = false;
            btnStop.Enabled = true;
            this.Text = "WatchTurnstiles-" + lblPort.Text;
            bStopWatchServer = false;
            WatchingServerRuning(); //Server is running....
            bStopWatchServer = true;

        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            bStopWatchServer = true;
            btnWatch.Enabled = true;
            btnStop.Enabled = false;
        }



        private void WatchTurnstiles_FormClosing(object sender, FormClosingEventArgs e)
        {
            new WGPacketShort().close();
            Environment.Exit(Environment.ExitCode);
            Application.ExitThread();
            Application.Exit();
        }


    }
}
