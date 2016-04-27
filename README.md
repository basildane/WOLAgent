# WOLAgent
Wake On LAN Agent.

Aquila WOL is designed for Wake-On-LAN over Internet.
WOL Agent is an optional (free) component that can be used with WOL to help with receiving wakeup messages over the Internet.
The challenge is usually that your firewall or router will not route broadcast-packets from the Internet to your internal LAN.
The WOL-Agent solves this problem.  The agent is a small service that you install on any available Windows server on your LAN.
It listens for WOL packets and then translates them into broadcast packets and sends them back into your LAN.
This solves the problem of firewalls and routers that cannot forward broadcast type messages.
