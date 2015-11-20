using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Data.Xml.Rpc;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace StateNameUniversalClient
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainPage : Page
	{
		public MainPage()
		{
			this.InitializeComponent();
		}

		private async void OnButtonClick(object sender, RoutedEventArgs e)
		{
			Output.Visibility = Visibility.Collapsed;
			Error.Visibility = Visibility.Collapsed;
			int number;
			bool ok = Int32.TryParse(InputNumber.Text, out number);
			if (!ok)
			{
				Error.Text = "Enter a number";
				Error.Visibility = Visibility.Visible;
				return;
			}
			var proxy = new StateNameProxy();

			try
			{
				var result = await proxy.GetName(number);
				StateName.Text = result;
				StateNumber.Text = number.ToString();
				Output.Visibility = Visibility.Visible;
			}
			catch (XmlRpcFaultException fex)
			{
				Error.Text = "[" + fex.FaultCode.ToString() + "] " + fex.FaultString;
				Error.Visibility = Visibility.Visible;
			}
			catch (Exception ex)
			{
				Error.Text = ex.Message;
				Error.Visibility = Visibility.Visible;
			}

		}

		private void OnInputNumberChanged(object sender, TextChangedEventArgs e)
		{
			Error.Visibility = Visibility.Collapsed;
		}


	}




	[XmlRpcUrl("http://www.cookcomputing.com/xmlrpcsamples/RPC2.ashx")]
	public class StateNameProxy : XmlRpcClientProtocol
	{
		[XmlRpcBegin("examples.getStateName")]
		public async Task<String> GetName(int number)
		{
			var methodInfo = GetType().GetTypeInfo().GetDeclaredMethod("GetName");
			var result = await this.InvokeAsync<String>(methodInfo, new object[] { number });
			return result.ToString();
		}



	}
}
