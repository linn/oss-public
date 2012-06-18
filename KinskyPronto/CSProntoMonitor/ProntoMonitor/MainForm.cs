/*
 * Created by SharpDevelop.
 * User: graham
 * Date: 25/03/2010
 * Time: 21:53
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using System.IO;
using System.Net;  
using System.Net.Sockets;
using System.Threading;


namespace ProntoMonitor
{
	public partial class MainForm : Form
	{
		private IPAddress[] iIpList;
		private TcpComms    iTcpComms = null;
		private bool        iAcceptIp = false;
		
		public MainForm()
		{
			InitializeComponent();
			
			iTextBoxOutput.AppendText( "Starting Monitor\r\n" );
			this.GetIpList();

            // Populate interface list or just use only available interface			
			if( iIpList.Length > 1 )
			{
			    iComboBoxIp.Items.Add( "<please select>" );
			    for( int i=0; i<iIpList.Length; i++ )
			    {
    			    iComboBoxIp.Items.Add( iIpList[i].ToString() );
    			}
			    iComboBoxIp.SelectedIndex = 0;
			    iAcceptIp = true;
			}
			else
			{
			    iComboBoxIp.Items.Add( iIpList[0].ToString() );
			    iAcceptIp = true;
			    iComboBoxIp.SelectedIndex = 0;
			}			
		} 		
		
		// delegate is used to communicate with UI Thread from a non-UI thread
        public delegate void UpdateTextBoxCb( string message );
						
		private void GetIpList()
		{
			string      hostName  = Dns.GetHostName();
			IPHostEntry hostEntry = Dns.GetHostEntry( hostName );
          	iIpList = hostEntry.AddressList;
        }
        
        private void StartComms( string aIp, int aPort )
        {
            // Callback after interface selected - start TCP server on its own thread
			iTcpComms = new TcpComms( aIp, aPort, iTextBoxOutput );
			Thread commsThread = new Thread( new ThreadStart( iTcpComms.Run ));
			commsThread.Start();
	    }   
	    		
		private void OnButtonClearClick(object sender, System.EventArgs e)
		{
			iTextBoxOutput.Clear();
		}
		
		private void OnComboBoxIpChange(object sender, System.EventArgs e)
		{
		    if( iAcceptIp )
		    {
                System.Windows.Forms.ComboBox comboBox = (System.Windows.Forms.ComboBox) sender;
		        this.StartComms( (string)comboBox.SelectedItem, 23 );
		        comboBox.Hide();
		        iLabelIpValue.Text = (string)comboBox.SelectedItem;
		        iLabelIpValue.Visible = true;
		        iAcceptIp = false;
		    }
		}
	    
		private void Shutdown(object sender, FormClosingEventArgs e)
		{
		    // Attempt to cleanly shutdown the TCP server
		    if( iTcpComms != null )
		    {
		        iTcpComms.Shutdown();
		    }
		}
	}
	
	
	public class TcpComms
	{
		private TextBox     iOutput;
		private TcpListener iListener;
		private Thread      iCommsThread;
		
		public TcpComms( string aIp, int aPort, TextBox aOutput )
		{
			iOutput  = aOutput;
			iListener = new TcpListener( IPAddress.Parse( aIp ), aPort );
			this.Output( "Serving on " + aIp + ":" + aPort + "\r\n" );
		}
		        
		public void Run()
		{
		    // Listen for and accept incoming TCP clients then start thread to handle incoming data
            iListener.Start();
            while( true )
            {
                try
                {
                    TcpClient client = iListener.AcceptTcpClient();		
                    iCommsThread = new Thread( new ParameterizedThreadStart( this.HandleComms ));
                    iCommsThread.Start( client );
                }
                catch
                {
                    // Exit loop on error (or shutdown)
                    break;
                }
            }                
        }
        
        public void Shutdown()
        {
            // Shutdown the TCP server
            iListener.Stop();
            try
            {
                iCommsThread.Abort();
            }
            catch
            {
            }
        }
        
        private void HandleComms( object tcpClient )
        {
            // Receive incoming data from client and display on form
            TcpClient       client = (TcpClient)tcpClient;        			
			NetworkStream   stream = client.GetStream();			
            byte[]          message = new byte[16384];
            int             bytesRead;
			this.Output( "Connected\r\n\r\n" );
			
            while( true )
            {
                bytesRead = 0;
                try
                {
                    bytesRead = stream.Read( message, 0, 16384 );
                }
                catch
                {
                    // Exit loop on error (or shutdown)
                    break;
                }

                if (bytesRead == 0)
                {
			        this.Output( "\r\n\r\nClient disconnected\r\n" );
                    break;
                }
    			this.Output( System.Text.ASCIIEncoding.ASCII.GetString( message, 0, bytesRead ) );
            }
            client.Close();
        }			
		
		private void Output( string message )
		{
			iOutput.BeginInvoke( new MainForm.UpdateTextBoxCb( UpdateOutput ), message );
		}
		
		private void UpdateOutput( string message )
        {
            iOutput.AppendText( message );
        }		
	}
}
