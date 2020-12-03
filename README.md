C# Console application demonstrating how to pull data from Companies House Streaming API (see https://developer-specs.company-information.service.gov.uk/streaming-api/guides/overview).

Requires the addition of an App.config XML file with one setting APIKey (obtained by registering an application per the link above).

<configuration>
  <appSettings>
    <add key="APIKey" value="" />
  </appSettings>
</configuration>

The app is set to consume the basic company information stream.

The app will accept command line argument, when present and valid it will be used as a timepoint to allow a degree of recent historic change to be pulled. On cancelling stream through CTRL+C the timepoint of the last record will be returned.

See the Companies House documentation for the limitations around timepoint and issues around rate limiting & requirement for a back-off strategy (in event of repeated connection attempts).

Standing on the shoulders of giants...

The code for cancellation was adapted from here;
https://stackoverflow.com/questions/48222797/capturing-cancelkeypress-to-stop-an-async-console-app-at-a-safe-point
EncodeTo64 I found here;
https://arcanecode.com/2007/03/21/encoding-strings-to-base64-in-c/
The code around getting the response (so you can trap the errors) I adapted from here;
https://stackoverflow.com/questions/30163316/httpclient-getstreamasync-and-http-status-codes