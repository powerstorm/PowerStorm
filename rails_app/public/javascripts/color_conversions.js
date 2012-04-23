// functions useful for working with hex, RGB, and HSV color values
// taken from: http://matthaynes.net/blog/2008/08/07/javascript-colour-functions/


/** 
* Converts integer to a hexidecimal code, prepad's single digit hex codes with 0 to 
* always return a two digit code. 
* 
* @param {Integer} i Integer to convert 
* @returns {String} The hexidecimal code 
*/  
function intToHex(i) {  
    var hex = parseInt(i).toString(16);  
    return (hex.length < 2) ? "0" + hex : hex;  
} 

/** 
* Converts a hex number back to an integer, uses the javascript parseInt method 
* with a base number (or radix) of 16. EG: parseInt("0x" + hex); 
* 
* @param {String} hex The hex code to convert 
* @returns {Integer} The integer conversion of the hex number 
*/  
function hexToInt(hex) {  
    return parseInt("0x" + hex);  
}  

/**
* Converts an array of rgb values [red, green, blue] to a HTML hex value
*
* @param {Array} rgb_value The rgb array values to convert
* @returns {String} The html color hexadecimal string #xxxxxx
*/

function rgbToHex( rgb_value ) {
  var rgb_hex = rgb_value[2] | (rgb_value[1] << 8) | (rgb_value[0] << 16);
  var hex = '';
  
  var hex_string = rgb_hex.toString(16);
  //console.log( 'rgb_hex: ' + hex_string + ' Length: ' + hex_string.length );
  if ( hex_string.length % 2 != 0 ) {
    hex_string = '0' + hex_string;
  }
  
  if (rgb_hex <= 255) {
    hex = '#0000' + hex_string;
  } else if (rgb_hex <= 65535) {
    hex = '#00' + hex_string;
  } else {
    hex = '#' + hex_string;
  }
  
  return hex;  
}

/** 
* Converts HSV to RGB value. 
* 
* @param {Integer} h Hue as a value between 0 - 360 degrees 
* @param {Integer} s Saturation as a value between 0 - 100 % 
* @param {Integer} v Value as a value between 0 - 100 % 
* @returns {Array} The RGB values  EG: [r,g,b], [255,255,255] 
*/  
function hsvToRgb(h,s,v) {  
  
    var s = s / 100,  
         v = v / 100;  
  
    var hi = Math.floor((h/60) % 6);  
    var f = (h / 60) - hi;  
    var p = v * (1 - s);  
    var q = v * (1 - f * s);  
    var t = v * (1 - (1 - f) * s);  
  
    var rgb = [];  
  
    switch (hi) {  
        case 0: rgb = [v,t,p];break;  
        case 1: rgb = [q,v,p];break;  
        case 2: rgb = [p,v,t];break;  
        case 3: rgb = [p,q,v];break;  
        case 4: rgb = [t,p,v];break;  
        case 5: rgb = [v,p,q];break;  
    }  
  
    var r = Math.min(255, Math.round(rgb[0]*256)),  
        g = Math.min(255, Math.round(rgb[1]*256)),  
        b = Math.min(255, Math.round(rgb[2]*256));  
  
    return [r,g,b];  
  
}     
  
/** 
* Converts RGB to HSV value. 
* 
* @param {Integer} r Red value, 0-255 
* @param {Integer} g Green value, 0-255 
* @param {Integer} b Blue value, 0-255 
* @returns {Array} The HSV values EG: [h,s,v], [0-360 degrees, 0-100%, 0-100%] 
*/  
function rgbToHsv(r, g, b) {  
  
    var r = (r / 255),  
         g = (g / 255),  
     b = (b / 255);   
  
    var min = Math.min(Math.min(r, g), b),  
        max = Math.max(Math.max(r, g), b),  
        delta = max - min;  
  
    var value = max,  
        saturation,  
        hue;  
  
    // Hue  
    if (max == min) {  
        hue = 0;  
    } else if (max == r) {  
        hue = (60 * ((g-b) / (max-min))) % 360;  
    } else if (max == g) {  
        hue = 60 * ((b-r) / (max-min)) + 120;  
    } else if (max == b) {  
        hue = 60 * ((r-g) / (max-min)) + 240;  
    }  
  
    if (hue < 0) {  
        hue += 360;  
    }  
  
    // Saturation  
    if (max == 0) {  
        saturation = 0;  
    } else {  
        saturation = 1 - (min/max);  
    }  
  
    return [Math.round(hue), Math.round(saturation * 100), Math.round(value * 100)];  
}  
