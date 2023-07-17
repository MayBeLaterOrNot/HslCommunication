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
using HslCommunication.Profinet.AllenBradley;
using HslCommunication;
using System.Xml.Linq;
using HslCommunicationDemo.PLC.AllenBrandly;
using HslCommunicationDemo.DemoControl;

namespace HslCommunicationDemo
{
	public partial class FormAllenBrandly : HslFormContent
	{
		public FormAllenBrandly( )
		{
			InitializeComponent( );
			allenBradleyNet = new AllenBradleyNet( "192.168.0.110" );
		}


		private AllenBradleyNet allenBradleyNet = null;
		private AllenBrandlyControl control;
		private AddressExampleControl addressExampleControl;
		private CodeExampleControl codeExampleControl;

		private void FormSiemens_Load( object sender, EventArgs e )
		{
			panel2.Enabled = false;

			Language( Program.Language );
			control = new AllenBrandlyControl( );
			this.userControlReadWriteDevice1.AddSpecialFunctionTab( control );

			addressExampleControl = new AddressExampleControl( );
			addressExampleControl.SetAddressExample( HslCommunicationDemo.PLC.AllenBrandly.Helper.GetCIPAddressExamples( ) );
			userControlReadWriteDevice1.AddSpecialFunctionTab( addressExampleControl, false, DeviceAddressExample.GetTitle( ) );

			codeExampleControl = new CodeExampleControl( );
			userControlReadWriteDevice1.AddSpecialFunctionTab( codeExampleControl, false, CodeExampleControl.GetTitle( ) );
		}


		private void Language( int language )
		{
			if (language == 2)
			{
				Text = "AllenBrandly Read PLC Demo";
				label1.Text = "Ip:";
				label3.Text = "Port:";
				button1.Text = "Connect";
				button2.Text = "Disconnect";
			}
		}

		private void FormSiemens_FormClosing( object sender, FormClosingEventArgs e )
		{

		}

		#region Connect And Close

		private void button1_Click( object sender, EventArgs e )
		{
			// 连接
			if (!int.TryParse( textBox2.Text, out int port ))
			{
				MessageBox.Show( DemoUtils.PortInputWrong );
				return;
			}

			if (!byte.TryParse( textBox15.Text, out byte slot ))
			{
				MessageBox.Show( DemoUtils.SlotInputWrong );
				return;
			}

			allenBradleyNet.IpAddress = textBox1.Text;
			allenBradleyNet.Port = port;
			allenBradleyNet.Slot = slot;
			allenBradleyNet.LogNet = LogNet;

			if (!string.IsNullOrEmpty( textBox_router.Text ))
			{
				allenBradleyNet.MessageRouter = new MessageRouter( textBox_router.Text );
			}

			//if (!string.IsNullOrEmpty( textBox16.Text ))
			//{
			//	allenBradleyNet.PortSlot = HslCommunication.BasicFramework.SoftBasic.HexStringToBytes( textBox16.Text );
			//}

			try
			{
				OperateResult connect = allenBradleyNet.ConnectServer( );
				if (connect.IsSuccess)
				{
					MessageBox.Show( HslCommunication.StringResources.Language.ConnectedSuccess );
					button2.Enabled = true;
					button1.Enabled = false;
					panel2.Enabled = true;

					// 设置子控件的读取能力
					userControlReadWriteDevice1.SetReadWriteNet( allenBradleyNet, "A1", true, 1 );
					// 设置批量读取
					userControlReadWriteDevice1.BatchRead.SetReadWriteNet( allenBradleyNet, "A1", string.Empty );
					userControlReadWriteDevice1.BatchRead.SetReadRandom( allenBradleyNet.Read, "A1;A2    Length input \"1;1\"" );
					// 设置报文读取
					userControlReadWriteDevice1.MessageRead.SetReadSourceBytes( m => allenBradleyNet.ReadFromCoreServer( m, true, false ), string.Empty, string.Empty );
					userControlReadWriteDevice1.MessageRead.SetReadSourceBytes( m => allenBradleyNet.ReadEipFromServer( m ), "EIP", "EIP Message, example: " );
					userControlReadWriteDevice1.MessageRead.SetReadSourceBytes( m => allenBradleyNet.ReadCipFromServer( m ), "CIP", "CIP Message, example: " );

					// TODO EIP及CIP的例子填充
					control.SetDevice( allenBradleyNet, "A1" );

					// 设置代码示例
					if (string.IsNullOrEmpty( textBox_router.Text ))
						codeExampleControl.SetCodeText( allenBradleyNet, nameof( allenBradleyNet.Slot ) );
					else
						codeExampleControl.SetCodeText( allenBradleyNet, nameof( allenBradleyNet.Slot ), nameof( allenBradleyNet.MessageRouter ) );
				}
				else
				{
					MessageBox.Show( HslCommunication.StringResources.Language.ConnectedFailed + connect.ToMessageShowString( ) );
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
			allenBradleyNet.ConnectClose( );
			button2.Enabled = false;
			button1.Enabled = true;
			panel2.Enabled = false;
		}


		#endregion


		public override void SaveXmlParameter( XElement element )
		{
			element.SetAttributeValue( DemoDeviceList.XmlIpAddress, textBox1.Text );
			element.SetAttributeValue( DemoDeviceList.XmlPort, textBox2.Text );
			element.SetAttributeValue( DemoDeviceList.XmlSlot, textBox15.Text );


			this.userControlReadWriteDevice1.GetDataTable( element );
		}

		public override void LoadXmlParameter( XElement element )
		{
			base.LoadXmlParameter( element );
			textBox1.Text = element.Attribute( DemoDeviceList.XmlIpAddress ).Value;
			textBox2.Text = element.Attribute( DemoDeviceList.XmlPort ).Value;
			textBox15.Text = element.Attribute( DemoDeviceList.XmlSlot ).Value;


			if (this.userControlReadWriteDevice1.LoadDataTable( element ) > 0)
				this.userControlReadWriteDevice1.SelectTabDataTable( );
		}

		private void userControlHead1_SaveConnectEvent_1( object sender, EventArgs e )
		{
			userControlHead1_SaveConnectEvent( sender, e );
		}

	}
}
