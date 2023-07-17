﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HslCommunication.Profinet.FATEK;
using HslCommunication;
using HslCommunication.ModBus;
using System.Threading;
using System.IO;
using System.Xml.Linq;
using HslCommunicationDemo.DemoControl;

namespace HslCommunicationDemo
{
	public partial class FormFatekProgrameServer : HslFormContent
	{
		public FormFatekProgrameServer( )
		{
			InitializeComponent( );
		}

		private void FormSiemens_Load( object sender, EventArgs e )
		{
			panel2.Enabled = false;

			checkBox3.CheckedChanged += CheckBox3_CheckedChanged;

			if (Program.Language == 2)
			{
				Text = "Fatek Virtual Server[supports TCP and Serial";
				label3.Text = "port:";
				button1.Text = "Start Server";
				button11.Text = "Close Server";

				label14.Text = "Com:";
				button5.Text = "Open Com";
				checkBox3.Text = "str-reverse";
			}

			addressExampleControl = new AddressExampleControl( );
			addressExampleControl.SetAddressExample( HslCommunicationDemo.PLC.Fatek.Helper.GetDeviceAddressExamples( ) );
			userControlReadWriteServer1.AddSpecialFunctionTab( addressExampleControl, false, DeviceAddressExample.GetTitle( ) );

			codeExampleControl = new CodeExampleControl( );
			userControlReadWriteServer1.AddSpecialFunctionTab( codeExampleControl, false, CodeExampleControl.GetTitle( ) );
		}

		private void CheckBox3_CheckedChanged( object sender, EventArgs e )
		{
			if (fatekServer != null)
			{
				//fatekServer.IsStringReverse = checkBox3.Checked;
			}
		}
		
		private void FormSiemens_FormClosing( object sender, FormClosingEventArgs e )
		{
			fatekServer?.ServerClose( );
		}

		#region Server Start


		private FatekProgramServer fatekServer;
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
				fatekServer = new FatekProgramServer( );                       // 实例化对象
				fatekServer.Station = byte.Parse( textBox1.Text );
				fatekServer.ActiveTimeSpan = TimeSpan.FromHours( 1 );
				fatekServer.OnDataReceived += BusTcpServer_OnDataReceived;

				//sPBServer.IsStringReverse = checkBox3.Checked;

				userControlReadWriteServer1.SetReadWriteServer( fatekServer, "D100" );
				fatekServer.ServerStart( port );

				button1.Enabled = false;
				panel2.Enabled = true;
				button11.Enabled = true;


				// 设置代码示例
				codeExampleControl.SetCodeText( "server", "", fatekServer, nameof( fatekServer.Station ) );
			}
			catch (Exception ex)
			{
				MessageBox.Show( ex.Message );
			}
		}


		private void button11_Click( object sender, EventArgs e )
		{
			// 停止服务
			fatekServer?.ServerClose( );
			button1.Enabled = true;
			button5.Enabled = true;
			button11.Enabled = false;
		}

		private void BusTcpServer_OnDataReceived( object sender, object source, byte[] modbus )
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

		private void button5_Click( object sender, EventArgs e )
		{
			// 启动串口
			if (fatekServer != null)
			{
				try
				{
					fatekServer.StartSerialSlave( textBox10.Text );
					button5.Enabled = false;

					// 设置代码示例
					codeExampleControl.SetCodeText( "server", textBox10.Text, fatekServer, nameof( fatekServer.Station ) );
				}
				catch(Exception ex)
				{
					MessageBox.Show( "Start Failed：" + ex.Message );
				}
			}
			else
			{
				MessageBox.Show( "Start tcp server first please!" );
			}
		}


		public override void SaveXmlParameter( XElement element )
		{
			element.SetAttributeValue( DemoDeviceList.XmlPort, textBox2.Text );
			element.SetAttributeValue( DemoDeviceList.XmlCom, textBox10.Text );
			element.SetAttributeValue( DemoDeviceList.XmlStringReverse, checkBox3.Checked );


			this.userControlReadWriteServer1.GetDataTable( element );
		}

		public override void LoadXmlParameter( XElement element )
		{
			base.LoadXmlParameter( element );
			textBox2.Text = element.Attribute( DemoDeviceList.XmlPort ).Value;
			textBox10.Text = element.Attribute( DemoDeviceList.XmlCom ).Value;
			checkBox3.Checked = bool.Parse( element.Attribute( DemoDeviceList.XmlStringReverse ).Value );

			this.userControlReadWriteServer1.LoadDataTable( element );
		}

		private void userControlHead1_SaveConnectEvent_1( object sender, EventArgs e )
		{
			userControlHead1_SaveConnectEvent( sender, e );
		}

		private void panel1_Paint( object sender, PaintEventArgs e )
		{

		}
	}
}
