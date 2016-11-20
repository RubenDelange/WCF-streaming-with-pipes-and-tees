WCF streaming with pipes and tees
=======================

Example/Playground application for forwarding data streams using WCF.

Components:

- Stream sender: client that sends a stream of large data (a file of several MB's)
	- file being sent is a .rar, .zip, .7zip, .tar archive containing multiple files
	
- Stream forwarder: service that receives a stream and forwards it to the receiver
	- create a pipe between forwarder and receiver + forward the incoming stream to the receiver via this pipe
	- save incoming stream as .rar file on the filesystem of the forwarder
	- create a pipe towards the filesystem of the forwarder + extract files from incoming .rar file on the filesystem of the forwarder
	< above actions are ASYNC >
	
- Stream receiver: service that receives a stream from the forwarder
	< via pipe from forwarder >
	- save incoming stream as .rar file on the filesystem of the receiver
	- create a pipe towards the filesystem of the receiver + extract files from incoming .rar file on the filesystem of the receiver
	< above actions are ASYNC >
