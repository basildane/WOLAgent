# WOLAgent
Wake On LAN Agent.

Aquila WOL is designed for Wake-On-LAN over Internet.
WOL Agent is an optional (free) component that can be used with WOL to help with receiving wakeup messages over the Internet.
The challenge is usually that your firewall or router will not route broadcast-packets from the Internet to your internal LAN.
The WOL-Agent solves this problem.  The agent is a small service that you install on any available Windows server on your LAN.
It listens for WOL packets and then translates them into broadcast packets and sends them back into your LAN.
This solves the problem of firewalls and routers that cannot forward broadcast type messages.

By default the application forwards port 9 to 255.255.255.255.  However, you can optionally change the ports or which interface it 
listens on.  You can also change the broadcast ip and port.  These items are defined by default in
<code>C:\ProgramData\Aquila Technology\Agent\config.xml</code>

A config file might look like this
```xml
<?xml version="1.0" encoding="utf-8" ?>
<agent>
  <listen>
    <port>9</port>
    <address>192.168.0.8</address>
  </listen>
  <send>
    <port>9</port>
    <address>255.255.255.255</address>
  </send>
</agent>
```
