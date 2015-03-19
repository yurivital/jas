/**
 * Created by Yuri on 11/03/2015.
 *
 */
"use strict";

// Define a JasConsole constructor
// @canvasCtx :  Id of the canvasCtx of the chart viewport
// settings : optional, settings og the api
// return : none
var JasConsole = function (canvaContext, settings) {

    this.dateStart = new Date();
    this.dateEnd = new Date();

    console.log('Initializing JasConsole settings');
    this.canvasCtx = canvaContext;
    // paramètres par défaut
    this.settings = {
        jasApiScheme: "http",
        jasApiHost: "localhost",
        jasApiPort: 8080,
        jasApiTemplateGet: "/API/GetTemperatureRecord/{deviceId}/{dateStart}/{dateEnd}"
    };
    if (typeof(settings) != 'undefined') {
        console.log('applying user defined setings');
        this.apply(this.settings, settings);
    }

    // Build the cart componant
    console.log('Initializing the charting engine');
    Chart.defaults.global.responsive = true;
    this.chart = new Chart(this.canvasCtx);

    // init the indexDb
    console.log('Initializing the browser db engine');
    window.indexedDB = window.indexedDB || window.mozIndexedDB || window.webkitIndexedDB || window.msIndexedDB;
    if (!window.indexedDB) {
        throw new JasConsoleException("Your browser doesn't support a stable version of IndexedDB. The cache will not be activated");
    }

    var request = window.indexedDB.open("JasConsole", 1);

    // Init the HttpRequest handler
    console.log('Initializing the ajax client');
    if (window.XMLHttpRequest) { // Mozilla, Safari, ...
        this.httpRequest = new XMLHttpRequest();
    }
    else if (window.ActiveXObject) { // IE
        try {
            this.httpRequest = new ActiveXObject("Microsoft.XMLHTTP");
        }
        catch (e) {
            try {
                this.httpRequest = new ActiveXObject("Msxml2.XMLHTTP");
            }
            catch (e) {
                console.log("Failed to load IE Ajax client");
            }
        }
    }

    if (this.httpRequest == null) {
        throw new JasConsoleException("Unable to load an Ajax client");
    }

    this.httpRequest.jasApi = this;

    // Return the url of the server API from inners settings
    this.jasApiUrl = function () {
        var url = this.settings.jasApiScheme + '://' + this.settings.jasApiHost;
        if (this.settings.jasApiScheme != 80) {
            url += ":" + this.settings.jasApiPort;
        }
        console.log('Jas API Url :' + url);
        return url;
    };

    this.buildJasApiAction = function (deviceId, dateStart, dateEnd) {
        var tmpl = this.settings.jasApiTemplateGet;
        tmpl = tmpl.replace('{deviceId}', deviceId);
        tmpl = tmpl.replace('{dateStart}', this.toJasString(dateStart));
        tmpl = tmpl.replace('{dateEnd}', this.toJasString(dateEnd));
        return tmpl;
    };


    this.httpRequest.onreadystatechange = function () {

        if (this.readyState == 4) {
            if (this.status == 200) {
                console.log("Data Received");
                var message = JSON.parse(this.response);
                console.log(message);

                if (message.status == 'OK') {
                    this.jasApi.display(message);
                }

            } else {
                throw new JasConsoleException('Unable to communicate with the JAS API server');
            }
        }
    };
};

// Set the start and the end of the displayed period
// @dateStart : the begining of the perdiod
// @dateEnd : the ending ot te period
// return : none
JasConsole.prototype.setDateRange = function (dateStart, dateEnd) {
    this.dateStart = new Date(dateStart);
    this.dateEnd = new Date(dateEnd);
    console.log('Changing date range : ' + this.toJasString(dateStart) + ' - ' + this.toJasString(dateEnd));
};


// Load the data for a device
// @devideId : the identity of the device
// return : none
JasConsole.prototype.loadTemperature = function (deviceId) {
    var targetUrl = this.jasApiUrl() + this.buildJasApiAction(deviceId, this.dateStart, this.dateEnd);
    console.log('Loading Temperature : ' + targetUrl);
    this.httpRequest.open('GET', targetUrl, true);
    this.httpRequest.send();
};


// Build the data representation
JasConsole.prototype.display = function (message) {
    console.log('Refresh display');

    var temps = message.temperatures;

    datasets = [];


    for(var i in temps){
        console.log(temps[i]);
        // Trier les données par sensor Id

    }

    console.log('Get data for the device and data range');
    var data = {
        labels: ["January", "February", "March", "April", "May", "June", "July"],
        datasets: [
            {
                label: "My First dataset",
                fillColor: "rgba(220,220,220,0.2)",
                strokeColor: "rgba(220,220,220,1)",
                pointColor: "rgba(220,220,220,1)",
                pointStrokeColor: "#fff",
                pointHighlightFill: "#fff",
                pointHighlightStroke: "rgba(220,220,220,1)",
                data: [65, 59, 80, 81, 56, 55, 40]
            },
            {
                label: "My Second dataset",
                fillColor: "rgba(151,187,205,0.2)",
                strokeColor: "rgba(151,187,205,1)",
                pointColor: "rgba(151,187,205,1)",
                pointStrokeColor: "#fff",
                pointHighlightFill: "#fff",
                pointHighlightStroke: "rgba(151,187,205,1)",
                data: [28, 48, 40, 19, 86, 27, 90]
            }
        ]
    };
    this.chart.Line(data);
};

// Represent an Exception thrown by JasConsole
function JasConsoleException(message) {
    this.error = message;
}

// Extend the Date for producing Jas Date format (YYYYMMDD)
JasConsole.prototype.toJasString = function (date) {
    date = new Date(date);
    var day = ("0" + date.getDate()).slice(-2);
    var month = ("0" + (date.getMonth() + 1)).slice(-2);
    return date.getFullYear() + (month) + (day);
};

// Extend the Date for producing HTML5 wire format (YYYY-MM-DD)
JasConsole.prototype.toWireString = function (date) {
    var day = ("0" + date.getDate()).slice(-2);
    var month = ("0" + (date.getMonth() + 1)).slice(-2);
    return date.getFullYear() + "-" + (month) + "-" + (day);
};
