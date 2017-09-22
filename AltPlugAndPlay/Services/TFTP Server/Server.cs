using AltPlugAndPlay.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NVelocity;
using NVelocity.App;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tftp.Net;

namespace AltPlugAndPlay.Services.TFTP_Server
{
    public class Server
    {
        private static Server s_instance = null;
        TftpServer m_server;

        string dummyConfig = @"!
hostname bob
ip domain-name minions.com
!
interface FastEthernet0/0
 ip address dhcp
 description $Description
 no shutdown
!
interface FastEthernet0/1
 ip address 10.1.1.1 255.255.255.0
 no shutdown
";

        public Server()
        {
            if(s_instance != null)
            {
                throw new Exception("Only a single instance of TFTP Server can be instantiated at a time");
            }
            s_instance = this;

            m_server = new TftpServer();
            m_server.OnReadRequest += server_OnReadRequest;
            m_server.OnWriteRequest += server_OnWriteRequest;
            m_server.Start();
        }
        private void OnReadRequest(
            ITftpTransfer transfer, 
            EndPoint client
            )
        {
            var connectionString = Startup.Configuration.GetValue<string>("Data:DefaultConnection:ConnectionString");
            var dbOptions = new DbContextOptionsBuilder<PnPServerContext>();
            dbOptions.UseSqlServer(connectionString);

            var dbContext = new PnPServerContext(dbOptions.Options);

            string ipAddress = "";
            if(client.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                ipAddress = (client as IPEndPoint).Address.ToString();
            } else if (client.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
            {
                ipAddress = (client as IPEndPoint).Address.ToString();
            }
            var networkDevice = dbContext.NetworkDevices.Where(x => x.IPAddress == ipAddress).FirstOrDefault();

            if(networkDevice == null)
            {
                transfer.Cancel(TftpErrorPacket.FileNotFound);
            }
            else
            {
                var config = Task.Run<string>(() => { return ConfigurationGenerator.Generator.Generate(ipAddress, dbContext); }).Result;
                var data = new MemoryStream(Encoding.ASCII.GetBytes(config));
                transfer.Start(data);
            }
        }

        static void server_OnReadRequest(ITftpTransfer transfer, EndPoint client)
        {
            if(s_instance == null)
            {
                throw new Exception("Invalid state, no static instance of TFTP Server");
            }
            s_instance.OnReadRequest(transfer, client);
        }

        static void server_OnWriteRequest(ITftpTransfer transfer, EndPoint client)
        {

        }
    }
}
