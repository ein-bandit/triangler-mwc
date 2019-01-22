const serverPort = "1234";
const serverAddress = window.location.hostname + ':' + serverPort;

var services = {
    deviceorientation: "deviceorientation"
};

const errorElement = document.getElementById('error');
const dataElement = document.getElementById('data');
const connectElement = document.getElementById('connect');


function initWebRTCConnection() {
    //rtc_main.js
    connect(serverAddress, setupDataChannelAndListeners);

    //connectElement.classList.add('hidden');
    dataElement.classList.remove('hidden');
}


function setupDataChannelAndListeners() {
    console.log("services:", services.deviceorientation);
    createLocalDataChannel();
    if (services.deviceorientation) {
        addDeviceOrientationListener();
    }
}

function addDeviceOrientationListener() {
    var data = document.getElementById('data');
    if (window.DeviceOrientationEvent) {
        window.addEventListener('deviceorientation', function(e) {
            var alpha = Math.floor(e.alpha);
            var beta = Math.floor(e.beta);
            var gamma = Math.floor(e.gamma);

            deviceData = {x:alpha, y:beta, z:gamma};

            data.innerHTML ='state: ' + dataChannel.readyState;
            if (dataChannel && dataChannel.readyState === 'open') {
                data.innerHTML = JSON.stringify(deviceData);
                dataChannel.send(JSON.stringify({type:'accelerometer', data: deviceData}));
            }
        }, function(error) {
            errorElement.innerHTML = error;
        });
    }
}