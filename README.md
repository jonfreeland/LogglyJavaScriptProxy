# LogglyJavaScriptProxy
Proxies requests from Loggly's JavaScript client in an ASP.NET MVC application to help protect the token.

Make sure attribute routes are enabled by adding the following, most likely in `Application_Start()`:
```csharp
RouteTable.Routes.MapMvcAttributeRoutes();
```
Also, by default, the Loggly token is expected to be in the `Web.config`:
```xml
<configuration>
  ...
  <appSettings>
    ...
    <add key="LogglyToken" value="YourLogglyTokenHere"/>
  </appSettings>
<configuration>
```
Follow [Loggly's documentation](https://github.com/loggly/loggly-jslogger#setup-proxy-for-ad-blockers) regarding using a proxy:
```javascript
_LTracker.push({
  'logglyKey': 'your-customer-token',
  'sendConsoleErrors' : true,
  'tag' : 'javascript-logs',
  'useDomainProxy' : true
});
```
Remember, don't set the Loggly token in JavaScript!
