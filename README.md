Based on XMLRPC dot NET by Charles Cook
Source: http://xml-rpc.net/ and https://code.google.com/p/xmlrpcnet/

XMLRPC-Universal is a library for consuming XML RPC services from a Windows UWP application.  For each web service, you should create a Proxy class that looks like this:

```cs
    using Windows.Data.Xml.Rpc;

    [XmlRpcUrl("http://www.cookcomputing.com/xmlrpcsamples/RPC2.ashx")]
    class TestProxy : XmlRpcClientProtocol
    {
        //Declare async RPC methods here
    }
```


To implement the RPC methods, the declarations should look like this:

```cs
    [XmlRpcMethod("examples.getStateName")]
    public async Task<String> GetStateName(int stateIndex)
    {
        //You must pass your parameters as an object array.
        return await InvokeAsync<String>(new object[] { number });
        
    }
```

There is a sample application included.
