﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HslCommunication.Profinet;
using HslCommunication.Profinet.Melsec;
using System.Threading;
using HslCommunication;
using System.Xml.Linq;
using HslCommunicationDemo.DemoControl;
using HslCommunicationDemo.PLC.Melsec;

namespace HslCommunicationDemo
{
	public partial class FormMelsecAsciiUdp : HslFormContent
	{
		public FormMelsecAsciiUdp( )
		{
			InitializeComponent( );
			melsec_net = new MelsecMcAsciiUdp( );
		}


		private MelsecMcAsciiUdp melsec_net = null;
		private McQna3EControl control;
		private AddressExampleControl addressExampleControl;
		private CodeExampleControl codeExampleControl;

		private void FormSiemens_Load( object sender, EventArgs e )
		{
			panel2.Enabled = false;
			Language( Program.Language );


			userControlReadWriteDevice1.AddSpecialFunctionTab( control );
			addressExampleControl = new AddressExampleControl( );
			addressExampleControl.SetAddressExample( Helper.GetMcAddress( ) );
			userControlReadWriteDevice1.AddSpecialFunctionTab( addressExampleControl, false, DeviceAddressExample.GetTitle( ) );

			codeExampleControl = new CodeExampleControl( );
			userControlReadWriteDevice1.AddSpecialFunctionTab( codeExampleControl, false, CodeExampleControl.GetTitle( ) );
		}


		private void Language( int language )
		{
			if (language == 2)
			{
				Text = "Melsec Read PLC Demo";

				label1.Text = "Ip:";
				label3.Text = "Port:";
				button1.Text = "Connect";
				button2.Text = "Disconnect";
				checkBox_string_reverse.Text = "string reverse by word";
			}
			else
			{
				checkBox_EnableWriteBitToWordRegister.Text = "支持使用位写入字寄存器(实际读字，修改位，写字)";
			}
		}


		private void FormSiemens_FormClosing( object sender, FormClosingEventArgs e )
		{

		}


		#region Connect And Close



		private void button1_Click( object sender, EventArgs e )
		{
			melsec_net.IpAddress = textBox1.Text;
			if (!int.TryParse( textBox2.Text, out int port ))
			{
				MessageBox.Show( DemoUtils.PortInputWrong );
				return;
			}

			melsec_net.LogNet = LogNet;
			melsec_net.Port = port;
			melsec_net.EnableWriteBitToWordRegister = checkBox_EnableWriteBitToWordRegister.Checked;
			melsec_net.ByteTransform.IsStringReverseByteWord = checkBox_string_reverse.Checked;

			button2.Enabled = true;
			button1.Enabled = false;
			panel2.Enabled = true;

			// 设置子控件的读取能力
			userControlReadWriteDevice1.SetReadWriteNet( melsec_net, "D100", false );
			// 设置批量读取
			userControlReadWriteDevice1.BatchRead.SetReadWriteNet( melsec_net, "D100", string.Empty );
			userControlReadWriteDevice1.BatchRead.SetReadRandom( melsec_net.ReadRandom, "D100;W100;D500" );
			userControlReadWriteDevice1.BatchRead.SetReadWordRandom( melsec_net.ReadRandom, "D100;W100;D500" );
			// 设置报文读取
			userControlReadWriteDevice1.MessageRead.SetReadSourceBytes( m => melsec_net.ReadFromCoreServer( m, true, false ), string.Empty, string.Empty );
			// 特殊读取
			control.SetDevice( melsec_net, "D100" );

			codeExampleControl.SetCodeText( melsec_net, nameof( MelsecMcUdp.EnableWriteBitToWordRegister ), "ByteTransform.IsStringReverseByteWord" );
		}

		private void button2_Click( object sender, EventArgs e )
		{
			// 断开连接
			button2.Enabled = false;
			button1.Enabled = true;
			panel2.Enabled = false;
		}
		
		#endregion

		public override void SaveXmlParameter( XElement element )
		{
			element.SetAttributeValue( DemoDeviceList.XmlIpAddress, textBox1.Text );
			element.SetAttributeValue( DemoDeviceList.XmlPort, textBox2.Text );
			element.SetAttributeValue( "EnableWriteBitToWordRegister", checkBox_EnableWriteBitToWordRegister.Text );
			element.SetAttributeValue( "IsStringReverseByteWord", checkBox_string_reverse.Checked );

			this.userControlReadWriteDevice1.GetDataTable( element );
		}

		public override void LoadXmlParameter( XElement element )
		{
			base.LoadXmlParameter( element );
			textBox1.Text = element.Attribute( DemoDeviceList.XmlIpAddress ).Value;
			textBox2.Text = element.Attribute( DemoDeviceList.XmlPort ).Value;
			checkBox_EnableWriteBitToWordRegister.Checked = GetXmlValue( element, "EnableWriteBitToWordRegister", false, bool.Parse );
			checkBox_string_reverse.Checked = GetXmlValue( element, "IsStringReverseByteWord", false, bool.Parse );

			if (this.userControlReadWriteDevice1.LoadDataTable( element ) > 0)
				this.userControlReadWriteDevice1.SelectTabDataTable( );
		}

		private void userControlHead1_SaveConnectEvent_1( object sender, EventArgs e )
		{
			userControlHead1_SaveConnectEvent( sender, e );

		}
	}
}
