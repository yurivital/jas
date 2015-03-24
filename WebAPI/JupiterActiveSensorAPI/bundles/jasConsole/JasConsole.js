/**
 * Created by Yuri on 11/03/2015.
 *
 */
"use strict";

// Version of the lib
var JasConsoleVersion = "1.0.0";

// Define a JasConsole constructor
// @canvasCtx :  Id of the canvasCtx of the chart viewport
// settings : optional, settings og the api
// return : none
var JasConsole = function (canvaContext, settings) {

    // Filters parameters
    // the begining of the period to display
    this.dateStart = new Date();
    // the end of the period to display
    this.dateEnd = new Date();
    // The device identity
    this.deviceId = null;

    console.log('Initializing JasConsole settings');
    this.canvasCtx = canvaContext;
    // default parameters
    this.settings = {
        // protocol RL schme
        jasApiScheme: "http",
        // host of the API
        jasApiHost: "localhost",
        // port number of the hist
        jasApiPort: 8080,
        // template of the Get verb
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

    // compute the key from filter parameters
    // Return a string representing the key
    this.computeKey = function()
    {
        return  this.deviceId + this.toJasString(  this.dateStart) + this.toJasString( this.dateEnd);
    };

    // Return the url of the server API from inners settings
    this.jasApiUrl = function () {
        var url = this.settings.jasApiScheme + '://' + this.settings.jasApiHost;
        if (this.settings.jasApiScheme != 80) {
            url += ":" + this.settings.jasApiPort;
        }
        console.log('Jas API Url :' + url);
        return url;
    };

    /// return of valorised url ressource
    // @deviceid : String of device Identifier
    // @datestart : date of the begining period
    // @dateend : date of the ending period
    // return : string
    this.buildJasApiAction = function (deviceId, dateStart, dateEnd) {
        var tmpl = this.settings.jasApiTemplateGet;
        tmpl = tmpl.replace('{deviceId}', deviceId);
        tmpl = tmpl.replace('{dateStart}', this.toJasString(dateStart));
        tmpl = tmpl.replace('{dateEnd}', this.toJasString(dateEnd));
        return tmpl;
    };

  // Handler of httprequest changing states
    this.httpRequest.onreadystatechange = function () {

        if (this.readyState == 4) {
            if (this.status == 200) {
                console.log("Data Received");
                var message = JSON.parse(this.response);
                console.log(message);

                if (message.status == 'OK') {
                    var key = this.jasApi.computeKey();
                    window.localStorage.setItem(key, JSON.stringify(  message));
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
    this.deviceId = deviceId;

    var key = this.computeKey();
    var message = localStorage.getItem(key);
    if(message != null)
    {
        console.log('Loading Temperature from cache');
        message = JSON.parse(message);
        console.log(message);
        return this.display(message);
    }

    var targetUrl = this.jasApiUrl() + this.buildJasApiAction(deviceId, this.dateStart, this.dateEnd);
    console.log('Loading Temperature from server : ' + targetUrl);
    this.httpRequest.open('GET', targetUrl, true);
    this.httpRequest.send();
};


// Build the data representation
JasConsole.prototype.display = function (message) {
    console.log('Refresh display');

    var temps = message.temperatures;

    var data= new Object();
    data.labels = [];
    data.datasets = [
        {
            label : this.deviceId,
            fillColor: "rgba(151,187,205,0.2)",
            strokeColor: "rgba(151,187,205,1)",
            pointColor: "rgba(151,187,205,1)",
            pointStrokeColor: "#fff",
            pointHighlightFill: "#fff",
            pointHighlightStroke: "rgba(151,187,205,1)",
            data : []
        }
    ];

    for(var i in temps){
       var temp = temps[i];
       console.log(temp);
       data.labels.push(temp.timestamp);
       data.datasets[0].data.push(temp.temperature);
        // Trier les données par sensor Id
    }
    console.log(data);
    this.chart.Line(data);
   /*
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
    */

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
