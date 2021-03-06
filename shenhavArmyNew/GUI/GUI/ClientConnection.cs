﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GUI.ViewModel;

namespace GUI
{
    class ClientConnection
    {
        static Regex CurrentThreadRegex = new Regex("currentThread={(.*?)}");
        static Regex newLogFilePath = new Regex("logFilePath={(.*?)}");
        static int clientNumber;
        public static int sendMessage(string data, Socket sender)
        {
            byte[] messageSent;
            messageSent = Encoding.ASCII.GetBytes(data);
            int byteSent = sender.Send(messageSent);
            // Data buffer 

            return byteSent;
        }
        /// Function - recieveMessage
        /// <summary>
        /// this function for recieving a message and it returns the actually message that was being sent.
        /// </summary>
        /// <param name="sender"> the socket for i got the message from.</param>
        /// <returns></returns>
        public static string recieveMessage(Socket sender)
        {
            byte[] messageReceived = new byte[1024];
            int byteRecv = sender.Receive(messageReceived);
            string recieve = Encoding.ASCII.GetString(messageReceived,
                                             0, byteRecv);
            return recieve;
        }
        public static int GetClientNumber()
        {
            return clientNumber;
        }
        /// Function - ExecuteClient
        /// <summary>
        /// execute the actuall client.
        /// </summary>
        /// <param name="data"> the data of the client.</param>
        public static void ExecuteClient(string data)
        {
            Socket sender = null;
            try
            {

                // Establish the remote endpoint  
                // for the socket. This example  
                // uses port 11111 on the local  
                // computer. 
                IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipAddr = ipHost.AddressList[0];
                IPEndPoint localEndPoint = new IPEndPoint(ipAddr, 11111);

                // Creation TCP/IP Socket using  
                // Socket Class Costructor 
                sender = new Socket(ipAddr.AddressFamily,
                           SocketType.Stream, ProtocolType.Tcp);

                try
                {

                    // Connect Socket to the remote  
                    // endpoint using method Connect() 
                    sender.Connect(localEndPoint);

                    // We print EndPoint information  
                    // that we are connected 
                    Console.WriteLine("Socket connected to -> {0} ",
                                  sender.RemoteEndPoint.ToString());

                    // Creation of messagge that 
                    // we will send to Server 
                    sendMessage(data, sender);
                    if (data != "exit")
                    {
                        string newData = recieveMessage(sender);
                        clientNumber= int.Parse(CurrentThreadRegex.Match(newData).Groups[1].Value);
                        newData = Regex.Replace(newData, @"currentThread={(.*?)}", "");
                        NavigationViewModel.fileList[clientNumber].ResultBlock = newData.Substring(0,newData.IndexOf("logFilePath"));
                        NavigationViewModel.fileList[clientNumber].NewLogFile = File.ReadAllText(newLogFilePath.Match(newData).Groups[1].Value);
                        NavigationViewModel.fileList[clientNumber].SaveLogFile = true;
                        //Change the textBlock to the error or final path depends if the code is good or bad.
                    }
                    else
                    {
                        recieveMessage(sender);
                    }
                    sender.Close();
                    // We receive the messagge using  
                    // the method Receive(). This  
                    // method returns number of bytes 
                    // received, that we'll use to  
                    // convert them to string 


                    // Close Socket using  
                    // the method Close() 
                    //sender.Shutdown(SocketShutdown.Both);
                    //sender.Close();
                }

                // Manage of Socket's Exceptions 
                catch (ArgumentNullException ane)
                {

                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }

                catch (SocketException se)
                {

                    Console.WriteLine("SocketException : {0}", se.ToString());
                }

                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }
            }

            catch (Exception e)
            {

                Console.WriteLine(e.ToString());
            }

        }
    }
}
