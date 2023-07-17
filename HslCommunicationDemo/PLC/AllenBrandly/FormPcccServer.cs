﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HslCommunication.Profinet;
using HslCommunication;
using HslCommunication.ModBus;
using System.Threading;
using System.Xml.Linq;
using HslCommunicationDemo.DemoControl;

namespace HslCommunicationDemo
{
	public partial class FormPcccServer : HslFormContent
	{
		public FormPcccServer( )
		{
			InitializeComponent( );
		}



		private void FormSiemens_Load( object sender, EventArgs e )
		{
			panel2.Enabled = false;


			if(Program.Language == 2)
			{
				Text = "Cip-PCCC Virtual Server [support single value]";
				label3.Text = "port:";
				button1.Text = "Start Server";
				button11.Text = "Close Server";
				label11.Text = "This server is not a strict cip protocol and only supports perfect communication with HSL components.";
			}


			addressExampleControl = new AddressExampleControl( );
			addressExampleControl.SetAddressExample( HslCommunicationDemo.PLC.AllenBrandly.Helper.GetPCCCAddressExamples( ) );
			userControlReadWriteServer1.AddSpecialFunctionTab( addressExampleControl, false, DeviceAddressExample.GetTitle( ) );

			codeExampleControl = new CodeExampleControl( );
			userControlReadWriteServer1.AddSpecialFunctionTab( codeExampleControl, false, CodeExampleControl.GetTitle( ) );
		}

		private void FormSiemens_FormClosing( object sender, FormClosingEventArgs e )
		{
			cipServer?.ServerClose( );
		}

		#region Server Start

		private HslCommunication.Profinet.AllenBradley.AllenBradleyPcccServer cipServer;
		private AddressExampleControl addressExampleControl;
		private CodeExampleControl codeExampleControl;

		private void button1_Click( object sender, EventArgs e )
		{
			if (!int.TryParse( textBox2.Text, out int port ))
			{
				MessageBox.Show( DemoUtils.PortInputWrong );
				return;
			}


			try
			{
				cipServer = new HslCommunication.Profinet.AllenBradley.AllenBradleyPcccServer( );                       // 实例化对象
				cipServer.ActiveTimeSpan = TimeSpan.FromHours( 1 );
				cipServer.OnDataReceived += BusTcpServer_OnDataReceived;
				
				cipServer.ServerStart( port );

				button1.Enabled = false;
				panel2.Enabled = true;
				button11.Enabled = true;
				userControlReadWriteServer1.SetReadWriteServer( cipServer, "F8:5", 10 );

				// 设置代码示例
				codeExampleControl.SetCodeText( "server", "", cipServer );
			}
			catch (Exception ex)
			{
				MessageBox.Show( ex.Message );
			}
		}

		private void button11_Click( object sender, EventArgs e )
		{
			// 停止服务
			cipServer?.ServerClose( );
			button1.Enabled = true;
			button11.Enabled = false;
		}

		private void BusTcpServer_OnDataReceived( object sender, object source, byte[] receive )
		{
			// 我们可以捕获到接收到的客户端的modbus报文
			// 如果是TCP接收的
			if (source is HslCommunication.Core.Net.AppSession session)
			{
				// 获取当前客户的IP地址
				string ip = session.IpAddress;
			}

			// 如果是串口接收的
			if (source is System.IO.Ports.SerialPort serialPort)
			{
				// 获取当前的串口的名称
				string portName = serialPort.PortName;
			}
		}

		#endregion

		private void userControlReadWriteServer1_Load( object sender, EventArgs e )
		{

		}


		public override void SaveXmlParameter( XElement element )
		{
			element.SetAttributeValue( DemoDeviceList.XmlPort, textBox2.Text );

			this.userControlReadWriteServer1.GetDataTable( element );
		}

		public override void LoadXmlParameter( XElement element )
		{
			base.LoadXmlParameter( element );
			textBox2.Text = element.Attribute( DemoDeviceList.XmlPort ).Value;

			this.userControlReadWriteServer1.LoadDataTable( element );
		}

		private void userControlHead1_SaveConnectEvent_1( object sender, EventArgs e )
		{
			userControlHead1_SaveConnectEvent( sender, e );
		}
	}
}
