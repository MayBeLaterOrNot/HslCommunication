﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HslCommunication.Profinet;
using System.Threading;
using HslCommunication.Profinet.Vigor;
using HslCommunication;
using System.Xml.Linq;
using HslCommunicationDemo.PLC.Common;
using HslCommunicationDemo.DemoControl;

namespace HslCommunicationDemo
{
	public partial class FormVigorSerialOverTcp : HslFormContent
	{
		public FormVigorSerialOverTcp( )
		{
			InitializeComponent( );
			vigor = new VigorSerialOverTcp( );
		}


		private VigorSerialOverTcp vigor = null;
		private AddressExampleControl addressExampleControl;
		private CodeExampleControl codeExampleControl;

		private void FormSiemens_Load( object sender, EventArgs e )
		{
			panel2.Enabled = false;

			Language( Program.Language );

			addressExampleControl = new AddressExampleControl( );
			addressExampleControl.SetAddressExample( HslCommunicationDemo.PLC.Vigor.Helper.GetVigorAddress( ) );
			userControlReadWriteDevice1.AddSpecialFunctionTab( addressExampleControl, false, DeviceAddressExample.GetTitle( ) );

			codeExampleControl = new CodeExampleControl( );
			userControlReadWriteDevice1.AddSpecialFunctionTab( codeExampleControl, false, CodeExampleControl.GetTitle( ) );
		}


		private void Language( int language )
		{
			if (language == 2)
			{
				Text = "Vigor Read PLC Demo";

				label27.Text = "Ip:";
				label26.Text = "Port:";
				button1.Text = "Connect";
				button2.Text = "Disconnect";
				label21.Text = "Address:";
			}
		}

		private void FormSiemens_FormClosing( object sender, FormClosingEventArgs e )
		{

		}
		
		#region Connect And Close

		private void button1_Click( object sender, EventArgs e )
		{
			if (!int.TryParse( textBox2.Text, out int port ))
			{
				MessageBox.Show( DemoUtils.PortInputWrong );
				return;
			}

			vigor?.ConnectClose( );
			vigor = new VigorSerialOverTcp( );
			vigor.IpAddress = textBox1.Text;
			vigor.Port = port;
			vigor.LogNet = LogNet;

			try
			{
				vigor.Station = byte.Parse( textBox15.Text );
				OperateResult connect = vigor.ConnectServer( );
				if (connect.IsSuccess)
				{
					MessageBox.Show( HslCommunication.StringResources.Language.ConnectedSuccess );

					button2.Enabled = true;
					button1.Enabled = false;
					panel2.Enabled = true;

					// 设置基本的读写信息
					userControlReadWriteDevice1.SetReadWriteNet( vigor, "D100", true );
					// 设置批量读取
					userControlReadWriteDevice1.BatchRead.SetReadWriteNet( vigor, "D100", string.Empty );
					// 设置报文读取
					userControlReadWriteDevice1.MessageRead.SetReadSourceBytes( m => vigor.ReadFromCoreServer( m ), string.Empty, string.Empty );

					// 设置代码示例
					codeExampleControl.SetCodeText( vigor, nameof( vigor.Station ) );
				}
				else
				{
					MessageBox.Show( HslCommunication.StringResources.Language.ConnectedFailed );
				}

			}
			catch (Exception ex)
			{
				MessageBox.Show( ex.Message );
			}
		}

		private void button2_Click( object sender, EventArgs e )
		{
			// 断开连接
			vigor.ConnectClose( );
			button2.Enabled = false;
			button1.Enabled = true;
			panel2.Enabled = false;
		}



		#endregion

		public override void SaveXmlParameter( XElement element )
		{
			element.SetAttributeValue( DemoDeviceList.XmlIpAddress, textBox1.Text );
			element.SetAttributeValue( DemoDeviceList.XmlPort, textBox2.Text );
			element.SetAttributeValue( DemoDeviceList.XmlStation, textBox15.Text );

			this.userControlReadWriteDevice1.GetDataTable( element );
		}

		public override void LoadXmlParameter( XElement element )
		{
			base.LoadXmlParameter( element );
			textBox1.Text = element.Attribute( DemoDeviceList.XmlIpAddress ).Value;
			textBox2.Text = element.Attribute( DemoDeviceList.XmlPort ).Value;
			textBox15.Text = element.Attribute( DemoDeviceList.XmlStation ).Value;

			if (this.userControlReadWriteDevice1.LoadDataTable( element ) > 0)
				this.userControlReadWriteDevice1.SelectTabDataTable( );
		}

		private void userControlHead1_SaveConnectEvent_1( object sender, EventArgs e )
		{
			userControlHead1_SaveConnectEvent( sender, e );
		}
	}
}
