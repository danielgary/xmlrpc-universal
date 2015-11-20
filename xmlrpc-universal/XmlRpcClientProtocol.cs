/* 
XML-RPC.NET library
Copyright (c) 2001-2011, Charles Cook <charlescook@cookcomputing.com>

Permission is hereby granted, free of charge, to any person 
obtaining a copy of this software and associated documentation 
files (the "Software"), to deal in the Software without restriction, 
including without limitation the rights to use, copy, modify, merge, 
publish, distribute, sublicense, and/or sell copies of the Software, 
and to permit persons to whom the Software is furnished to do so, 
subject to the following conditions:

The above copyright notice and this permission notice shall be 
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
DEALINGS IN THE SOFTWARE.
*/

using System;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
//using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;
using System.Runtime.CompilerServices;

namespace Windows.Data.Xml.Rpc
{
	public class XmlRpcClientProtocol :
#if (!SILVERLIGHT && !WINDOWS_UWP)
    Component,
#endif
	IXmlRpcProxy
	{
		WebSettings webSettings = new WebSettings();
		XmlRpcFormatSettings XmlRpcFormatSettings = new XmlRpcFormatSettings();

		private CookieCollection _responseCookies;
		private WebHeaderCollection _responseHeaders;
		private XmlRpcNonStandard _nonStandard;
		private string _url = null;
		private string _xmlRpcMethod = null;

		private Guid _id = Util.NewGuid();

		static XmlRpcClientProtocol()
		{
#if (SILVERLIGHT)
      WebRequest.RegisterPrefix("http://", System.Net.Browser.WebRequestCreator.ClientHttp);
      WebRequest.RegisterPrefix("https://", System.Net.Browser.WebRequestCreator.ClientHttp);
#endif
		}

#if (!COMPACT_FRAMEWORK && !SILVERLIGHT && !WINDOWS_UWP)
    public XmlRpcClientProtocol(System.ComponentModel.IContainer container)
    {
      container.Add(this);
      InitializeComponent();
    }
#endif
		public XmlRpcClientProtocol()
		{

			httpFilter = new Web.Http.Filters.HttpBaseProtocolFilter();
			httpClient = new HttpClient(httpFilter);

			InitializeComponent();
		}

		private HttpClient httpClient;



		private async Task<T> InvokeAsync<T>(
		  MethodInfo mi,
		  params object[] Parameters)
		{
			return await InvokeAsync<T>(this, mi, Parameters);
		}

		public async Task<T> InvokeAsync<T>(
		   object[] Parameters,
			[CallerMemberName] string methodName = "")
		{
			var methodInfo = GetType().GetTypeInfo().GetDeclaredMethod(methodName);
			return await InvokeAsync<T>(methodInfo, Parameters);
		}



		public async Task<T> InvokeAsync<T>(
		  Object clientObj,
		  string methodName,
		  params object[] parameters)
		{
			MethodInfo mi = GetMethodInfoFromName(clientObj, methodName, parameters);
			return await InvokeAsync<T>(this, mi, parameters);
		}

		public async Task<T> InvokeAsync<T>(
		  Object clientObj,
		  MethodInfo mi,
		  params object[] parameters)
		{
#if (SILVERLIGHT)
      throw new NotSupportedException();
#else


			try
			{
				string useUrl = GetEffectiveUrl(clientObj);


				HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, new Uri(useUrl));



				XmlRpcRequest req = MakeXmlRpcRequest2(message, mi, parameters,
				  clientObj, _xmlRpcMethod, _id);
				SetProperties2(message);
				SetRequestHeaders2(Headers, message);
#if (!COMPACT_FRAMEWORK && !SILVERLIGHT && !WINDOWS_UWP)
				SetClientCertificates(ClientCertificates, webReq);
#endif

				bool logging = (RequestEvent != null);





				try
				{
					var serializer = new XmlRpcRequestSerializer(XmlRpcFormatSettings);
					var content = serializer.SerializeRequestToString(req);
					message.Content = new HttpStringContent(content);



					var httpResponse = await httpClient.SendRequestAsync(message);
					var rpcResponse = await ReadResponseAsync(req, httpResponse);
					return (T)rpcResponse.retVal;


				}
				finally
				{

				}




			}
			finally
			{

			}

#endif
		}

		public bool AllowAutoRedirect
		{
			get { return webSettings.AllowAutoRedirect; }
			set { webSettings.AllowAutoRedirect = value; }
		}

#if (!COMPACT_FRAMEWORK && !SILVERLIGHT && !WINDOWS_UWP)
		[Browsable(false)]
		public X509CertificateCollection ClientCertificates
		{
			get { return webSettings.ClientCertificates; }
		}
#endif

#if (!COMPACT_FRAMEWORK)
		public string ConnectionGroupName
		{
			get { return webSettings.ConnectionGroupName; }
			set { webSettings.ConnectionGroupName = value; }
		}
#endif

#if !WINDOWS_UWP
		[Browsable(false)]
#endif
		public ICredentials Credentials
		{
			get { return webSettings.Credentials; }
			set { webSettings.Credentials = value; }
		}

#if (!COMPACT_FRAMEWORK && !FX1_0)
		public bool EnableCompression
		{
			get { return webSettings.EnableCompression; }
			set { webSettings.EnableCompression = value; }
		}
#endif

#if !WINDOWS_UWP
		[Browsable(false)]
#endif
		public WebHeaderCollection Headers
		{
			get { return webSettings.Headers; }
		}

#if (!COMPACT_FRAMEWORK && !FX1_0 && !SILVERLIGHT)
		public bool Expect100Continue
		{
			get { return webSettings.Expect100Continue; }
			set { webSettings.Expect100Continue = value; }
		}

		public bool UseNagleAlgorithm
		{
			get { return webSettings.UseNagleAlgorithm; }
			set { webSettings.UseNagleAlgorithm = value; }
		}
#endif

		public global::Windows.Web.Http.Filters.HttpBaseProtocolFilter httpFilter { get; set; }
		public HttpCookieManager CookieManager
		{
			get
			{
				return httpFilter.CookieManager;
			}
		}

		public CookieContainer CookieContainer
		{
			get { return webSettings.CookieContainer; }
		}

		public Guid Id
		{
			get { return _id; }
		}

		public int Indentation
		{
			get { return XmlRpcFormatSettings.Indentation; }
			set { XmlRpcFormatSettings.Indentation = value; }
		}

		public bool KeepAlive
		{
			get { return webSettings.KeepAlive; }
			set { webSettings.KeepAlive = value; }
		}

		public XmlRpcNonStandard NonStandard
		{
			get { return _nonStandard; }
			set { _nonStandard = value; }
		}

		public bool PreAuthenticate
		{
			get { return webSettings.PreAuthenticate; }
			set { webSettings.PreAuthenticate = value; }
		}

#if (!SILVERLIGHT)
#if !WINDOWS_UWP
		[Browsable(false)]
#endif
		public HttpVersion ProtocolVersion
		{
			get { return webSettings.ProtocolVersion; }
			set { webSettings.ProtocolVersion = value; }
		}
#endif

#if (!SILVERLIGHT)
#if !WINDOWS_UWP
		[Browsable(false)]
#endif
		public IWebProxy Proxy
		{
			get { return webSettings.Proxy; }
			set { webSettings.Proxy = value; }
		}
#endif

		public CookieCollection ResponseCookies
		{
			get { return _responseCookies; }
		}

		public WebHeaderCollection ResponseHeaders
		{
			get { return _responseHeaders; }
		}

		public int Timeout
		{
			get { return webSettings.Timeout; }
			set { webSettings.Timeout = value; }
		}

		public string Url
		{
			get { return _url; }
			set { _url = value; }
		}

		public bool UseEmptyElementTags
		{
			get { return XmlRpcFormatSettings.UseEmptyElementTags; }
			set { XmlRpcFormatSettings.UseEmptyElementTags = value; }
		}

		public bool UseEmptyParamsTag
		{
			get { return XmlRpcFormatSettings.UseEmptyParamsTag; }
			set { XmlRpcFormatSettings.UseEmptyParamsTag = value; }
		}

		public bool UseIndentation
		{
			get { return XmlRpcFormatSettings.UseIndentation; }
			set { XmlRpcFormatSettings.UseIndentation = value; }
		}

		public bool UseIntTag
		{
			get { return XmlRpcFormatSettings.UseIntTag; }
			set { XmlRpcFormatSettings.UseIntTag = value; }
		}

		public string UserAgent
		{
			get { return webSettings.UserAgent; }
			set { webSettings.UserAgent = value; }
		}

		public bool UseStringTag
		{
			get { return XmlRpcFormatSettings.UseStringTag; }
			set { XmlRpcFormatSettings.UseStringTag = value; }
		}

#if !WINDOWS_UWP
		[Browsable(false)]
#endif
		public Encoding XmlEncoding
		{
			get { return XmlRpcFormatSettings.XmlEncoding; }
			set { XmlRpcFormatSettings.XmlEncoding = value; }
		}

		public string XmlRpcMethod
		{
			get { return _xmlRpcMethod; }
			set { _xmlRpcMethod = value; }
		}

		public void SetProperties2(HttpRequestMessage message)
		{
			message.Headers.TryAppendWithoutValidation("User-Agent", UserAgent);
			message.Headers.TryAppendWithoutValidation("Keep-Alive", KeepAlive.ToString());

		}

		public void SetProperties(WebRequest webReq)
		{
#if (!SILVERLIGHT)
			if (Proxy != null)
				webReq.Proxy = Proxy;
#endif
			HttpWebRequest httpReq = (HttpWebRequest)webReq;

#if (!SILVERLIGHT)
#if WINDOWS_UWP
			httpReq.Headers["User-Agent"] = UserAgent;
			httpReq.Headers["Keep-Alive"] = KeepAlive.ToString();
			//httpReq.Headers["Accept-Encoding"] = "gzip,deflate";



#else
			httpReq.UserAgent = UserAgent;
			httpReq.ProtocolVersion = ProtocolVersion;
			httpReq.KeepAlive = KeepAlive;
			httpReq.AllowAutoRedirect = AllowAutoRedirect;
			webReq.PreAuthenticate = PreAuthenticate;
			webReq.Timeout = Timeout;
			// Compact Framework sets this to false by default
			(webReq as HttpWebRequest).AllowWriteStreamBuffering = true;
#endif
#endif
			httpReq.CookieContainer = CookieContainer;
#if (!COMPACT_FRAMEWORK && !FX1_0 && !SILVERLIGHT && !WINDOWS_UWP)
			httpReq.ServicePoint.Expect100Continue = Expect100Continue;
			httpReq.ServicePoint.UseNagleAlgorithm = UseNagleAlgorithm;
#endif
#if (!COMPACT_FRAMEWORK && !SILVERLIGHT && !WINDOWS_UWP)
			webReq.ConnectionGroupName = ConnectionGroupName;
#endif
#if (!SILVERLIGHT)
			webReq.Credentials = Credentials;
#else
      webReq.Credentials = Credentials;
      webReq.UseDefaultCredentials = false;
#endif
#if (!COMPACT_FRAMEWORK && !FX1_0 && !SILVERLIGHT && !WINDOWS_UWP)
			if (EnableCompression)
				webReq.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
#endif
		}

		private void SetRequestHeaders(
		  WebHeaderCollection headers,
		  WebRequest webReq)
		{
			foreach (string key in headers)
			{
#if (!SILVERLIGHT)
				webReq.Headers[key] = headers[key];
#endif
			}
		}


		private void SetRequestHeaders2(WebHeaderCollection headers, HttpRequestMessage message)
		{
			foreach (string key in headers)
			{
				message.Headers.TryAppendWithoutValidation(key, headers[key]);
			}
		}

#if (!COMPACT_FRAMEWORK && !SILVERLIGHT && !WINDOWS_UWP)
		private void SetClientCertificates(
		  X509CertificateCollection certificates,
		  WebRequest webReq)
		{
			foreach (X509Certificate certificate in certificates)
			{
				HttpWebRequest httpReq = (HttpWebRequest)webReq;
				httpReq.ClientCertificates.Add(certificate);
			}
		}
#endif
		XmlRpcRequest MakeXmlRpcRequest(WebRequest webReq, MethodInfo mi,
		  object[] parameters, object clientObj, string xmlRpcMethod,
		  Guid proxyId)
		{
			webReq.Method = "POST";
			webReq.ContentType = "text/xml";
			string rpcMethodName = XmlRpcTypeInfo.GetRpcMethodName(mi);
			XmlRpcRequest req = new XmlRpcRequest(rpcMethodName, parameters, mi,
			  xmlRpcMethod, proxyId);
			return req;
		}


		XmlRpcRequest MakeXmlRpcRequest2(HttpRequestMessage message, MethodInfo mi, object[] parameters, object clientObj, string xmlRpcMethod, Guid proxyId)
		{
			message.Method = HttpMethod.Post;
			message.Headers.TryAppendWithoutValidation("Content-Type", "text/xml");
			string rpcMethodName = XmlRpcTypeInfo.GetRpcMethodName(mi);
			XmlRpcRequest req = new XmlRpcRequest(rpcMethodName, parameters, mi,
			  xmlRpcMethod, proxyId);
			return req;
		}

		XmlRpcResponse ReadResponse(
		  XmlRpcRequest req,
		  WebResponse webResp,
		  Stream respStm)
		{
			HttpWebResponse httpResp = (HttpWebResponse)webResp;
			if (httpResp.StatusCode != global::System.Net.HttpStatusCode.OK)
			{
				// status 400 is used for errors caused by the client
				// status 500 is used for server errors (not server application
				// errors which are returned as fault responses)
				if (httpResp.StatusCode == global::System.Net.HttpStatusCode.BadRequest)
					throw new XmlRpcException(httpResp.StatusDescription);
				else
					throw new XmlRpcServerException(httpResp.StatusDescription);
			}
			var deserializer = new XmlRpcResponseDeserializer();
			deserializer.NonStandard = _nonStandard;
			Type retType = req.mi.ReturnType;
			XmlRpcResponse xmlRpcResp
			  = deserializer.DeserializeResponse(respStm, req.ReturnType);
			return xmlRpcResp;
		}


		async Task<XmlRpcResponse> ReadResponseAsync(
		  XmlRpcRequest req,
		  HttpResponseMessage webResp)
		{
			//HttpWebResponse httpResp = (HttpWebResponse)webResp;
			if (!webResp.IsSuccessStatusCode)
			{
				// status 400 is used for errors caused by the client
				// status 500 is used for server errors (not server application
				// errors which are returned as fault responses)

				throw new XmlRpcServerException(webResp.StatusCode.ToString());
			}
			var deserializer = new XmlRpcResponseDeserializer();

			var stream = await webResp.Content.ReadAsInputStreamAsync();


			deserializer.NonStandard = _nonStandard;
			Type retType = req.mi.ReturnType;
			if (retType.IsConstructedGenericType)
				retType = retType.GenericTypeArguments[0];
			XmlRpcResponse xmlRpcResp
			  = deserializer.DeserializeResponse(stream.AsStreamForRead(), retType);
			return xmlRpcResp;
		}


		MethodInfo GetMethodInfoFromName(object clientObj, string methodName,
		  object[] parameters)
		{
			Type[] paramTypes = new Type[0];
			if (parameters != null)
			{
				paramTypes = new Type[parameters.Length];
				for (int i = 0; i < paramTypes.Length; i++)
				{
					if (parameters[i] == null)
						throw new XmlRpcNullParameterException("Null parameters are invalid");
					paramTypes[i] = parameters[i].GetType();
				}
			}
			Type type = clientObj.GetType();
			MethodInfo mi = type.GetMethod(methodName, paramTypes);
			if (mi == null)
			{
				try
				{
					mi = type.GetMethod(methodName);
				}
				catch (global::System.Reflection.AmbiguousMatchException)
				{
					throw new XmlRpcInvalidParametersException("Method parameters match "
					  + "the signature of more than one method");
				}
				if (mi == null)
					throw new Exception(
					  "Invoke on non-existent or non-public proxy method");
				else
					throw new XmlRpcInvalidParametersException("Method parameters do "
					  + "not match signature of any method called " + methodName);
			}
			return mi;
		}

		public object EndInvoke(IAsyncResult asr, Type returnType)
		{
			object reto = null;
			Stream responseStream = null;
			try
			{
				XmlRpcAsyncResult clientResult = (XmlRpcAsyncResult)asr;
				if (clientResult.Exception != null)
					throw clientResult.Exception;
				if (clientResult.EndSendCalled)
					throw new Exception("dup call to EndSend");
				clientResult.EndSendCalled = true;
				if (clientResult.XmlRpcRequest != null && returnType != null)
					clientResult.XmlRpcRequest.ReturnType = returnType;
				HttpWebResponse webResp = (HttpWebResponse)clientResult.WaitForResponse();
				clientResult._responseCookies = webResp.Cookies;
				clientResult._responseHeaders = webResp.Headers;
				responseStream = clientResult.ResponseBufferedStream;
				if (ResponseEvent != null)
				{
					OnResponse(new XmlRpcResponseEventArgs(
					  clientResult.XmlRpcRequest.proxyId,
					  clientResult.XmlRpcRequest.number,
					  responseStream));
					responseStream.Position = 0;
				}
#if (!COMPACT_FRAMEWORK && !FX1_0 && !SILVERLIGHT && !WINDOWS_UWP)
				responseStream = MaybeDecompressStream((HttpWebResponse)webResp,
				  responseStream);
#endif
				XmlRpcResponse resp = ReadResponse(clientResult.XmlRpcRequest,
				  webResp, responseStream);
				reto = resp.retVal;
			}
			finally
			{
				if (responseStream != null)
					responseStream.Dispose();
			}
			return reto;
		}

		string GetEffectiveUrl(object clientObj)
		{
			// client can either have define URI in attribute or have set it
			// via proxy's ServiceURI property - but must exist by now
			if (!string.IsNullOrEmpty(Url))
				return Url;
			string useUrl = XmlRpcTypeInfo.GetUrlFromAttribute(clientObj.GetType());
			if (!string.IsNullOrEmpty(useUrl))
				return useUrl;
			throw new XmlRpcMissingUrl("Proxy XmlRpcUrl attribute or Url "
			  + "property not set.");
		}

		[XmlRpcMethod("system.listMethods")]
		public async Task<string[]> SystemListMethodsAsync()
		{
			return (string[])await InvokeAsync<string[]>(new Object[0]);
		}



		[XmlRpcMethod("system.methodSignature")]
		public async Task<object[]> SystemMethodSignatureAsync(string MethodName)
		{
			return (object[])await InvokeAsync<object[]>(
			  new Object[] { MethodName });
		}



		[XmlRpcMethod("system.methodHelp")]
		public async Task<string> SystemMethodHelpAsync(string MethodName)
		{
			return (string)await InvokeAsync<string>(
			  new Object[] { MethodName });
		}

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
		}

		protected virtual WebRequest GetWebRequest(Uri uri)
		{
			WebRequest req = WebRequest.Create(uri);
			return req;
		}


#if (!COMPACT_FRAMEWORK && !FX1_0 && !SILVERLIGHT && !WINDOWS_UWP)
		// support for gzip and deflate
		protected Stream MaybeDecompressStream(HttpWebResponse httpWebResp,
		  Stream respStream)
		{
			Stream decodedStream;

			string contentEncoding = httpWebResp.ContentEncoding.ToLower();
			string coen = httpWebResp.Headers["Content-Encoding"];
			if (contentEncoding.Contains("gzip"))
			{
				decodedStream = new System.IO.Compression.GZipStream(respStream,
				  System.IO.Compression.CompressionMode.Decompress);
			}
			else if (contentEncoding.Contains("deflate"))
			{
				decodedStream = new System.IO.Compression.DeflateStream(respStream,
				  System.IO.Compression.CompressionMode.Decompress);
			}
			else
				decodedStream = respStream;
			return decodedStream;
		}
#endif

		protected virtual WebResponse GetWebResponse(WebRequest request,
		  IAsyncResult result)
		{
			return request.EndGetResponse(result);
		}


		public void AttachLogger(XmlRpcLogger logger)
		{
			logger.SubscribeTo(this);
		}

		public event XmlRpcRequestEventHandler RequestEvent;
		public event XmlRpcResponseEventHandler ResponseEvent;

		protected virtual void OnRequest(XmlRpcRequestEventArgs e)
		{
			if (RequestEvent != null)
			{
				RequestEvent(this, e);
			}
		}


		protected virtual void OnResponse(XmlRpcResponseEventArgs e)
		{
			if (ResponseEvent != null)
			{
				ResponseEvent(this, e);
			}
		}


	}

	//#if (COMPACT_FRAMEWORK && )
	//  // dummy attribute because System.ComponentModel.Browsable is not
	//  // support in the compact framework
	//  [AttributeUsage(AttributeTargets.Property)]
	//  public class BrowsableAttribute : Attribute
	//  {
	//    public BrowsableAttribute(bool dummy)
	//    {
	//    }
	//  }
	//#endif

	public delegate void XmlRpcRequestEventHandler(object sender,
	  XmlRpcRequestEventArgs args);

	public delegate void XmlRpcResponseEventHandler(object sender,
	  XmlRpcResponseEventArgs args);
}


