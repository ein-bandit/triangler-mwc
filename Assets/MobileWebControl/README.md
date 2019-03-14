# Mobile Web Control

Mobile Web Control is a library offering an easy way to establish a WebRTC communication between web browsers and Unity.
It's focus lies on using web technology, especially HTML5 device APis, to create low latency and easy accessible game controllers.
However, you can use this library to to stream any kind of data between your Unity application and a mordern web browser.

Included in this pacakage is a HTTP weberver, a WebRTC server and a threadsafe event system implementation for receiving and sending low latency data messages.

## How to use it
Include this package in your Unity application.
_Implement a message interpreter (using the INetworkDataInterpreter interface) and extend the abstract MobileWebControlInitializer, which you put together with a (Event-)Dispatcher to your scene(s).
_Create a HTML Controller interface and a JavaScript configuration file to be able to use the HTML5 device APIs and communicate with your application.
More details can be found in the [Setup](#Setup) Section
A demo implementation can be found [here](https://bitbucket.org/kaufi07/mobile-web-control/src/master/). (Check out the Assets/Scripts/Network/ folder)

## Implementation details
multiple threads (each webrtc client 1, websocket 1)
unitytoolbag dispatcher to wait for unitys main thread to be ready and pass events then.
extended frontend impl from webrtc.net 


## Setup
Firstly you need to create an implementation of the abstract MobileWebControlInitializer which is the entry point for network communication.
Internally your implementation will be a singleton of Unitys Monobehaviour (DontDestroyOnLoad) and will call the clean up methods of used server implementations automatically when unity your application is shutting down.
Preferably you can use Unitys Start method to initialize the MobileWebControl but you can also call the MobileWebControl instance any other point of time which fits for your application.
Keep in mind that internally a webserver and a websocket are fired up, so this may consume a little performance.

```c#
class YourMobileWebControlInitializer : MobileWebControlInitializer {
    void Start() {
        Initialize();
    }
    void Initialize() {
        MobileWebController.Instance.Initialize(
            new YourNetworkDataInterpreter();
        );
    }
}
```

For your implementation you will need a network data interpreter which will parse the message data sent from the frontend.

You can get some inspiration from [this](https://bitbucket.org/kaufi07/mobile-web-control/src/master/Assets/Scripts/Network/MyNetworkDataInterpreter.cs) implementation inside the demo project.
You are free to use whatever code you need from there but coming up with your very own approach is perfectly fine as well.

Finally create a GameObject on your entry scene and add YourMobileWebControlInitializer as well as a Dispatcher from the UnityToolbag to it.
Your Initializer will expose some Properties to the Unity inspector allowing you to specify webserver and webRTC ports as well as the directory you placed your web resources.
The default webserver implementation will serve your files first but if not present, a file with the same name from the libraries web resources may be served.
You can use your Initializer in all scenes, since it is a (Unity) singleton the servers will be instantiated only once.

a screenshot of how it is added to unity.

*** Further customization ***
If you are willing to use your own Webserver and/or WebRTC implementation there is no need to inherit from MobileWebControlInitializer. You can call the static MobileWebController instance from anywhere else in your applcaition.

If no custom implementatons are provided the default servers will be fired up.
To use a custom servers you need to:
 - implement the IWebServer interface 
 - implement the IWebRTC interface
 - Pass instances of those inplmentations to the MobileWebController

If you decide to use your own WebRTC implementation bear in mind that you may also need to adapt or extend parts of the frontend webrtc code.


### Included Libraries

* [Flex](https://github.com/statianzo/Fleck) - C# WebSocket implementation for the WebRTC signaling process (MIT License)
* [LitJson](https://github.com/LitJSON/litjson) - JSON intepreter and converter for C# (UniLicense)
* [WebRTC.NET](https://github.com/radioman/WebRtc.NET) - WebRTC implementation for C# (MIT License)


# Support
You can always open an issue including a detailed description and steps to reproduce.
If you like this lib consider buying me a coffee :)
[crypto address tba]

License
----

MIT


***Free Software, Hell Yeah!***

