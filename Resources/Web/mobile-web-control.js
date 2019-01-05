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
    connect(serverAddress, setupChannelsAndListeners);

    connectElement.classList.add('hidden');
    dataElement.classList.remove('hidden');
}


function setupChannelsAndListeners() {
    console.log("services:", services.deviceorientation);
    if (services.deviceorientation) {
        createLocalDataChannel(services.deviceorientation);
        setTimeout(function() {
            addDeviceOrientationListener();
        },2000);
    }
}

function addDeviceOrientationListener() {
    var data = document.getElementById('data');
    if (window.DeviceOrientationEvent) {
        window.addEventListener('deviceorientation', function(e) {
            var alpha = Math.floor(e.alpha);
            var beta = Math.floor(e.beta);
            var gamma = Math.floor(e.gamma);

            deviceData = "".concat(alpha, ',', beta, ',', gamma);

            console.log(channels[services.deviceorientation].readyState);
            if (channels && channels[services.deviceorientation]) {
                data.innerHTML = deviceData + " " + channels[services.deviceorientation].re;
                channels[services.deviceorientation].send(deviceData);
            }
        }, function(error) {
            errorElement.innerHTML = error;
        });
    }
}