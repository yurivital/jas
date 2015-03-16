/**
 * Created by Yuri on 11/03/2015.
 *
*/
"use strict"

// Define a JasConsole constructor
// @canvasCtx :  Id of the canvasCtx of the chart viewport
// settings : optional, settings og the api
// return : none
var JasConsole = function(canvaContext , settings){

    console.log('Initializing JasConsole settings');
   this.canvasCtx = canvaContext;
   // paramètres par défaut
   this.settings ={
       jasApiScheme : "http",
       jasApiHost : "localhost",
       jasApiPort :8080,
       jasApiTemplate:  "/API/GetTemperatureRecord/{deviceId}/{dateStart}/{dateEnd}",
          };
    if(typeof(settings) != 'undefined')    {
        console.log('applying user defined setings');
        this.apply(this.settings, settings);
    }

    // Build the cart componant
    console.log('Initializing the charting engine')
    Chart.defaults.global.responsive = true;
    this.chart = new Chart(this.canvasCtx);

    // init the indexDb
    console.log('Initializing the browser db engine')
    window.indexedDB = window.indexedDB || window.mozIndexedDB || window.webkitIndexedDB || window.msIndexedDB;
    if(!window.indexedDB)
    {
        throw new JasConsoleException("Your browser doesn't support a stable version of IndexedDB. The cache will not be activated");
    }

    var request = window.indexedDB.open("JasConsole",1);

    // Return the url of the server API from inners settings
    this.jasApiUrl = function() {
        var url = this.settings.jasApiScheme + '://' + this.settings.jasApiHost;
        if (this.settings.jasApiScheme != 80) {
            url += ":" + this.settings.jasApiPort;
        }
        console.log('Jas API Url :'+ url);
        return url;
    };
};

// Set the start and the end of the displayed period
// @dateStart : the begining of the perdiod
// @dateEnd : the ending ot te period
// return : none
JasConsole.prototype.setDateRange = function (dateStart,dateEnd){
   console.log('Changing date range : ' + dateStart + ' - ' + dateEnd );
};


// Load the data for a device
// @devideId : the identity of the device
// return : none
JasConsole.prototype.loadTemperature = function(deviceId){
    console.log('Loading Temperature : ' + this.jasApiUrl());
};

// Build the data representation
JasConsole.prototype.display = function(){
    console.log('Refresh display')

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


function JasConsoleException(message)
{
    this.error = message;
}